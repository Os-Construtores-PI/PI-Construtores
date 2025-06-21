using System;
using UnityEngine;

public static class Operators<T> where T : struct, IComparable
{
    public static readonly Func<T, float, T> Multiply;
    public static readonly Func<T, float, T> Divide;
    public static readonly Func<T, T, T,T> Clamp;

    static Operators()
    {
        Type type = typeof(T);

        if (type == typeof(int))
        {
            Multiply = (a, b) => (T)(object)((int)(object)a * b);
            Divide = (a, b) => (T)(object)(int)((int)(object)a / b);
            Clamp = (val, min, max) => (T)(object)Mathf.Clamp((int)(object)val, (int)(object)min, (int)(object)max);
        }
        else if (type == typeof(float))
        {
            Multiply = (a, b) => (T)(object)((float)(object)a * b);
            Divide = (a, b) => (T)(object)((float)(object)a / b);
            Clamp = (val, min, max) => (T)(object)Mathf.Clamp((float)(object)val, (float)(object)min, (float)(object)max);
        }

        else if (type == typeof(double))
        {
            Multiply = (a, b) => (T)(object)((double)(object)a * b);
            Divide = (a, b) => (T)(object)((double)(object)a / b);
        }
        else
        {
            throw new NotSupportedException($"Tipo {type} não suportado em Operators<T>.");
        }
    }
}
