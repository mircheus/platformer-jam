using UnityEngine;

/// <summary>
/// Разовый скрипт-триггер: после завершения заданной мини-игры один раз
/// запускает диалог. Вешается ситуативно на любой объект сцены.
///
/// Намеренно изолирован от ядра: подписывается на обобщённое событие
/// MinigameSystemController.MinigameCompleted, фильтрует по нужной MinigameData
/// и зовёт DialogueSelector. Контроллер мини-игр про эту связку ничего не знает.
/// </summary>
public class OneShotMinigameDialogue : MonoBehaviour
{
    [Header("Trigger")]
    [Tooltip("Мини-игра, после завершения которой запускается диалог.")]
    [SerializeField] private MinigameData triggerMinigame;

    [Header("Dialogue")]
    [Tooltip("Диалог, который проигрывается один раз после завершения мини-игры.")]
    [SerializeField] private DialogueData dialogue;
    [Tooltip("Нужен, чтобы поймать момент завершения диалога для дропа. Тот же DialogueSystem, что и в сцене.")]
    [SerializeField] private DialogueSystem dialogueSystem;

    [Header("Item Drop After Dialogue (optional)")]
    [Tooltip("Предмет, который выпадает после того, как запущенный диалог завершится. Если dropPickup не задан — дроп выключен.")]
    [SerializeField] private DialogueEndItemDrop drop;

    private bool hasPlayed;

    private void Start()
    {
        if (GameContext.Instance != null && GameContext.Instance.MinigameSystem != null)
        {
            GameContext.Instance.MinigameSystem.MinigameCompleted += OnMinigameCompleted;
        }
    }

    private void OnDisable()
    {
        if (GameContext.Instance != null && GameContext.Instance.MinigameSystem != null)
        {
            GameContext.Instance.MinigameSystem.MinigameCompleted -= OnMinigameCompleted;
        }

        drop.Cancel();
    }

    private void OnMinigameCompleted(MinigameData completed)
    {
        if (hasPlayed)
        {
            return;
        }

        if (triggerMinigame == null || completed != triggerMinigame)
        {
            return;
        }

        if (dialogue == null)
        {
            Debug.LogWarning($"OneShotMinigameDialogue ('{name}'): triggerMinigame задан, но dialogue не назначен — нечего проигрывать.");
            return;
        }

        hasPlayed = true;

        // Отписываемся сразу: больше реагировать не на что (скрипт одноразовый).
        GameContext.Instance.MinigameSystem.MinigameCompleted -= OnMinigameCompleted;

        Debug.Log($"OneShotMinigameDialogue ('{name}'): мини-игра '{completed.id}' завершена — запускаем диалог.");

        // Армим дроп ДО запуска диалога: игрок на время диалога заблокирован,
        // поэтому ближайший DialogueEnded — гарантированно этот диалог.
        drop.Arm(dialogueSystem);

        GameContext.Instance.DialogueSelector.SelectDialogue(dialogue);
    }
}
