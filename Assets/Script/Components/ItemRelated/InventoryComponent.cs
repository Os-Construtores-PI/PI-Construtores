using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(BrainComponent))]
public class InventoryComponent : ComponentBehaviour
{
    [SerializeField]
    private List<InventoryItem> items = new();
    private EntityType type;


    private void Awake()
    {
        if (TryGetComponent(out BrainComponent brain))
        {
            type = brain.identity.TipoEntidade;
        }

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
        print($"Adicionado: {data.itemName} x {quantity}, {data.itemStats}, {data.usageType}, {data.Isunique}");
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


        if (data.usageType == ItemUsageType.Equipable && type == EntityType.PLAYER)
        {
            if (TryGetComponent<EquipamentComponent>(out var equipment))
            {
                equipment.Equip(data);
            }
        }
        else if (data.usageType == ItemUsageType.Consumable)
        {
            if (TryGetComponent(out StatComponent statComponent))
            {
                foreach (var stat in data.itemStats)
                {
                    statComponent.ApplyStat(stat.stat, stat.tier, statComponent.gameObject, StatComponent.StatTime.TEMPORARY);
                }
                RemoveItem(data, 1); // Consome
            }
        }
        else
        {
            Debug.LogWarning("Item não é utilizável.");
        }
    }
    public List<InventoryItem> GetItems()
    {
        return items;
    }
    public void ClearItems()
    {
        items.Clear();
    }
}
