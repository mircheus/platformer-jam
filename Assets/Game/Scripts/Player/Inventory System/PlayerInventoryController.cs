using UnityEngine;

public class PlayerInventoryController : MonoBehaviour
{
    private PlayerInventoryModel model;

    private void Awake()
    {
        model = new PlayerInventoryModel();
    }

    public bool TryPickup(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("Pickup failed: item is null");
            return false;
        }

        if (model.HasItem())
        {
            Debug.Log($"Pickup failed: already holding {model.GetCurrentItem().DisplayName}");
            return false;
        }

        model.SetItem(item);

        Debug.Log($"Item picked up: {item.DisplayName} | ID: {item.ItemID}");

        return true;
    }

    public bool HasItem()
    {
        return model.HasItem();
    }

    public bool HasItem(string itemID)
    {
        return model.HasItem() && model.GetCurrentItem().ItemID == itemID;
    }

    public ItemData GetCurrentItem()
    {
        return model.GetCurrentItem();
    }

    public void ConsumeItem()
    {
        if (!model.HasItem())
        {
            return;
        }

        Debug.Log($"Item consumed: {model.GetCurrentItem().DisplayName}");

        model.Clear();
    }
}
