using UnityEngine;

/// <summary>
/// Разовый скрипт-триггер: после завершения заданной мини-игры один раз
/// переключает Animator на новое idle-состояние и оставляет его так до конца.
/// Вешается ситуативно на любой объект сцены (обычно на самого NPC).
///
/// Намеренно изолирован от ядра: подписывается на обобщённое событие
/// MinigameSystemController.MinigameCompleted, фильтрует по нужной MinigameData
/// и зовёт Animator.Play. Контроллер мини-игр про эту связку ничего не знает.
///
/// Переключение «залипает», потому что у целевого состояния нет исходящих
/// переходов и на него не ведёт AnyState — назад его ничего не вернёт.
/// </summary>
public class OneShotMinigameIdleSwap : MonoBehaviour
{
    [Header("Trigger")]
    [Tooltip("Мини-игра, после завершения которой меняется idle-анимация.")]
    [SerializeField] private MinigameData triggerMinigame;

    [Header("Animation")]
    [Tooltip("Animator персонажа, у которого меняем idle.")]
    [SerializeField] private Animator animator;
    [Tooltip("Имя состояния Animator, на которое переключаемся (напр. Rabbit_Idle_Berushi).")]
    [SerializeField] private string idleStateName = "Rabbit_Idle_Berushi";
    [Tooltip("Индекс слоя Animator. Обычно 0 (Base Layer).")]
    [SerializeField] private int layer = 0;

    private bool hasSwapped;

    private void OnEnable()
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
    }

    private void OnMinigameCompleted(MinigameData completed)
    {
        if (hasSwapped)
        {
            return;
        }

        if (triggerMinigame == null || completed != triggerMinigame)
        {
            return;
        }

        if (animator == null)
        {
            Debug.LogWarning($"OneShotMinigameIdleSwap ('{name}'): triggerMinigame задан, но animator не назначен — нечего переключать.");
            return;
        }

        hasSwapped = true;

        // Отписываемся сразу: больше реагировать не на что (скрипт одноразовый).
        GameContext.Instance.MinigameSystem.MinigameCompleted -= OnMinigameCompleted;

        Debug.Log($"OneShotMinigameIdleSwap ('{name}'): мини-игра '{completed.id}' завершена — переключаем idle на '{idleStateName}'.");

        animator.Play(idleStateName, layer);
    }
}
