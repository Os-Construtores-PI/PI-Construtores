using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    // Lista de itens no inventário
    [SerializeField]
    private List<InventoryItem> items = new();
    public List<InventoryItem> GetItems() => items;
    public void ClearItems() => items.Clear();
    public void AddItem(ItemDataBase data, int quantity = 1)
    {
        // Se o item não é único, tenta acumular com outro igual
        if (!data.Isunique)
        {
            var existing = items.Find(i => i.data == data);
            if (existing != null)
            {
                existing.quantity += quantity;
                return;
            }
        }

        // Caso contrário, adiciona um novo item à lista
        items.Add(new InventoryItem(data, quantity));
        Debug.Log($"Adicionado: {data.itemName} x{quantity}");
    }
    public void RemoveItem(ItemDataBase data, int quantity = 1)
    {
        var existing = items.Find(i => i.data == data);
        if (!data.Isunique)
        {
            if (existing != null)
            {
                if ((existing.quantity - quantity) <= 0)
                {
                    items.Remove(existing);
                }
                else
                {
                    existing.quantity -= quantity;
                }
            }
            return;
        }
        else
        {
            items.Remove(existing);
        }
    }
    public void UseItem(ItemDataBase itemData)
    {
        if (itemData.GetType() == typeof(PassiveItemData))
        {
            Debug.Log("Não pode usar item passivo");
        }
        else if (itemData.GetType() == typeof(ConsumableItemData))
        {
            Debug.Log("Usado");
            RemoveItem(itemData);
            // EFFECTS
        }
    }
}


