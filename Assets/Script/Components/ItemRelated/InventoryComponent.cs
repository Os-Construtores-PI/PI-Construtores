using System.Collections.Generic;
using UnityEngine;

public class InventoryComponent : ComponentBehaviour
{
    public List<InventoryItem> items = new();
    [SerializeField] StatComponent statComponent;
    private void Awake()
    {
        statComponent = GetComponent<StatComponent>();
    }
    public void AddItem(ItemData data, int quantity = 1)
    {
        if (!data.Isunique)
        {
            var existing = items.Find(i => i.data == data);
            if (existing != null)
            {
                existing.quantity += quantity;
                return;
            }
        }
        items.Add(new InventoryItem(data, quantity));
        print($"Adicionado: {data.itemName} x{quantity}");
    }
    public void RemoveItem(ItemData data, int quantity = 1)
    {
        var item = items.Find(i => i.data == data);
        if (item != null)
        {
            item.quantity -= quantity;
            if (item.quantity <= 0)
                items.Remove(item);
        }
    }
    public void UseItem(ItemData data)
    {
        if (!items.Exists(i => i.data == data)) return;

        switch (data.usageType)
        {
            case ItemUsageType.Consumable:
                foreach (var stat in data.itemStats)
                {
                    statComponent.ApplyStat(stat.stat, stat.tier, statComponent.gameObject, StatComponent.StatTime.TEMPORARY);
                }
                RemoveItem(data, 1); // Consome uma unidade
                break;

            default:
                Debug.LogWarning("Item não é consumível.");
                break;
        }
    }
}
