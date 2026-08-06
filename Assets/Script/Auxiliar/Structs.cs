using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct Spawner
{
  public string spawner_tag;
  public GameObject obj;

  [HideInInspector]
  public List<Transform> positions;
}

[Serializable]
public readonly struct StatModification : IEquatable<StatModification>
{
  public readonly StatType StatType;
  public readonly ModifyType ModifyType;
  public readonly bool IsTemporary;
  public readonly float RemainingTime;

  [NonSerialized]
  public readonly CancellationTokenSource CancellationSource;

  public StatModification(
    StatType statType,
    ModifyType modifyType,
    bool isTemporary,
    float remainingTime = 0f,
    CancellationTokenSource cts = null
  )
  {
    StatType = statType;
    ModifyType = modifyType;
    IsTemporary = isTemporary;
    RemainingTime = remainingTime;
    CancellationSource = cts;
  }

  public override readonly string ToString()
  {
    string tempText = IsTemporary
      ? $" (temporário, {RemainingTime:0.0}s restantes)"
      : " (permanente)";
    return $"[{StatType}]{ModifyType}{tempText}";
  }

  public readonly bool Equals(StatModification other) =>
    StatType == other.StatType && ModifyType == other.ModifyType;

  public override readonly bool Equals(object obj) =>
    obj is StatModification other && Equals(other);

  public override readonly int GetHashCode() => HashCode.Combine(StatType, ModifyType);
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

[Serializable]
public struct RankTime
{
  public RankType Rank;
  public int Seconds;
}
