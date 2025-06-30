using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipableItem", menuName = "Inventory/Item/Equipable")]
public class EquipableItemData : StatItemData
{
    public List<SkillData> skills;
}
