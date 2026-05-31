using UnityEngine;

public class PickupInteractable : IInteractable
{
    [Header("Item Data")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private bool advanceProgressForNpc = false;

    protected override void OnInteract()
    {
        Debug.Log($"Trying to pick up item | ObjectID : {ObjectID}");

        if (GameContext.Instance.PlayerInventory.TryPickup(itemData))
        {
            gameObject.SetActive(false);

            if (advanceProgressForNpc)
            {
                advanceProgressEvent.Invoke();
            }
        }
    }

    public void SetInteractable()
    {
        isInteractable = true;
    }

    public void AdvanceProgress(NPCInteractable npc)
    {
        npc.AdvanceProgress();
    }
}
