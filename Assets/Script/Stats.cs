using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Stats
{
    // Listagem de modificações ativas
    private readonly List<StatModification> activeModifications = new();

    // Valores atuais
    private Typestats stats = new()
    {
        _numstats = new(),
        _boolstats = new()
    };

    // Valores base originais (para evitar acúmulo indesejado)
    private Dictionary<string, float> numericBaseValues = new();
    private Dictionary<string, bool> boolBaseValues = new();

    public UnityEvent<string, float> OnNumModified = new();
    public UnityEvent<string, bool> OnBoolModified = new();

    // --- ADIÇÃO DE STATS ---
    public bool AddStat<T>(string name, T value) where T : IComparable
    {
        if (typeof(T) == typeof(float))
        {
            if (stats._numstats.ContainsKey(name)) return false;
            float val = Convert.ToSingle(value);
            stats._numstats[name] = val;
            numericBaseValues[name] = val;
            return true;
        }
        else if (typeof(T) == typeof(bool))
        {
            if (stats._boolstats.ContainsKey(name)) return false;
            bool val = Convert.ToBoolean(value);
            stats._boolstats[name] = val;
            boolBaseValues[name] = val;
            return true;
        }

        Debug.LogWarning($"[Stats] Tipo não suportado: {typeof(T)}");
        return false;
    }

    public bool RemoveStat<T>(string name) where T : IComparable
    {
        if (typeof(T) == typeof(float))
        {
            numericBaseValues.Remove(name);
            return stats._numstats.Remove(name);
        }
        if (typeof(T) == typeof(bool))
        {
            boolBaseValues.Remove(name);
            return stats._boolstats.Remove(name);
        }

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

            float original = numericBaseValues[name]; // pega valor base
            float change = original * (multiplier - 1f) * direction;
            stats._numstats[name] = original + change;

            OnNumModified.Invoke(name, stats._numstats[name]);
            activeModifications.Add(new StatModification(name, tier, type, false));
            return true;
        }
        else if (typeof(T) == typeof(bool))
        {
            if (!stats._boolstats.ContainsKey(name)) return false;

            bool original = boolBaseValues[name]; // pega valor base
            stats._boolstats[name] = type == ModifyTYPE.POSITIVE;

            OnBoolModified.Invoke(name, stats._boolstats[name]);
            activeModifications.Add(new StatModification(name, tier, type, false));
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

            float original = numericBaseValues[name];
            float change = original * (multiplier - 1f) * direction;

            SetStat(name, original + change);

            float timer = duration;
            while (timer > 0f)
            {
                timer -= Time.deltaTime;
                UpdateTemporaryTime(name, timer);
                yield return null;
            }

            SetStat(name, original); // restaura valor base
        }
        else if (typeof(T) == typeof(bool))
        {
            if (!stats._boolstats.ContainsKey(name)) yield break;

            bool original = boolBaseValues[name];
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

    // --- REMOVER MODIFICAÇÕES ---
    public void RemoveActiveModifications(string statName)
    {
        activeModifications.RemoveAll(mod => mod.StatName == statName);

        // restaura valor base
        if (numericBaseValues.ContainsKey(statName))
            SetStat(statName, numericBaseValues[statName]);
        else if (boolBaseValues.ContainsKey(statName))
            SetStat(statName, boolBaseValues[statName]);
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

        // atualizar base values
        numericBaseValues = new(nums);
        boolBaseValues = new(bools);
    }
}
