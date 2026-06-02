using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Разовая скриптовая сцена: после завершения заданного диалога блокирует игрока,
/// уводит персонажа к точке выхода движением и опционально выключает его.
/// Срабатывает один раз.
///
/// В стиле <see cref="OneShotArrivalCutscene"/> / <see cref="OneShotEntranceCutscene"/>:
/// вешается ситуативно на отдельный (всегда активный) объект-оркестратор,
/// подписывается на обобщённое событие DialogueSystem.DialogueEnded, фильтрует по
/// нужному диалогу и сама оркеструет уход. Ядро про эту сцену ничего не знает.
///
/// Уходящий персонаж — отдельная ссылка и НЕ обязан совпадать с тем, кто вошёл в
/// <see cref="OneShotEntranceCutscene"/>: уйти может любой персонаж сцены.
/// </summary>
public class OneShotDepartureCutscene : MonoBehaviour
{
    [Header("Trigger")]
    [Tooltip("Диалог, после ЗАВЕРШЕНИЯ которого персонаж уходит.")]
    [SerializeField] private DialogueData triggerDialogue;
    [SerializeField] private DialogueSystem dialogueSystem;

    [Header("Departing Character")]
    [Tooltip("Уходящий персонаж (может отличаться от того, кто вошёл).")]
    [SerializeField] private Transform character;
    [Tooltip("Куда уходит персонаж. Если пусто — персонаж уходит без движения (сразу выключается, если включён deactivateAfterLeave).")]
    [SerializeField] private Transform exitPoint;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float arriveThreshold = 0.05f;
    [Tooltip("Выключать ли персонажа (SetActive(false)) после того, как он дошёл до точки выхода.")]
    [SerializeField] private bool deactivateAfterLeave = true;

    [Header("Timing")]
    [Tooltip("Пауза после завершения диалога перед тем, как персонаж пойдёт на выход.")]
    [SerializeField] private float delayBeforeLeave = 0.5f;

    [Header("Walk Animation (optional)")]
    [SerializeField] private Animator animator;
    [Tooltip("Имя bool-параметра аниматора, включающего анимацию ходьбы. Пусто — не трогаем аниматор.")]
    [SerializeField] private string walkBoolParam;

    [Header("Facing (optional)")]
    [Tooltip("SpriteRenderer персонажа для разворота по направлению движения. Пусто — флип выключен.")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("Куда смотрит спрайт БЕЗ флипа (flipX = false). Нужно, чтобы разворот шёл в правильную сторону.")]
    [SerializeField] private bool spriteFacesRight = true;
    
    [Header("Disabling Player controls")]
    [SerializeField] private bool disablePlayerControls = false;

    [Header("On Finished")]
    [Tooltip("Вызывается в самом конце — после ухода персонажа и возврата управления игроку.")]
    [SerializeField] private UnityEvent onStarted;
    [SerializeField] private UnityEvent onFinished;

    private bool hasPlayed;
    private bool waitingForTrigger;

    private void OnEnable()
    {
        // Одноразовый триггер уже отработал — повторно не подписываемся.
        if (hasPlayed)
        {
            return;
        }

        if (dialogueSystem != null)
        {
            dialogueSystem.DialogueEnded += OnDialogueEnded;
            waitingForTrigger = true;
        }
        else
        {
            Debug.LogWarning($"OneShotDepartureCutscene ('{name}'): dialogueSystem не назначен — катсцена не запустится.");
        }
    }

    private void OnDisable()
    {
        if (waitingForTrigger && dialogueSystem != null)
        {
            dialogueSystem.DialogueEnded -= OnDialogueEnded;
            waitingForTrigger = false;
        }
    }

    private void OnDialogueEnded(DialogueData dialogue)
    {
        Debug.Log($"{gameObject.name} HasPlayed: {hasPlayed}");
        if (hasPlayed)
        {
            return;
        }

        // Развилка A: фильтруем по нужному диалогу (сравнение ссылок на ScriptableObject).
        if (triggerDialogue == null || dialogue != triggerDialogue)
        {
            return;
        }

        hasPlayed = true;

        // Триггер одноразовый — отписываемся сразу.
        dialogueSystem.DialogueEnded -= OnDialogueEnded;
        waitingForTrigger = false;
        
        if(disablePlayerControls)
            SetPlayerBlocked(true);

        StartCoroutine(LeaveRoutine());
    }

    private IEnumerator LeaveRoutine()
    {
        onStarted?.Invoke();
        
        if (delayBeforeLeave > 0f)
        {
            yield return new WaitForSeconds(delayBeforeLeave);
        }

        SetWalking(true);

        if (character != null && exitPoint != null)
        {
            FaceTowards(exitPoint.position.x);

            while (Vector2.Distance(character.position, exitPoint.position) > arriveThreshold)
            {
                character.position = Vector2.MoveTowards(
                    character.position,
                    exitPoint.position,
                    moveSpeed * Time.deltaTime);

                yield return null;
            }
        }

        SetWalking(false);
        
        if(disablePlayerControls)
            SetPlayerBlocked(false);

        if (deactivateAfterLeave && character != null)
        {
            character.gameObject.SetActive(false);
        }

        // Скрипт отработал, управление уже возвращено игроку — дёргаем внешние реакции.
        onFinished?.Invoke();
    }

    private void SetWalking(bool walking)
    {
        if (animator != null && !string.IsNullOrEmpty(walkBoolParam))
        {
            animator.SetBool(walkBoolParam, walking);
        }
    }

    private void FaceTowards(float targetX)
    {
        if (spriteRenderer == null || character == null)
        {
            return;
        }

        float dx = targetX - character.position.x;

        // Пренебрежимо малое смещение — не трогаем флип (персонаж уже у цели).
        if (Mathf.Abs(dx) < 0.0001f)
        {
            return;
        }

        bool movingRight = dx > 0f;
        spriteRenderer.flipX = movingRight != spriteFacesRight;
    }

    private void SetPlayerBlocked(bool blocked)
    {
        if (GameContext.Instance == null)
        {
            return;
        }

        GameObject player = GameContext.Instance.Player;

        if (player != null)
        {
            PlayerMovementController movement = player.GetComponent<PlayerMovementController>();

            if (movement != null)
            {
                if (blocked)
                {
                    movement.DisableMovement();
                    movement.DisableInteraction();
                }
                else
                {
                    movement.EnableMovement();
                    movement.EnableInteraction();
                }
            }
        }

        InteractionSystemController interaction = GameContext.Instance.InteractionSystem;

        if (interaction != null)
        {
            if (blocked)
            {
                interaction.DisableInteraction();
            }
            else
            {
                interaction.EnableInteraction();
            }
        }
    }
}
