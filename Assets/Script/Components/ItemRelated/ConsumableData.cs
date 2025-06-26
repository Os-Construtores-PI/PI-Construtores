using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumableItem", menuName = "Inventory/Item/Consumable")]
public class ConsumableItemData : ItemDataBase
{
    public List<StatEntry> itemStats = new();

    [System.Serializable]
    public class StatEntry
    {
        public StatType stat;
        public float duration;
        public float cooldown;
        public QualityTier tier;
    }
}
