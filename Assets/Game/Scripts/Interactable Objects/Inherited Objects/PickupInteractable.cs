using UnityEngine;

public class PickupInteractable : IInteractable
{
    [Header("Item Data")]
    [SerializeField] private ItemData itemData;

    protected override void OnInteract()
    {
        Debug.Log($"Trying to pick up item | ObjectID : {ObjectID}");

        var inventory = FindFirstObjectByType<PlayerInventoryController>();

        if (inventory == null)
        {
            Debug.LogWarning("PlayerInventoryController not found on scene");
            return;
        }

        if (inventory.TryPickup(itemData))
        {
            gameObject.SetActive(false);
        }
    }
}
