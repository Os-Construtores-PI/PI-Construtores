using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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