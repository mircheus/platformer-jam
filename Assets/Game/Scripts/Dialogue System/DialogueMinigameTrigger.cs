using UnityEngine;

/// <summary>
/// Хост-адаптер (шов) между диалогом и мини-играми. Когда диалог встаёт на
/// паузу после реплики с триггером, запускает привязанную к этой реплике
/// мини-игру и по её завершении продолжает диалог (Resume).
///
/// Так DialogueSystem остаётся «глупым» (про мини-игры не знает), а
/// направление зависимостей не нарушается: мост — здесь, в хосте.
/// </summary>
public class DialogueMinigameTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueSystem dialogueSystem;

    private void OnEnable()
    {
        if (dialogueSystem != null)
        {
            dialogueSystem.DialoguePaused += OnDialoguePaused;
        }
    }

    private void OnDisable()
    {
        if (dialogueSystem != null)
        {
            dialogueSystem.DialoguePaused -= OnDialoguePaused;
        }
    }

    private void OnDialoguePaused(DialogueData dialogue, int lineIndex)
    {
        MinigameData minigame = dialogue.lines[lineIndex].minigameAfter;

        // Нет мини-игры (или не удалось запустить) — не оставляем диалог
        // висеть на паузе, сразу продолжаем.
        if (minigame == null)
        {
            dialogueSystem.Resume();
            return;
        }

        Debug.Log($"Dialogue '{dialogue.dialogueID}' paused at line {lineIndex}, launching minigame: {minigame.id}");

        bool started = GameContext.Instance.MinigameSystem.Launch(minigame, dialogueSystem.Resume);

        if (!started)
        {
            dialogueSystem.Resume();
        }
    }
}
