using UnityEngine;

public class PickupInteractable : IInteractable
{
    [Header("Item Data")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private bool advanceProgressForNpc = false;
    [SerializeField] private NPCInteractable targetNPC;

    protected override void OnInteract()
    {
        Debug.Log($"Trying to pick up item | ObjectID : {ObjectID}");

        if (GameContext.Instance.PlayerInventory.TryPickup(itemData))
        {
            gameObject.SetActive(false);

            if (advanceProgressForNpc)
            {
                targetNPC.AdvanceProgress();
            }
        }
    }

    public void SetInteractable()
    {
        isInteractable = true;
    }
}
