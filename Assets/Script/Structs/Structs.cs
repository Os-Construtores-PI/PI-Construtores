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
    [HideInInspector] public List<Transform> positions;
}