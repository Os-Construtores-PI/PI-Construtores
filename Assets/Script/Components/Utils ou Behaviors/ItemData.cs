using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    public ItemUsageType usageType;
    public string itemName;
    public List<StatEntry> itemStats = new();
    public int quantity;
    public bool Isunique;
    public GameObject item;
    public Sprite itemIcon;

    
    [System.Serializable]
    public class StatEntry
    {
        public StatType stat;
        public QualityTier tier;
    }
}
