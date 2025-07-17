using System;
using System.Collections.Generic;
using UnityEngine;

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