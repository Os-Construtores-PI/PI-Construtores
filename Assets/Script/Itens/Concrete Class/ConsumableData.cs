using UnityEngine;

[CreateAssetMenu(fileName = "ConsumableItem", menuName = "Inventory/Item/Consumable")]
public class ConsumableItemData : StatItemData
{
    public float duration;
    public float cooldown;
}
