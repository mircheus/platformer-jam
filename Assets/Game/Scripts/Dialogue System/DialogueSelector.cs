using UnityEngine;

public class DialogueSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueSystem dialogueSystem;

    public void SelectDialogue(NPCInteractable npc)
    {
        if (npc == null)
        {
            Debug.LogWarning("NPC is null");
            return;
        }

        DialogueData dialogue = npc.GetCurrentDialogue();

        if (dialogue == null)
        {
            Debug.LogWarning($"No dialogue for NPC: {npc.ObjectID} | progress: {npc.Progress}");
            return;
        }

        Debug.Log($"Dialogue triggered for NPC: {npc.ObjectID} | progress: {npc.Progress}");

        dialogueSystem.StartDialogue(dialogue);
    }

    public void SelectDialogue(DialogueData dialogue)
    {
        if (dialogue == null)
        {
            Debug.LogWarning("Dialogue is null");
            return;
        }

        dialogueSystem.StartDialogue(dialogue);
    }
}
