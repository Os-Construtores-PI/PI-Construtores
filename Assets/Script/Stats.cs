using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class Stats
{
    // Struct que agrupa os dois tipos de atributos
    struct Typestats
    {
        public Dictionary<string, float> _numstats;
        public Dictionary<string, bool> _boolstats;
    }

    private Typestats stats = new()
    {
        _numstats = new Dictionary<string, float>(),
        _boolstats = new Dictionary<string, bool>()
    };

    private readonly Dictionary<QualityTier, float> evaluation = new()
    {
        { QualityTier.COMMON, 1.0f },
        { QualityTier.UNCOMMON, 1.15f },
        { QualityTier.RARE, 1.30f },
        { QualityTier.EPIC, 1.65f },
        { QualityTier.LEGENDARY, 1.80f }
    };

    // Tempo base para modificações temporárias (em segundos)
    private const float TEMPORARY_DURATION = 10f;
    public UnityEvent<string,float > _numModified = new();
    public UnityEvent<string,bool> _boolModified = new();

    public bool ModifyStatImmediate<Ttype>(string name, ModifyTYPE type, QualityTier tier) where Ttype : IComparable
    {
        float multiplier = evaluation[tier];
        float direction = type == ModifyTYPE.POSITIVE ? 1f : -1f;

        if (typeof(Ttype) == typeof(float))
        {
            if (!stats._numstats.ContainsKey(name)) return false;
            float original = stats._numstats[name];
            float change = original * (multiplier - 1f) * direction;
            stats._numstats[name] += change;
            _numModified.Invoke(name, stats._numstats[name]);
            return true;
        }

        if (typeof(Ttype) == typeof(bool))
        {
            if (!stats._boolstats.ContainsKey(name)) return false;
            stats._boolstats[name] = type == ModifyTYPE.POSITIVE;
            _boolModified.Invoke(name, stats._boolstats[name]);
            return true;
        }

        return false;
    }
    public IEnumerator ModifyStatCoroutine<Ttype>(string name, ModifyTYPE type, QualityTier tier, TimeTYPE timeType, float duration) where Ttype : IComparable
    {
        float multiplier = evaluation[tier];
        float direction = type == ModifyTYPE.POSITIVE ? 1f : -1f;

        if (typeof(Ttype) == typeof(float))
        {
            if (!stats._numstats.ContainsKey(name)) yield break;
            float original = stats._numstats[name];
            float change = original * (multiplier - 1f) * direction;

            stats._numstats[name] += change;
            yield return new WaitForSeconds(duration);
            stats._numstats[name] = original;
        }
        else if (typeof(Ttype) == typeof(bool))
        {
            if (!stats._boolstats.ContainsKey(name)) yield break;
            bool original = stats._boolstats[name];
            stats._boolstats[name] = type == ModifyTYPE.POSITIVE;
            yield return new WaitForSeconds(duration);
            stats._boolstats[name] = original;
        }

        yield break;
    }



    public bool AddStat<Ttype>(string name, Ttype value) where Ttype : IComparable
    {
        if (typeof(Ttype) == typeof(float))
        {
            if (stats._numstats.ContainsKey(name)) return false;
            stats._numstats.Add(name, Convert.ToSingle(value));
            return true;
        }
        else if (typeof(Ttype) == typeof(bool))
        {
            if (stats._boolstats.ContainsKey(name)) return false;
            stats._boolstats.Add(name, Convert.ToBoolean(value));
            return true;
        }

        return false;
    }

    public bool RemoveStat<Ttype>(string name) where Ttype : IComparable
    {
        if (typeof(Ttype) == typeof(float))
        {
            return stats._numstats.Remove(name);
        }
        else if (typeof(Ttype) == typeof(bool))
        {
            return stats._boolstats.Remove(name);
        }

        return false;
    }
}
