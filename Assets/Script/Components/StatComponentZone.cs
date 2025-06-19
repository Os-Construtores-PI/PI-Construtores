using System.Collections.Generic;
using UnityEngine;

public class StatComponentZone : StatComponent
{
    [SerializeField] StatType zoneStat;
    [SerializeField] StatTier zoneTier;
    Dictionary<StatType, string> parTipoTag = new() {
        { StatType.armor, "Creature" },
        { StatType.speed,"Creature"},
        { StatType.attack,"Weapon"},
        { StatType.jump, "Player" },
    };
    // Zone
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Entity")) && other.CompareTag(parTipoTag[zoneStat]))
        {
            ApplyStat(zoneStat, zoneTier, other.gameObject);
        }
    }
}
