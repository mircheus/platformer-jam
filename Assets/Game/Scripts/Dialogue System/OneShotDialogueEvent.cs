using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Разовая скриптовая сцена: после ЗАВЕРШЕНИЯ заданного диалога вызывает UnityEvent.
/// Срабатывает один раз.
///
/// В стиле <see cref="OneShotDepartureCutscene"/> / <see cref="OneShotEntranceCutscene"/>:
/// вешается ситуативно на отдельный (всегда активный) объект-оркестратор,
/// подписывается на обобщённое событие DialogueSystem.DialogueEnded, фильтрует по
/// нужному диалогу (сравнение ссылок на ScriptableObject) и дёргает onTriggered.
/// Ядро про эту сцену ничего не знает.
/// </summary>
public class OneShotDialogueEvent : MonoBehaviour
{
    [Header("Trigger")]
    [Tooltip("Диалог, после ЗАВЕРШЕНИЯ которого вызывается событие.")]
    [SerializeField] private DialogueData triggerDialogue;
    [SerializeField] private DialogueSystem dialogueSystem;

    [Header("On Triggered")]
    [Tooltip("Вызывается один раз, когда триггер-диалог завершился.")]
    [SerializeField] private UnityEvent onTriggered;

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
            Debug.LogWarning($"OneShotDialogueEvent ('{name}'): dialogueSystem не назначен — событие не сработает.");
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
        if (hasPlayed)
        {
            return;
        }

        // Фильтруем по нужному диалогу (сравнение ссылок на ScriptableObject).
        if (triggerDialogue == null || dialogue != triggerDialogue)
        {
            return;
        }

        hasPlayed = true;

        // Триггер одноразовый — отписываемся сразу.
        dialogueSystem.DialogueEnded -= OnDialogueEnded;
        waitingForTrigger = false;

        onTriggered?.Invoke();
    }
}
