using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class Stats
{
    private readonly List<StatModification> activeModifications = new();
    private Typestats stats = new()
    {
        _numstats = new(),
        _boolstats = new()
    };
    // Tempo base para modificações temporárias (em segundos)
    private const float TEMPORARY_DURATION = 10f;
    public UnityEvent<string, float> _numModified = new();
    public UnityEvent<string, bool> _boolModified = new();

    public bool ModifyStatImmediate<Ttype>(string name, ModifyTYPE type, QualityTier tier) where Ttype : IComparable
    {
        float multiplier = Tiers.GetMultiplier(tier);
        float direction = type == ModifyTYPE.POSITIVE ? 1f : -1f;

        if (typeof(Ttype) == typeof(float))
        {
            if (!stats._numstats.ContainsKey(name)) return false;
            float original = stats._numstats[name];
            float change = original * (multiplier - 1f) * direction;
            stats._numstats[name] += change;
            _numModified.Invoke(name, stats._numstats[name]);
            activeModifications.Add(new(name, tier, type, false));
            return true;
        }
        if (typeof(Ttype) == typeof(bool))
        {
            if (!stats._boolstats.ContainsKey(name)) return false;
            stats._boolstats[name] = type == ModifyTYPE.POSITIVE;
            _boolModified.Invoke(name, stats._boolstats[name]);
            activeModifications.Add(new(name, tier, type, false));
            return true;
        }
        Debug.Log("Tipo não suportado");
        return false;
    }
    public IEnumerator ModifyStatCoroutine<Ttype>(string name, ModifyTYPE type, QualityTier tier, float duration) where Ttype : IComparable
    {
        StatModification tempMod = new(name, tier, type, true, duration);
        activeModifications.Add(tempMod);

        float multiplier = Tiers.GetMultiplier(tier);
        float direction = type == ModifyTYPE.POSITIVE ? 1f : -1f;

        if (typeof(Ttype) == typeof(float))
        {
            if (!stats._numstats.ContainsKey(name)) yield break;
            float original = stats._numstats[name];
            float change = original * (multiplier - 1f) * direction;

            SetStat(name, original + change);

            float timer = duration;
            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                UpdateTemporaryTime(name, timer);
                yield return null;
            }

            SetStat(name, original);
        }
        else if (typeof(Ttype) == typeof(bool))
        {
            if (!stats._boolstats.ContainsKey(name)) yield break;
            bool original = stats._boolstats[name];

            SetStat(name, type == ModifyTYPE.POSITIVE);

            float timer = duration;
            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                UpdateTemporaryTime(name, timer);
                yield return null;
            }

            SetStat(name, original);
        }

        // Remove modificação temporária
        activeModifications.RemoveAll(mod => mod.StatName == name && mod.IsTemporary);
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
        Debug.Log("Tipo não suportado AddStat");
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
        Debug.Log("Tipo não suportado RemoveStat");
        return false;
    }


    public void SetStat<Ttype>(string name, Ttype value) where Ttype : IComparable
    {
        if (typeof(Ttype) == typeof(float))
        {
            if (!stats._numstats.ContainsKey(name)) return;
            stats._numstats[name] = Convert.ToSingle(value);
            _numModified.Invoke(name, stats._numstats[name]);

        }
        else if (typeof(Ttype) == typeof(bool))
        {
            if (!stats._boolstats.ContainsKey(name)) return;
            stats._boolstats[name] = Convert.ToBoolean(value);
            _boolModified.Invoke(name, stats._boolstats[name]);
        }
        else
        {
            Debug.Log("Tipo não suportado SetStat");
        }
    }

    // UTILS
    public IReadOnlyList<StatModification> GetActiveModifications()
    {
        return activeModifications.AsReadOnly();
    }

    private void UpdateTemporaryTime(string statName, float timeLeft)
    {
        for (int i = 0; i < activeModifications.Count; i++)
        {
            if (activeModifications[i].StatName == statName && activeModifications[i].IsTemporary)
            {
                var updated = activeModifications[i];
                updated.RemainingTime = timeLeft;
                activeModifications[i] = updated;
            }
        }
    }
}
