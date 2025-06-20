using System.Collections.Generic;
using UnityEngine;

public class StatComponentZone : StatComponent
{
    [SerializeField] StatType zoneStat;
    [SerializeField] StatTier zoneTier;

    Dictionary<StatType, List<string>> parTipoTag = new() {
        { StatType.armor, new List<string> {"Creature","Player"} },
        { StatType.speed,new List<string> {"Creature","Player"}},
        { StatType.attack,new List<string> {"Weapon","Zone"}},
        { StatType.jump, new List<string> {"Player"} }
    };
    // Zone
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Entity")) && parTipoTag.TryGetValue(zoneStat,out List<string> tags) && tags.Contains(other.tag))
        {
            ApplyStat(zoneStat, zoneTier, other.gameObject);
        }
    }
}
