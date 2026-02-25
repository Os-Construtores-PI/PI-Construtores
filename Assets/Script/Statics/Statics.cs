using System;
using System.Collections.Generic;
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
  public static readonly Dictionary<Constants.StatsNames, Type> Map = new()
  {
    { Constants.StatsNames.CanDash, typeof(bool) },
    { Constants.StatsNames.EnableRegen, typeof(bool) },
    { Constants.StatsNames.Speed, typeof(float) },
    { Constants.StatsNames.Health, typeof(float) },
    { Constants.StatsNames.MaxHealth, typeof(float) },
    { Constants.StatsNames.Defense, typeof(float) },
    { Constants.StatsNames.JumpForce, typeof(float) },
  };

  public static Type GetType(Constants.StatsNames stat) => Map[stat];
}

public static class Constants
{
  public static class PersistentNames
  {
    public static readonly string DataPath = Application.persistentDataPath + "GAMEDATA.json";
    public static readonly string CryptoKey = "Pão de Queijo";
  }

  public static class SceneNames
  {
    public static readonly string DebugScene = "Cena Debug";
    public static readonly string Fase0 = "Fase0";
    public static readonly string MainMenu = "MainMenu";
  }

  public static class AnimatorTriggerNames
  {
    public static readonly string Idle = "Idle";
    public static readonly string Walk = "Walk";
    public static readonly string Jump = "Jump";
    public static readonly string DoubleJump = "DoubleJump";
    public static readonly string Hit = "Hit";
    public static readonly string Dash = "Dash";
  }

  public static class AnimatorBoolNames
  {
    public static readonly string IsGrounded = "IsGrounded";
  }

  public static class AnimatorFloatNames
  {
    public const string VelocityY = "VelocityY";
    public const string VelocityX = "VelocityX";
  }

  public static class EffectsNames
  {
    public static class Player
    {
      public const string Dash = "Dash";
      public const string Jump = "Jump";
      public const string Run = "Run";
    }

    public static class Interface
    {
      public const string Speed = "Speed";
    }
  }

  public static class Values
  {
    public static readonly float GraplingHookSpeed = 10f;
  }

  public static class HudPanelNames
  {
    public static readonly string GameOver = "GameOver";
    public static readonly string Pause = "Pause";
    public static readonly string Dialogue = "Dialogue";
    public static readonly string HealthBar = "HealthBar";
    public static readonly string DashIcon = "DashIcon";
    public static readonly string EndGame = "EndGame";
    public static readonly string AmethystCounter = "AmethystCounter";
    public static readonly string InteractionPopup = "InteractionPopup";
    public static readonly string InteractionLetter = "InteractionLetter";
    public static readonly string Cutscene = "Cutscene";
    public static readonly string TeleportFadePanel = "TeleportFadePanel";
  }

  public static class MenuPanelNames
  {
    public static readonly string Menu = "Menu";
    public static readonly string AudioMenu = "AudioMenu";
    public static readonly string OptionsMenu = "OptionsMenu";
    public static readonly string SaveMenu = "SaveMenu";
  }

  public static class PlayerCommonObjects
  {
    public static HashSet<Type> types = new()
    {
      typeof(BasicButton),
      typeof(PuzzleColorButton),
      typeof(DialogueArea),
    };
  }

  public static class PandoraObjects
  {
    public static HashSet<Type> types = new() { typeof(GraplingHookTarget) };
  }

  public static class RuskaObjects
  {
    public static HashSet<Type> types = new() { };
  }

  public enum StatsNames
  {
    CanDash,
    Speed,
    Health,
    Defense,
    MaxHealth,
    JumpForce,
    EnableRegen,
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
}

public static class ReflectionHelpers
{
  public static PropertyInfo GetPropertyByStatName(this Type type, string statName)
  {
    var properties = type.GetProperties(
      BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
    );
    foreach (var prop in properties)
    {
      var attr = prop.GetCustomAttribute<StatAttribute>();
      if (attr != null && attr.Name == statName)
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
