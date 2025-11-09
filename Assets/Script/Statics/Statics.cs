using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;

public static class Tiers
{
    public static readonly Dictionary<QualityTier, float> EvaluationMap = new()
    {
        { QualityTier.COMMON, 1.0f },
        { QualityTier.UNCOMMON, 1.20f },
        { QualityTier.RARE, 1.35f },
        { QualityTier.EPIC, 1.65f },
        { QualityTier.LEGENDARY, 1.80f }
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
         new((int)ColorCode.YELLOW,Color.yellow),
         new((int)ColorCode.BLUE,Color.blue),
         new((int)ColorCode.RED,Color.red),
         new((int)ColorCode.GREEN,Color.green)
    };
}

public static class ListUtils
{
    public static bool ListIdenticalComparison<T>(List<T> list1, List<T> list2)
    {
        if (list1 == null || list2 == null) return default;
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
        { Constants.StatsNames.JumpForce, typeof(float) }
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
        public static readonly string MenuScene = "MainMenu";
    }
    public static class Values
    {
        public static readonly float GraplingHookSpeed = 10f;
    }
    public static class PanelNames
    {
        public static readonly string GameOver = "PainelGameOver";
        public static readonly string InteractionPopup = "InteractionPopup";
        public static readonly string InteractionLetter = "InteractionLetter";
        public static readonly string GraplingHookCutscene = "GraplingHookCutscene";
        public static readonly string TeleportFadePanel = "TeleportFadePanel";
    }
    public static class PlayerCommonObjects
    {
        public static HashSet<Type> types = new() { typeof(BasicButton), typeof(PuzzleColorButton) };
    }
    public static class PandoraObjects
    {
        public static HashSet<Type> types = new() { typeof(GraplingHookTarget) };
    }
    public static class RuskaObjects
    {
        public static HashSet<Type> types = new() {};
    }
    public enum StatsNames
    {
        CanDash, Speed, Health, Defense, MaxHealth, JumpForce, EnableRegen
    }
    public enum Tags
    {
        Player,Enemy,RunningWall
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
    public static float PlayerFriction(float value, float frictionAmount, Vector2 intention)
    {
        if (intention == Vector2.zero)
            return SmoothLerp(value, 0f, frictionAmount);
        return value;
    }
    
}


public static class ReflectionHelpers
{
    public static PropertyInfo GetPropertyByStatName(this Type type, string statName)
    {
        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var prop in properties)
        {
            var attr = prop.GetCustomAttribute<StatAttribute>();
            if (attr != null && attr.Name == statName)
                return prop;
        }
        return null;
    }
}