using System.Collections.Generic;
using Project.Tools.DictionaryHelp;
using UnityEngine;

public class StatZone : StatComponent
{
    [SerializeField] private StatType zoneStat;
    [SerializeField] private QualityTier zoneTier;
    [SerializeField] StatTime statTime = StatTime.TEMPORARY;
    [Header("Só funciona se for status temporário")]
    [SerializeField] float statDuration;
    [SerializeField] float statCooldown;
    [SerializeField] private SerializableDictionary<StatType, List<string>> parTipoStatus_Tags = new() {
        { StatType.ARMOR, new List<string> {"Creature","Player"} },
        { StatType.SPEED,new List<string> {"Creature","Player"}},
        { StatType.ATTACK,new List<string> {"Item","Zone"}},
        { StatType.JUMP, new List<string> {"Player"} }
    };
    // Zone
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer.Equals(LayerMask.NameToLayer("Entity")) && parTipoStatus_Tags.TryGetValue(zoneStat, out List<string> tags) && tags.Contains(other.tag))
        {
            if (other.TryGetComponent(out StatComponent component))
            {
                component.ApplyStat(zoneStat, zoneTier, other.gameObject, statTime,statDuration,statCooldown);       
            }
        }
    }
}
