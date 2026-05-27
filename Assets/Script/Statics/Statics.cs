using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

public static class Tiers
{
  public static readonly Dictionary<QualityTier, float> EvaluationMap = new()
  {
    { QualityTier.COMMON, 1.0f },
    { QualityTier.UNCOMMON, 1.20f },
    { QualityTier.RARE, 1.35f },
    { QualityTier.EPIC, 1.65f },
    { QualityTier.LEGENDARY, 1.80f },
  };

  public static float GetMultiplier(QualityTier tier)
  {
    return EvaluationMap.TryGetValue(tier, out var value) ? value : 1.0f;
  }
}

public static class Lookups
{
  public static class Effects
  {
    public static Dictionary<string, EffectType> LookupTable = new()
    {
      { EffectType.JumpEffect.ToString(), EffectType.JumpEffect },
      { EffectType.DashEffect.ToString(), EffectType.DashEffect },
      { EffectType.ChargingEffect.ToString(), EffectType.ChargingEffect },
      { EffectType.BoostEffect.ToString(), EffectType.BoostEffect },
      { EffectType.SpeedEffect.ToString(), EffectType.SpeedEffect },
    };
  }

  public static class Trails
  {
    public static Dictionary<string, TrailType> LookupTable = new()
    {
      { TrailType.MovementTrail.ToString(), TrailType.MovementTrail },
      { TrailType.MovementSupport1Trail.ToString(), TrailType.MovementSupport1Trail },
      { TrailType.MovementSupport2Trail.ToString(), TrailType.MovementSupport2Trail },
    };
  }
}

public static class CodeBaseFour
{
  public static List<Code> Codes = new()
  {
    new((int)ColorCode.YELLOW, Color.yellow),
    new((int)ColorCode.BLUE, Color.blue),
    new((int)ColorCode.RED, Color.red),
    new((int)ColorCode.GREEN, Color.green),
  };
}

public static class ListUtils
{
  public static bool ListIdenticalComparison<T>(List<T> list1, List<T> list2)
  {
    if (list1 == null || list2 == null)
      return default;
    return list1.SequenceEqual(list2);
  }

  public static string ToString<T>(List<T> list)
  {
    string result = "///////////////////";
    foreach (var obj in list)
    {
      result += $"\n/// {obj}";
    }
    result += "\n///////////////////";
    return result;
  }
}

public static class StringtoTypes
{
  public static readonly Dictionary<string, Type> TypeMap = new()
  {
    { "bool", typeof(bool) },
    { "int", typeof(int) },
    { "float", typeof(float) },
    { "double", typeof(double) },
    { "string", typeof(string) },
    { "long", typeof(long) },
    { "short", typeof(short) },
    { "byte", typeof(byte) },
  };
}

public static class StatTypeMap
{
  public static readonly Dictionary<StatType, Type> Map = new()
  {
    { StatType.CanDash, typeof(bool) },
    { StatType.Regen, typeof(bool) },
    { StatType.Speed, typeof(float) },
    { StatType.Health, typeof(float) },
    { StatType.MaxHealth, typeof(float) },
    { StatType.JumpForce, typeof(float) },
  };

  public static Type GetType(StatType stat) => Map[stat];
}

public static class Constants
{
  public static class PersistentNames
  {
    public static readonly string DataPath = Application.persistentDataPath + "GameData.json";
    public static readonly string ConfigPath = Application.persistentDataPath + "ConfigData.json";
    public const string CryptoKey = "Pão de Queijo";
  }

  public static class SceneNames
  {
    public const string DebugScene = "Cena Debug";
    public const string FirstLevel = "Fase0";
    public const string MainMenu = "MainMenu";
  }

  public static class AnimatorTriggerNames
  {
    public const string Idle = nameof(Idle);
    public const string Walk = nameof(Walk);
    public const string WasVerticalBoosted = nameof(WasVerticalBoosted);
    public const string Jump = nameof(Jump);
    public const string DoubleJump = nameof(DoubleJump);
    public const string Hit = nameof(Hit);
    public const string Dash = nameof(Dash);
  }

  public static class AnimatorBoolNames
  {
    public const string IsGrounded = nameof(IsGrounded);
  }

  public static class AnimatorFloatNames
  {
    public const string VelocityY = nameof(VelocityY);
    public const string VelocityX = nameof(VelocityX);
  }

  public static class PlayerShakes
  {
    public static class Damage
    {
      public const float Amplitude = 1f;
      public const float Frequency = 1f;
      public const float Duration = 0.25f;
    }

    public static class Running
    {
      public const float Amplitude = 0.1f;
      public const float Frequency = 0.7f;
      public const float StopDelay = 0.50f;
    }
  }

  public static class Values
  {
    public const float GraplingHookSpeed = 10f;
  }

  public static class HudPanelNames
  {
    public const string GameOver = nameof(GameOver);
    public const string Pause = nameof(Pause);
    public const string Dialogue = nameof(Dialogue);
    public const string HealthBar = nameof(HealthBar);
    public const string BoostBar = nameof(BoostBar);
    public const string DashIcon = nameof(DashIcon);
    public const string EndGame = nameof(EndGame);
    public const string AmethystCounter = nameof(AmethystCounter);
    public const string InteractionPopup = nameof(InteractionPopup);
    public const string InteractionLetter = nameof(InteractionLetter);
    public const string LockOnOverlay = nameof(LockOnOverlay);
    public const string Cutscene = nameof(Cutscene);
    public const string TeleportFadePanel = nameof(TeleportFadePanel);
  }

  public static class MenuPanelNames
  {
    public const string Menu = nameof(Menu);
    public const string AudioMenu = nameof(AudioMenu);
    public const string OptionsMenu = nameof(OptionsMenu);
    public const string SaveMenu = nameof(SaveMenu);
  }

  public static class PlayerCommonObjects
  {
    public static HashSet<Type> types = new()
    {
      typeof(BasicButton),
      typeof(PuzzleColorButton),
      typeof(DialogueArea),
      typeof(SwingObject),
    };
  }

  public static class CameraGroup
  {
    public const string MainCamera = nameof(MainCamera);
    public const string MainCinemachine = nameof(MainCinemachine);
    public const string BoostCinemachine = nameof(BoostCinemachine);
    public const string CinemachineLockOn = nameof(CinemachineLockOn);
    public const string LockInGroup = nameof(LockInGroup);
  }

  public static class PandoraObjects
  {
    public static HashSet<Type> types = new() { typeof(GraplingHookTarget) };
  }

  public static class RuskaObjects
  {
    public static HashSet<Type> types = new() { };
  }

  public enum Tags
  {
    Player,
    Enemy,
    RunningWall,
  }
}

public static class StaticRandomizer
{
  public static List<T> ListRandomizer<T>(List<T> oglist)
  {
    List<T> list = oglist;
    System.Random rng = new();
    int n = list.Count;
    while (n > 1)
    {
      n--;
      int k = rng.Next(n + 1);
      (list[n], list[k]) = (list[k], list[n]);
    }
    return list;
  }
}

public static class QualityOfLife
{
  public static float SmoothLerp(float from, float to, float smoothing)
  {
    return Mathf.Lerp(from, to, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
  }

  public static float SmoothStepLerp(float from, float to, float smoothing)
  {
    float t = 1f - Mathf.Exp(-smoothing * Time.fixedDeltaTime);
    // 3t^2 - 2t^3 -> Curva perfeita para controle de personagem
    float smoothT = t * t * (3f - 2f * t);
    return Mathf.Lerp(from, to, smoothT);
  }

  public static float SmoothCubicIn(float from, float to, float smoothing)
  {
    float t = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
    float cubicIn = t * t * t; // t ao cubo
    return Mathf.Lerp(from, to, cubicIn);
  }

  public static float SmoothCubicOut(float from, float to, float smoothing)
  {
    float t = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
    float invT = t - 1f;
    // Formula Cubic Out: (t-1)^3 + 1
    float cubicT = invT * invT * invT + 1f;
    return Mathf.Lerp(from, to, cubicT);
  }

  public static float SmoothQuadIn(float from, float to, float smoothing)
  {
    float t = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
    float quadIn = t * t; // Eleva o peso do tempo ao quadrado
    return Mathf.Lerp(from, to, quadIn);
  }

  public static float SmoothQuadOut(float from, float to, float smoothing)
  {
    float t = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
    // Aplica a curvatura Quad Out: t * (2 - t)
    float quadT = t * (2f - t);
    return Mathf.Lerp(from, to, quadT);
  }

  public static float PlayerFriction(float value, float frictionAmount, Vector2 intention)
  {
    // Se não há intenção, aplica fricção cúbica para uma parada mais natural
    return (intention == Vector2.zero) ? SmoothCubicOut(value, 0f, frictionAmount) : value;
  }

  public static bool IsValidIndex<T>(List<T> list, int index)
  {
    return index == (list.Count - 1) && index >= 0;
  }

  public static void WriteJsonInDisk<T>(T classObject, string path)
  {
    string json = JsonUtility.ToJson(classObject, true);
    File.WriteAllText(path, DataCryptography.Encrypt(json));
  }
}

public static class ReflectionHelpers
{
  public static PropertyInfo GetPropertyByStatName(this Type type, StatType statType)
  {
    var properties = type.GetProperties(
      BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
    );
    foreach (var prop in properties)
    {
      var attr = prop.GetCustomAttribute<StatAttribute>();
      if (attr != null && attr.Type == statType)
        return prop;
    }
    return null;
  }
}

public static class DataCryptography
{
  public static string Encrypt(string input)
  {
    if (string.IsNullOrEmpty(input))
      return string.Empty;

    byte[] data = Encoding.UTF8.GetBytes(input);
    byte[] key = Encoding.UTF8.GetBytes(Constants.PersistentNames.CryptoKey);

    for (int i = 0; i < data.Length; i++)
      data[i] ^= key[i % key.Length];

    return Convert.ToBase64String(data);
  }

  public static string Decrypt(string input)
  {
    if (string.IsNullOrEmpty(input))
      return string.Empty;

    byte[] data = Convert.FromBase64String(input);
    byte[] key = Encoding.UTF8.GetBytes(Constants.PersistentNames.CryptoKey);

    for (int i = 0; i < data.Length; i++)
      data[i] ^= key[i % key.Length];

    return Encoding.UTF8.GetString(data);
  }
}
