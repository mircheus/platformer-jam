public class PlayerInventoryModel
{
    private ItemData currentItem;

    public bool HasItem()
    {
        return currentItem != null;
    }

    public ItemData GetCurrentItem()
    {
        return currentItem;
    }

    public void SetItem(ItemData item)
    {
        currentItem = item;
    }

    public void Clear()
    {
        currentItem = null;
    }
}
