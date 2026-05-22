using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Stats
{
  // =========================================================
  // CAMPOS INTERNOS
  // =========================================================

  private readonly List<StatModification> activeModifications = new();

  // Dictionaries now use StatType instead of strings
  private Dictionary<StatType, float> _numstats = new();
  private Dictionary<StatType, bool> _boolstats = new();

  private Dictionary<StatType, float> numericBaseValues = new();
  private Dictionary<StatType, bool> boolBaseValues = new();

  // =========================================================
  // EVENTOS
  // =========================================================

  public UnityEvent<StatType, float> OnNumModified = new();
  public UnityEvent<StatType, bool> OnBoolModified = new();

  // =========================================================
  // REGISTRO  (Add / Remove)
  // =========================================================

  public bool AddStat<T>(StatType statType, T value)
    where T : IComparable
  {
    if (typeof(T) == typeof(float))
    {
      if (_numstats.ContainsKey(statType))
        return false;
      float val = Convert.ToSingle(value);
      _numstats[statType] = val;
      numericBaseValues[statType] = val;
      return true;
    }

    if (typeof(T) == typeof(bool))
    {
      if (_boolstats.ContainsKey(statType))
        return false;
      bool val = Convert.ToBoolean(value);
      _boolstats[statType] = val;
      boolBaseValues[statType] = val;
      return true;
    }

    Debug.LogWarning($"[Stats] Tipo não suportado: {typeof(T)}");
    return false;
  }

  public bool RemoveStat<T>(StatType statType)
    where T : IComparable
  {
    if (typeof(T) == typeof(float))
    {
      numericBaseValues.Remove(statType);
      return _numstats.Remove(statType);
    }

    if (typeof(T) == typeof(bool))
    {
      boolBaseValues.Remove(statType);
      return _boolstats.Remove(statType);
    }

    Debug.LogWarning($"[Stats] Tipo não suportado: {typeof(T)}");
    return false;
  }

  // =========================================================
  // LEITURA  (Get)
  // =========================================================

  public bool TryGetNum(StatType statType, out float value) =>
    _numstats.TryGetValue(statType, out value);

  public bool TryGetBool(StatType statType, out bool value) =>
    _boolstats.TryGetValue(statType, out value);

  public bool TryGetBaseNum(StatType statType, out float value) =>
    numericBaseValues.TryGetValue(statType, out value);

  // =========================================================
  // ESCRITA DIRETA  (Set)
  // =========================================================

  public void SetStat<T>(StatType statType, T value)
    where T : IComparable
  {
    if (typeof(T) == typeof(float) && _numstats.ContainsKey(statType))
    {
      _numstats[statType] = Convert.ToSingle(value);
      OnNumModified.Invoke(statType, _numstats[statType]);
      return;
    }

    if (typeof(T) == typeof(bool) && _boolstats.ContainsKey(statType))
    {
      _boolstats[statType] = Convert.ToBoolean(value);
      OnBoolModified.Invoke(statType, _boolstats[statType]);
    }
  }

  public bool SetBaseStat(StatType statType, float newBase)
  {
    if (!numericBaseValues.ContainsKey(statType))
      return false;

    numericBaseValues[statType] = newBase;
    _numstats[statType] = newBase;
    OnNumModified.Invoke(statType, newBase);
    return true;
  }

  // =========================================================
  // MODIFICAÇÃO VIA TIER
  // =========================================================

  public bool ModifyStatImmediate<T>(StatType statType, ModifyTYPE type, QualityTier tier)
    where T : IComparable
  {
    float multiplier = Tiers.GetMultiplier(tier); // Assuming Tiers class exists in your project
    float direction = type == ModifyTYPE.POSITIVE ? 1f : -1f;

    if (typeof(T) == typeof(float))
    {
      if (!_numstats.ContainsKey(statType))
        return false;
      float original = numericBaseValues[statType];
      SetStat(statType, original + original * (multiplier - 1f) * direction);
      activeModifications.Add(new StatModification(statType, tier, type, false));
      return true;
    }

    if (typeof(T) == typeof(bool))
    {
      if (!_boolstats.ContainsKey(statType))
        return false;
      SetStat(statType, type == ModifyTYPE.POSITIVE);
      activeModifications.Add(new StatModification(statType, tier, type, false));
      return true;
    }

    Debug.LogWarning($"[Stats] Tipo não suportado: {typeof(T)}");
    return false;
  }

  public IEnumerator ModifyStatCoroutine<T>(
    StatType statType,
    ModifyTYPE type,
    QualityTier tier,
    float duration
  )
    where T : IComparable
  {
    activeModifications.Add(new StatModification(statType, tier, type, true, duration));

    float multiplier = Tiers.GetMultiplier(tier);
    float direction = type == ModifyTYPE.POSITIVE ? 1f : -1f;

    if (typeof(T) == typeof(float))
    {
      if (!_numstats.ContainsKey(statType))
        yield break;
      float original = numericBaseValues[statType];
      SetStat(statType, original + original * (multiplier - 1f) * direction);
      yield return RunTimer(statType, duration);
      SetStat(statType, original);
    }
    else if (typeof(T) == typeof(bool))
    {
      if (!_boolstats.ContainsKey(statType))
        yield break;
      bool original = boolBaseValues[statType];
      SetStat(statType, type == ModifyTYPE.POSITIVE);
      yield return RunTimer(statType, duration);
      SetStat(statType, original);
    }

    activeModifications.RemoveAll(mod => mod.StatType == statType && mod.IsTemporary);
  }

  // =========================================================
  // MODIFICAÇÃO CUSTOMIZADA
  // =========================================================

  public bool ModifyStatByMultiplier(StatType statType, float multiplier)
  {
    if (!numericBaseValues.TryGetValue(statType, out float baseVal))
      return false;

    SetStat(statType, baseVal * multiplier);
    activeModifications.Add(
      new StatModification(statType, QualityTier.NONE, ModifyTYPE.CUSTOM, false)
    );
    return true;
  }

  public IEnumerator ModifyStatByMultiplierCoroutine(
    StatType statType,
    float multiplier,
    float duration
  )
  {
    if (!numericBaseValues.TryGetValue(statType, out float baseVal))
      yield break;

    activeModifications.Add(
      new StatModification(statType, QualityTier.NONE, ModifyTYPE.CUSTOM, true, duration)
    );
    SetStat(statType, baseVal * multiplier);
    yield return RunTimer(statType, duration);
    SetStat(statType, baseVal);
    activeModifications.RemoveAll(mod => mod.StatType == statType && mod.IsTemporary);
  }

  public bool ModifyStatToTarget(StatType statType, float targetValue)
  {
    if (!_numstats.ContainsKey(statType))
      return false;

    SetStat(statType, targetValue);
    activeModifications.Add(
      new StatModification(statType, QualityTier.NONE, ModifyTYPE.CUSTOM, false)
    );
    return true;
  }

  public IEnumerator ModifyStatToTargetCoroutine(
    StatType statType,
    float targetValue,
    float duration
  )
  {
    if (!numericBaseValues.TryGetValue(statType, out float baseVal))
      yield break;

    activeModifications.Add(
      new StatModification(statType, QualityTier.NONE, ModifyTYPE.CUSTOM, true, duration)
    );
    SetStat(statType, targetValue);
    yield return RunTimer(statType, duration);
    SetStat(statType, baseVal);
    activeModifications.RemoveAll(mod => mod.StatType == statType && mod.IsTemporary);
  }

  public bool ModifyStatByDelta(StatType statType, float delta)
  {
    if (!_numstats.TryGetValue(statType, out float current))
      return false;

    SetStat(statType, current + delta);
    activeModifications.Add(
      new StatModification(statType, QualityTier.NONE, ModifyTYPE.CUSTOM, false)
    );
    return true;
  }

  public IEnumerator ModifyStatByDeltaCoroutine(StatType statType, float delta, float duration)
  {
    if (!numericBaseValues.TryGetValue(statType, out float baseVal))
      yield break;

    activeModifications.Add(
      new StatModification(statType, QualityTier.NONE, ModifyTYPE.CUSTOM, true, duration)
    );
    SetStat(statType, baseVal + delta);
    yield return RunTimer(statType, duration);
    SetStat(statType, baseVal);
    activeModifications.RemoveAll(mod => mod.StatType == statType && mod.IsTemporary);
  }

  // =========================================================
  // GERENCIAMENTO DE MODIFICAÇÕES ATIVAS
  // =========================================================

  public void RemoveActiveModifications(StatType statType)
  {
    activeModifications.RemoveAll(mod => mod.StatType == statType);

    if (numericBaseValues.ContainsKey(statType))
      SetStat(statType, numericBaseValues[statType]);
    else if (boolBaseValues.ContainsKey(statType))
      SetStat(statType, boolBaseValues[statType]);
  }

  public IReadOnlyList<StatModification> GetActiveModifications() =>
    activeModifications.AsReadOnly();

  // =========================================================
  // SERIALIZAÇÃO
  // =========================================================

  public Dictionary<StatType, float> GetNumericStats() => new(_numstats);

  public Dictionary<StatType, bool> GetBoolStats() => new(_boolstats);

  public void LoadFromDictionaries(
    Dictionary<StatType, float> nums,
    Dictionary<StatType, bool> bools
  )
  {
    _numstats = new(nums);
    _boolstats = new(bools);
    numericBaseValues = new(nums);
    boolBaseValues = new(bools);
  }

  // =========================================================
  // HELPERS PRIVADOS
  // =========================================================

  private IEnumerator RunTimer(StatType statType, float duration)
  {
    float timer = duration;
    while (timer > 0f)
    {
      timer -= Time.deltaTime;
      UpdateTemporaryTime(statType, timer);
      yield return null;
    }
  }

  private void UpdateTemporaryTime(StatType statType, float timeLeft)
  {
    for (int i = 0; i < activeModifications.Count; i++)
    {
      if (activeModifications[i].StatType == statType && activeModifications[i].IsTemporary)
      {
        var updated = activeModifications[i];
        updated.RemainingTime = timeLeft;
        activeModifications[i] = updated;
      }
    }
  }
}
