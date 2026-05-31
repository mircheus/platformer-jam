using UnityEngine;

/// <summary>
/// Хост-адаптер (шов) между диалогом и мини-играми. Слушает завершение
/// диалога и, если у долистанного набора задана мини-игра, просит
/// MinigameSystem её запустить.
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
            dialogueSystem.DialogueEnded += OnDialogueEnded;
        }
    }

    private void OnDisable()
    {
        if (dialogueSystem != null)
        {
            dialogueSystem.DialogueEnded -= OnDialogueEnded;
        }
    }

    private void OnDialogueEnded(DialogueData dialogue)
    {
        if (dialogue == null || dialogue.minigameOnComplete == null)
        {
            return;
        }

        Debug.Log($"Dialogue '{dialogue.dialogueID}' finished, launching minigame: {dialogue.minigameOnComplete.id}");

        GameContext.Instance.MinigameSystem.Launch(dialogue.minigameOnComplete);
    }
}
