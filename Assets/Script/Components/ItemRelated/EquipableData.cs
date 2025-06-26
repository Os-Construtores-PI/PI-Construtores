using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipableItem", menuName = "Inventory/Item/Equipable")]
public class EquipableItemData : ItemDataBase
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
