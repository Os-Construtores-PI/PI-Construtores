
[System.Serializable]
public class InventoryItem
{
    public ItemData data;
    public int quantity;

    public InventoryItem(ItemData data, int quantity = 1)
    {
        this.data = data;
        this.quantity = quantity;
    }
}
public class ItemComponent : ComponentBehaviour
{
    public ItemData itemData;
    public int quantity = 1;
}
