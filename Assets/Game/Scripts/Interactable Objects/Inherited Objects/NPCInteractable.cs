using System;
using UnityEngine;

public class NPCInteractable : IInteractable
{
    [Header("NPC Data")]
    [SerializeField] private DialogueData questDialogue;
    [SerializeField] private DialogueData defaultDialogue;

    public DialogueData GetDefaultDialogue() { return defaultDialogue; }

    public DialogueData GetQuestDialogue() { return questDialogue; }

    protected override void OnInteract()
    {
        Debug.Log($"Передаем NPC диалоговой системе | ObjectID : {ObjectID}");

        var questSystem = FindFirstObjectByType<QuestSystemController>();
        var dialogueSelector = FindFirstObjectByType<DialogueSelector>();

        //if (questSystem != null)
        //    questSystem.TryCompleteQuestByNPC(ObjectID);

        if (dialogueSelector != null)
            dialogueSelector.SelectDialogue(ObjectID, this);
    }
}