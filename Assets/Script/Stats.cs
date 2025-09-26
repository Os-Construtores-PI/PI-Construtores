using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Stats
{
    private readonly List<StatModification> activeModifications = new();

    private Typestats stats = new()
    {
        _numstats = new(),
        _boolstats = new()
    };

    public UnityEvent<string, float> OnNumModified = new();
    public UnityEvent<string, bool> OnBoolModified = new();

    // --- ADIÇÃO DE STATS ---
    public bool AddStat<T>(string name, T value) where T : IComparable
    {
        if (typeof(T) == typeof(float))
        {
            if (stats._numstats.ContainsKey(name)) return false;
            stats._numstats[name] = Convert.ToSingle(value);
            return true;
        }
        else if (typeof(T) == typeof(bool))
        {
            if (stats._boolstats.ContainsKey(name)) return false;
            stats._boolstats[name] = Convert.ToBoolean(value);
            return true;
        }

        Debug.LogWarning($"[Stats] Tipo não suportado: {typeof(T)}");
        return false;
    }

    public bool RemoveStat<T>(string name) where T : IComparable
    {
        if (typeof(T) == typeof(float)) return stats._numstats.Remove(name);
        if (typeof(T) == typeof(bool)) return stats._boolstats.Remove(name);

        Debug.LogWarning($"[Stats] Tipo não suportado: {typeof(T)}");
        return false;
    }

    // --- MODIFICAÇÃO IMEDIATA ---
    public bool ModifyStatImmediate<T>(string name, ModifyTYPE type, QualityTier tier) where T : IComparable
    {
        float multiplier = Tiers.GetMultiplier(tier);
        float direction = type == ModifyTYPE.POSITIVE ? 1f : -1f;

        if (typeof(T) == typeof(float))
        {
            if (!stats._numstats.ContainsKey(name)) return false;
            float original = stats._numstats[name];
            float change = original * (multiplier - 1f) * direction;
            stats._numstats[name] += change;
            OnNumModified.Invoke(name, stats._numstats[name]);
            activeModifications.Add(new(name, tier, type, false));
            return true;
        }
        else if (typeof(T) == typeof(bool))
        {
            if (!stats._boolstats.ContainsKey(name)) return false;
            stats._boolstats[name] = type == ModifyTYPE.POSITIVE;
            OnBoolModified.Invoke(name, stats._boolstats[name]);
            activeModifications.Add(new(name, tier, type, false));
            return true;
        }

        Debug.LogWarning($"[Stats] Tipo não suportado: {typeof(T)}");
        return false;
    }

    // --- MODIFICAÇÃO TEMPORÁRIA ---
    public IEnumerator ModifyStatCoroutine<T>(string name, ModifyTYPE type, QualityTier tier, float duration) where T : IComparable
    {
        StatModification tempMod = new(name, tier, type, true, duration);
        activeModifications.Add(tempMod);

        float multiplier = Tiers.GetMultiplier(tier);
        float direction = type == ModifyTYPE.POSITIVE ? 1f : -1f;

        if (typeof(T) == typeof(float))
        {
            if (!stats._numstats.ContainsKey(name)) yield break;
            float baseValue = stats._numstats[name];
            float change = baseValue * (multiplier - 1f) * direction;

            SetStat(name, baseValue + change);

            float timer = duration;
            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                UpdateTemporaryTime(name, timer);
                yield return null;
            }

            // ⚠️ Melhor: recalcular baseado no valor base em vez de restaurar "às cegas"
            stats._numstats[name] = baseValue;
            OnNumModified.Invoke(name, baseValue);
        }
        else if (typeof(T) == typeof(bool))
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

        activeModifications.RemoveAll(mod => mod.StatName == name && mod.IsTemporary);
    }

    public void RemoveActiveModifications(string statName)
    {
        // Remove todas as mods daquele stat
        activeModifications.RemoveAll(mod => mod.StatName == statName);

        // Se for numérico, restaura pro valor inicial registrado
        if (stats._numstats.ContainsKey(statName))
        {
            float baseValue = stats._numstats[statName];
            SetStat(statName, baseValue);
        }
        // Se for booleano, volta para false (ou outro valor padrão que você definir)
        else if (stats._boolstats.ContainsKey(statName))
        {
            SetStat(statName, false);
        }
    }


    // --- SET GENÉRICO ---
    public void SetStat<T>(string name, T value) where T : IComparable
    {
        if (typeof(T) == typeof(float) && stats._numstats.ContainsKey(name))
        {
            stats._numstats[name] = Convert.ToSingle(value);
            OnNumModified.Invoke(name, stats._numstats[name]);
        }
        else if (typeof(T) == typeof(bool) && stats._boolstats.ContainsKey(name))
        {
            stats._boolstats[name] = Convert.ToBoolean(value);
            OnBoolModified.Invoke(name, stats._boolstats[name]);
        }
    }

    // --- UTILS ---
    public IReadOnlyList<StatModification> GetActiveModifications() => activeModifications.AsReadOnly();

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

    // Serialização custom para salvar no DataSystem
    public Dictionary<string, float> GetNumericStats() => new(stats._numstats);
    public Dictionary<string, bool> GetBoolStats() => new(stats._boolstats);

    public void LoadFromDictionaries(Dictionary<string, float> nums, Dictionary<string, bool> bools)
    {
        stats._numstats = new(nums);
        stats._boolstats = new(bools);
    }
}
