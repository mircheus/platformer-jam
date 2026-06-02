using System.Collections;
using UnityEngine;

/// <summary>
/// Разовая скриптовая сцена на объекте NPC: при первом прибытии игрока на
/// заданную локацию блокирует игрока, проигрывает диалог NPC, после чего уводит
/// NPC к точке выхода движением и выключает его. Срабатывает один раз.
///
/// Намеренно изолирована от основных систем: подписывается на обобщённые
/// события (LocationTransitionController.ArrivedAtLocation,
/// DialogueSystem.DialogueEnded) и сама оркеструет последовательность. Ядро про
/// эту сцену ничего не знает.
/// </summary>
public class OneShotArrivalCutscene : MonoBehaviour
{
    [Header("Trigger")]
    [Tooltip("LocationId локации, прибытие на которую запускает сцену.")]
    [SerializeField] private string triggerLocationId;

    [Header("Dialogue")]
    [SerializeField] private DialogueSystem dialogueSystem;
    [Tooltip("Диалог NPC. Если пусто — диалог пропускается, NPC сразу уходит.")]
    [SerializeField] private DialogueData dialogue;

    [Header("Leave Movement")]
    [Tooltip("Куда уходит NPC. Если пусто — NPC просто выключается без движения.")]
    [SerializeField] private Transform exitPoint;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float arriveThreshold = 0.05f;

    [Header("Walk Animation (optional)")]
    [SerializeField] private Animator animator;
    [Tooltip("Имя bool-параметра аниматора, включающего анимацию ходьбы. Пусто — не трогаем аниматор.")]
    [SerializeField] private string walkBoolParam;

    private bool hasPlayed;
    private bool waitingForDialogueEnd;

    private void Start()
    {
        if (GameContext.Instance != null && GameContext.Instance.LocationTransition != null)
        {
            GameContext.Instance.LocationTransition.ArrivedAtLocation += OnArrived;
        }
    }

    private void OnDisable()
    {
        if (GameContext.Instance != null && GameContext.Instance.LocationTransition != null)
        {
            GameContext.Instance.LocationTransition.ArrivedAtLocation -= OnArrived;
        }

        if (dialogueSystem != null)
        {
            dialogueSystem.DialogueEnded -= OnDialogueEnded;
        }
    }

    private void OnArrived(Location location)
    {
        if (hasPlayed)
        {
            return;
        }
        
        if (location == null || location.LocationId != triggerLocationId)
        {
            return;
        }
        
        hasPlayed = true;
        SetPlayerBlocked(true);
        
        // Нет диалога — сразу уводим NPC.
        if (dialogueSystem == null || dialogue == null)
        {
            StartCoroutine(LeaveRoutine());
            return;
        }

        waitingForDialogueEnd = true;
        dialogueSystem.DialogueEnded += OnDialogueEnded;
        dialogueSystem.StartDialogue(dialogue);
    }

    private void OnDialogueEnded(DialogueData dialogue)
    {
        if (!waitingForDialogueEnd)
        {
            return;
        }

        waitingForDialogueEnd = false;
        dialogueSystem.DialogueEnded -= OnDialogueEnded;

        // DialogueSystem.EndDialogue вернул управление игроку — снова блокируем,
        // пока NPC уходит.
        SetPlayerBlocked(true);

        StartCoroutine(LeaveRoutine());
    }

    private IEnumerator LeaveRoutine()
    {
        SetWalking(true);

        if (exitPoint != null)
        {
            while (Vector2.Distance(transform.position, exitPoint.position) > arriveThreshold)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    exitPoint.position,
                    moveSpeed * Time.deltaTime);

                yield return null;
            }
        }

        SetWalking(false);

        // Сначала возвращаем управление, затем выключаем NPC (после SetActive(false)
        // эта корутина прервётся, поэтому unblock — до него).
        SetPlayerBlocked(false);
        gameObject.SetActive(false);
    }

    private void SetWalking(bool walking)
    {
        if (animator != null && !string.IsNullOrEmpty(walkBoolParam))
        {
            animator.SetBool(walkBoolParam, walking);
        }
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
