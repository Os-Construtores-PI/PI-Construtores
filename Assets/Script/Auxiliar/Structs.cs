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

  [HideInInspector]
  public List<Transform> positions;
}

[Serializable]
public struct StatModification
{
  public string StatName;
  public QualityTier Tier;
  public ModifyTYPE ModifyType;
  public bool IsTemporary;
  public float RemainingTime; // Tempo restante em segundos (se for temporário)

  public StatModification(
    string statName,
    QualityTier tier,
    ModifyTYPE modifyType,
    bool isTemporary,
    float duration = 0f
  )
  {
    StatName = statName;
    Tier = tier;
    ModifyType = modifyType;
    IsTemporary = isTemporary;
    RemainingTime = duration;
  }

  public override readonly string ToString()
  {
    string tempText = IsTemporary
      ? $" (temporário, {RemainingTime:0.0}s restantes)"
      : " (permanente)";
    return $"[{StatName}] {Tier} {ModifyType}{tempText}";
  }
}

public struct Typestats
{
  public Dictionary<string, float> _numstats;
  public Dictionary<string, bool> _boolstats;
}

public struct Code
{
  public int Number;
  public Color Color;

  public Code(int num, Color col)
  {
    Number = num;
    Color = col;
  }
}

[Serializable]
public struct CustomPanel
{
  public string Name;
  public List<GameObject> Panel;
}

[Serializable]
public struct CustomCanvas
{
  public int PlayerID;
  public List<CustomPanel> Panels;
}

public struct InfoPlayerInteraction
{
  public GameObject Obj;
  public PlayerContext PlayerContext;

  public InfoPlayerInteraction(GameObject gameObject, PlayerContext script)
  {
    Obj = gameObject;
    PlayerContext = script;
  }
}

[Serializable]
public struct IconImage
{
  public string Destiny;
  public Sprite Sprite;
}

[Serializable]
public struct Effect
{
  public string Name;
  public GameObject GameObject;
}

[Serializable]
public struct LevelPath
{
  public float Rotation;
  public LevelPathType PathType;
}
