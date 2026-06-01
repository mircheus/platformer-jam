using UnityEngine;

public class PickupInteractable : IInteractable
{
    [Header("Item Data")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private bool advanceProgressForNpc = false;
    [SerializeField] private NPCInteractable targetNPC;

    [Header("Dialogue On Pickup")]
    [SerializeField] private DialogueTrigger dialogueOnPickup;

    protected override void OnInteract()
    {
        Debug.Log($"Trying to pick up item | ObjectID : {ObjectID}");

        if (GameContext.Instance.PlayerInventory.TryPickup(itemData))
        {
            GameContext.Instance.ItemHolder.Attach(transform);
            isInteractable = false;

            if (advanceProgressForNpc)
            {
                targetNPC.AdvanceProgress();
            }

            dialogueOnPickup.TryPlay();
        }
    }

    public void SetInteractable()
    {
        isInteractable = true;
    }
}
