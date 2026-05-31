using UnityEngine;

public class NPCInteractable : IInteractable
{
    [Header("Dialogues")]
    [SerializeField] private DialogueData[] dialogues;

    [Header("InteractableObject")]
    [SerializeField] private PickupInteractable pickupInteractable;
    [SerializeField] private bool activateInteractableAfterDialogue;

    private int progress;

    public int Progress => progress;

    public DialogueData GetCurrentDialogue()
    {
        if (dialogues == null || dialogues.Length == 0)
        {
            return null;
        }

        int index = Mathf.Clamp(progress, 0, dialogues.Length - 1);

        return dialogues[index];
    }

    public void AdvanceProgress()
    {
        if (dialogues != null && progress < dialogues.Length - 1)
        {
            progress++;
        }
    }

    public void SetProgress(int value)
    {
        progress = value;
    }

    protected override void OnInteract()
    {
        Debug.Log($"Передаем NPC диалоговой системе | ObjectID : {ObjectID}");

        GameContext.Instance.DialogueSelector.SelectDialogue(this);

        if (activateInteractableAfterDialogue)
        {
            ActivatePickupInteractable();
        }
    }

    private void ActivatePickupInteractable()
    {
        pickupInteractable.SetInteractable();
    }
}
