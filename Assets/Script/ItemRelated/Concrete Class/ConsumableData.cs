using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumableItem", menuName = "Inventory/Item/Consumable")]
public class ConsumableItemData : StatItemData
{
    public float duration;
    public float cooldown;
}
