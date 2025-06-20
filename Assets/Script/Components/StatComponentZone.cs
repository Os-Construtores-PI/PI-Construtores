using System.Collections.Generic;
using Project.Tools.DictionaryHelp;
using UnityEngine;

public class StatComponentZone : StatComponent
{
    [SerializeField] private StatType zoneStat;
    [SerializeField] private StatTier zoneTier;

    [SerializeField] private SerializableDictionary<StatType, List<string>> parTipoStatus_Tags = new() {
        { StatType.armor, new List<string> {"Creature","Player"} },
        { StatType.speed,new List<string> {"Creature","Player"}},
        { StatType.attack,new List<string> {"Weapon","Zone"}},
        { StatType.jump, new List<string> {"Player"} }
    };
    // Zone
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Entity")) && parTipoStatus_Tags.TryGetValue(zoneStat, out List<string> tags) && tags.Contains(other.tag))
        {
            ApplyStat(zoneStat, zoneTier, other.gameObject);
        }
    }
}
