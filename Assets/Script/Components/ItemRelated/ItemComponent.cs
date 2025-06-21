using System.Collections.Generic;
using UnityEngine;


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
    [SerializeField] public ItemData itemData;
    [SerializeField] public int quantity = 1;
    public GameObject item;

    private void Awake()
    {
        item = gameObject;
    }

    public string GetName() => itemData != null ? itemData.itemName : null ?? "Unknown";
}
