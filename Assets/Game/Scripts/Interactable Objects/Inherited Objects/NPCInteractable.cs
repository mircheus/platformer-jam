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

        GameContext.Instance.DialogueSelector.SelectDialogue(ObjectID, this);
    }
}