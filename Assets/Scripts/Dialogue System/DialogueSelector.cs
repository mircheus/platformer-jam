using UnityEngine;

public class DialogueSelector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestSystemController questSystem;
    [SerializeField] private DialogueSystem dialogueSystem;

    private void Start()
    {
        foreach(var quest in questSystem.GetAllQuests())
        {
            Debug.Log($"{quest.QuestName} | {quest.QuestID} | {quest.CurrentState} | TargetNPC = {quest.TargetNPC_ID}");
        }
    }

    public void SelectDialogue(string npcObjectID, NPCInteractable npc)
    {
        if (npc == null)
        {
            Debug.LogWarning("NPC is null");
            return;
        }

        QuestData activeQuest = GetActiveQuestForNPC(npcObjectID);

        DialogueData selectedDialogue = null;

        if (activeQuest != null)
        {
            Debug.Log($"Quest dialogue triggered for NPC: {npcObjectID}");
            selectedDialogue = npc.GetQuestDialogue();
        }
        else
        {
            Debug.Log($"Default dialogue triggered for NPC: {npcObjectID}");
            selectedDialogue = npc.GetDefaultDialogue();
        }

        if (selectedDialogue != null)
        {
            dialogueSystem.StartDialogue(selectedDialogue, npcObjectID);
        }
        else
        {
            Debug.LogWarning($"No dialogue found for NPC: {npcObjectID}");
        }
    }

    private QuestData GetActiveQuestForNPC(string npcObjectID)
    {
        foreach (var quest in questSystem.GetAllQuests())
        {
            if (quest.CurrentState != QuestState.Active)
                continue;

            if (quest.TargetNPC_ID == npcObjectID)
                return quest;
        }

        return null;
    }
}