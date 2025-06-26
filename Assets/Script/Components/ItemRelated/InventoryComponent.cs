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
            type = brain.identity.TipoEntidade;
    }

    public void AddItem(ItemDataBase data, int quantity = 1)
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
        Debug.Log($"Adicionado: {data.itemName} x{quantity}");
    }

    public void RemoveItem(ItemDataBase data, int quantity = 1)
    {
        var item = items.Find(i => i.data == data);
        if (item != null)
        {
            item.quantity -= quantity;
            if (item.quantity <= 0)
                items.Remove(item);
        }
    }

    public void UseItem(ItemDataBase data)
    {
        if (!items.Exists(i => i.data == data)) return;

        switch (data)
        {
            case EquipableItemData equipable when type == EntityType.PLAYER:
                if (TryGetComponent<EquipamentComponent>(out var equipComp))
                    equipComp.Equip(equipable);
                break;

            case ConsumableItemData consumable:
                if (TryGetComponent<StatComponent>(out var statComp))
                {
                    foreach (var stat in consumable.itemStats)
                    {
                        statComp.IncreaseStat(stat.stat, stat.tier, gameObject, StatComponent.StatTime.TEMPORARY, stat.duration, stat.cooldown);
                    }
                    RemoveItem(data, 1);
                }
                break;

            case PassiveItemData:
                Debug.Log("Item passivo não é utilizável diretamente.");
                break;

            default:
                Debug.LogWarning("Tipo de item desconhecido.");
                break;
        }
    }

    public List<InventoryItem> GetItems() => items;

    public void ClearItems() => items.Clear();
}
