using System;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct StatEntry
{
    public string stat_name;
    public QualityTier tier;
}

[System.Serializable]
public struct Spawner
{
    public string spawner_tag;
    public GameObject obj;
    [HideInInspector] public List<Transform> positions;
}

[Serializable]
public struct StatModification
{
    public string StatName;
    public QualityTier Tier;
    public ModifyTYPE ModifyType;
    public bool IsTemporary;
    public float RemainingTime; // Tempo restante em segundos (se for temporário)

    public StatModification(string statName, QualityTier tier, ModifyTYPE modifyType, bool isTemporary, float duration = 0f)
    {
        StatName = statName;
        Tier = tier;
        ModifyType = modifyType;
        IsTemporary = isTemporary;
        RemainingTime = duration;
    }

    public override string ToString()
    {
        string tempText = IsTemporary ? $" (temporário, {RemainingTime:0.0}s restantes)" : " (permanente)";
        return $"[{StatName}] {Tier} {ModifyType}{tempText}";
    }
}
public struct Typestats
{
    public Dictionary<string, float> _numstats;
    public Dictionary<string, bool> _boolstats;
}


