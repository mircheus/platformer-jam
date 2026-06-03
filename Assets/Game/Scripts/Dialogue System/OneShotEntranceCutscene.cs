using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Разовая скриптовая сцена: после завершения заданного диалога блокирует игрока,
/// вводит в кадр персонажа (включает его и доводит движением до точки входа), после
/// чего запускает с ним диалог. Опционально после этого диалога роняет предмет.
/// Срабатывает один раз.
///
/// В стиле <see cref="OneShotArrivalCutscene"/>: вешается ситуативно на отдельный
/// (всегда активный) объект-оркестратор, подписывается на обобщённое событие
/// DialogueSystem.DialogueEnded, фильтрует по нужному диалогу и сама оркеструет
/// последовательность. Ядро про эту сцену ничего не знает.
///
/// Персонаж входа держится отдельной ссылкой и может лежать в сцене выключенным:
/// скрипт нельзя вешать на него самого, т.к. на выключенном объекте Start не
/// вызовется и подписка не произойдёт.
/// </summary>
public class OneShotEntranceCutscene : MonoBehaviour
{
    [Header("Trigger")]
    [Tooltip("Диалог, после ЗАВЕРШЕНИЯ которого запускается катсцена входа.")]
    [SerializeField] private DialogueData triggerDialogue;
    [SerializeField] private DialogueSystem dialogueSystem;

    [Header("Entering Character")]
    [Tooltip("Входящий персонаж. Можно держать выключенным — будет включён в начале катсцены.")]
    [SerializeField] private Transform character;
    [Tooltip("Куда входит персонаж. Если пусто — персонаж просто включается без движения.")]
    [SerializeField] private Transform entryPoint;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float arriveThreshold = 0.05f;

    [Header("Timing")]
    [Tooltip("Пауза перед появлением персонажа (после завершения триггер-диалога).")]
    [SerializeField] private float delayBeforeEntrance = 0.5f;
    [Tooltip("Пауза после того, как персонаж дошёл, перед запуском диалога с ним.")]
    [SerializeField] private float delayAfterEntrance = 0.5f;

    [Header("Walk Animation (optional)")]
    [SerializeField] private Animator animator;
    [Tooltip("Имя bool-параметра аниматора, включающего анимацию ходьбы. Пусто — не трогаем аниматор.")]
    [SerializeField] private string walkBoolParam;

    [Header("Facing (optional)")]
    [Tooltip("SpriteRenderer персонажа для разворота по направлению движения. Пусто — флип выключен.")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("Куда смотрит спрайт БЕЗ флипа (flipX = false). Нужно, чтобы разворот шёл в правильную сторону.")]
    [SerializeField] private bool spriteFacesRight = true;

    [Header("Dialogue After Entrance")]
    [Tooltip("Диалог, который запускается после входа персонажа. Если пусто — катсцена просто вернёт управление игроку.")]
    [SerializeField] private DialogueData entranceDialogue;

    [Header("Item Drop After Dialogue (optional)")]
    [Tooltip("Предмет, который выпадает после завершения диалога входа. Если dropPickup не задан — дроп выключен.")]
    [SerializeField] private DialogueEndItemDrop drop;

    private bool hasPlayed;
    private bool waitingForTrigger;

    private void OnEnable()
    {
        if (dialogueSystem != null)
        {
            dialogueSystem.DialogueEnded += OnDialogueEnded;
            waitingForTrigger = true;
        }
        else
        {
            Debug.LogWarning($"OneShotEntranceCutscene ('{name}'): dialogueSystem не назначен — катсцена не запустится.");
        }
    }

    private void OnDisable()
    {
        if (waitingForTrigger && dialogueSystem != null)
        {
            dialogueSystem.DialogueEnded -= OnDialogueEnded;
            waitingForTrigger = false;
        }

        drop.Cancel();
    }

    private void OnDialogueEnded(DialogueData dialogue)
    {
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

        // Отписываемся сразу: триггер одноразовый, и чтобы не поймать завершение
        // запускаемого ниже диалога входа.
        dialogueSystem.DialogueEnded -= OnDialogueEnded;
        waitingForTrigger = false;

        // EndDialogue только что вернул управление игроку — сразу блокируем снова,
        // пока персонаж входит в кадр.
        SetPlayerBlocked(true);

        StartCoroutine(EntranceRoutine());
    }

    private IEnumerator EntranceRoutine()
    {
        if (delayBeforeEntrance > 0f)
        {
            yield return new WaitForSeconds(delayBeforeEntrance);
        }

        if (character != null)
        {
            character.gameObject.SetActive(true);
        }

        SetWalking(true);

        if (character != null && entryPoint != null)
        {
            FaceTowards(entryPoint.position.x);

            while (Vector2.Distance(character.position, entryPoint.position) > arriveThreshold)
            {
                character.position = Vector2.MoveTowards(
                    character.position,
                    entryPoint.position,
                    moveSpeed * Time.deltaTime);

                yield return null;
            }
        }

        SetWalking(false);

        if (delayAfterEntrance > 0f)
        {
            yield return new WaitForSeconds(delayAfterEntrance);
        }

        if (entranceDialogue == null)
        {
            Debug.LogWarning($"OneShotEntranceCutscene ('{name}'): entranceDialogue не назначен — возвращаю управление игроку.");
            SetPlayerBlocked(false);
            yield break;
        }

        // Полностью снимаем СВОЮ блокировку перед передачей управления диалогу.
        // Важно: SetPlayerBlocked отключал и PlayerMovementController.canInteract,
        // который DialogueSystem по завершении диалога обратно НЕ включает (он
        // управляет только движением и InteractionSystemController). Если не снять
        // здесь — после диалога входа движение вернётся, а взаимодействие нет.
        // StartDialogue ниже синхронно заблокирует своё подмножество заново.
        SetPlayerBlocked(false);

        // Армим дроп ДО запуска диалога: игрок на время диалога заблокирован,
        // поэтому ближайший DialogueEnded — гарантированно этот диалог.
        // Управление игроку вернёт сам DialogueSystem по завершении диалога входа.
        drop.Arm(dialogueSystem);

        GameContext.Instance.DialogueSelector.SelectDialogue(entranceDialogue);
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
