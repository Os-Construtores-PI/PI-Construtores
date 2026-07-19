using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct StatEntry
{
  public string stat_name;
  public QualityTier tier;
}

[Serializable]
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
  public StatType StatType;
  public QualityTier Tier;
  public ModifyTYPE ModifyType;
  public bool IsTemporary;
  public float RemainingTime;

  public StatModification(
    StatType statType,
    QualityTier tier,
    ModifyTYPE modifyType,
    bool isTemporary,
    float remainingTime = 0f
  )
  {
    StatType = statType;
    Tier = tier;
    ModifyType = modifyType;
    IsTemporary = isTemporary;
    RemainingTime = remainingTime;
  }

  public override readonly string ToString()
  {
    string tempText = IsTemporary
      ? $" (temporário, {RemainingTime:0.0}s restantes)"
      : " (permanente)";
    return $"[{StatType}] {Tier} {ModifyType}{tempText}";
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

[Serializable]
public struct TimedPlatformTarget
{
  public Transform Target;
  public float StopTime;
  public float TimeToNext;
}

[Serializable]
public struct ComboStage
{
  public int Multiplier;
  public float TimeToExitStage;
}

[Serializable]
public struct ComboPopupImage
{
  public Sprite Sprite;
  public ComboPopupType Type;
}

[Serializable]
public struct PunchPanelSettings
{
  public float Duration;
  public float Strength;
  public float TweenDuration;
  public int Vibrato;
  public float Elasticity;
  public float MaxRotationZ;
  public static PunchPanelSettings Default =>
    new()
    {
      Duration = 2f,
      Strength = 0.35f,
      TweenDuration = 0.45f,
      Vibrato = 6,
      Elasticity = 0.5f,
      MaxRotationZ = 25f,
    };
}

[Serializable]
public struct RankSpriteEntry
{
  public RankType Rank;
  public Sprite Sprite;
}
