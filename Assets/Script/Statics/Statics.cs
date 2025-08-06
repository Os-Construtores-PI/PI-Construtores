using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;

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



public static class Constants
{
    public static class PanelNames
    {
        public static readonly string GameOver = "GameOver";
        public static readonly string InteractionPopup = "InteractionPopup";
        public static readonly string InteractionLetter = "InteractionLetter";
    } 
    public static readonly float GraplingHookCutsceneDuration = 3.5f;
    public static class LowRangeObjects
    {
        public static HashSet<Type> types = new() {typeof(BasicButton),typeof(PuzzleColorButton)};
    }
    public static class HighRangeObjects
    {
        public static HashSet<Type> types = new() {typeof(GraplingHookTarget)};
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