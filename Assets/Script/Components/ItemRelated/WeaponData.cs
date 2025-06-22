using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Inventory/Weapon Data")]
public class WeaponData : ItemData
{
    public List<SkillData> skillSlots = new();
}
