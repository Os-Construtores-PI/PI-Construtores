using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Stats : IDisposable
{
  // =========================================================
  // INTERNAL FIELDS
  // =========================================================

  private readonly List<StatModification> activeModifications = new();
  private readonly Dictionary<StatType, float> _numstats = new();
  private readonly Dictionary<StatType, bool> _boolstats = new();
  private readonly Dictionary<StatType, float> numericBaseValues = new();
  private readonly Dictionary<StatType, bool> boolBaseValues = new();

  private readonly Dictionary<StatType, CancellationTokenSource> _activeTokens = new();

  private bool _disposed;

  // =========================================================
  // EVENTS
  // =========================================================

  public UnityEvent<StatType, float> OnNumModified = new();
  public UnityEvent<StatType, bool> OnBoolModified = new();

  // =========================================================
  // DISPOSAL
  // =========================================================

  public void Dispose()
  {
    if (_disposed)
      return;
    _disposed = true;

    foreach (var cts in _activeTokens.Values)
    {
      cts?.Cancel();
      cts?.Dispose();
    }
    _activeTokens.Clear();
    activeModifications.Clear();

    OnNumModified?.RemoveAllListeners();
    OnBoolModified?.RemoveAllListeners();
  }

  // =========================================================
  // REGISTER  (Add / Remove)
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
    CancelModifications(statType);

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
  // READ  (Get)
  // =========================================================

  public bool TryGetNum(StatType statType, out float value) =>
    _numstats.TryGetValue(statType, out value);

  public bool TryGetBool(StatType statType, out bool value) =>
    _boolstats.TryGetValue(statType, out value);

  public bool TryGetBaseNum(StatType statType, out float value) =>
    numericBaseValues.TryGetValue(statType, out value);

  // =========================================================
  // DIRECT WRITE  (Set)
  // =========================================================

  public void SetStat<T>(StatType statType, T value)
    where T : IComparable
  {
    if (typeof(T) == typeof(float) && _numstats.ContainsKey(statType))
    {
      _numstats[statType] = Convert.ToSingle(value);
      OnNumModified?.Invoke(statType, _numstats[statType]);
      return;
    }

    if (typeof(T) == typeof(bool) && _boolstats.ContainsKey(statType))
    {
      _boolstats[statType] = Convert.ToBoolean(value);
      OnBoolModified?.Invoke(statType, _boolstats[statType]);
    }
  }

  public bool SetBaseStat(StatType statType, float newBase)
  {
    if (!numericBaseValues.ContainsKey(statType))
      return false;

    numericBaseValues[statType] = newBase;
    _numstats[statType] = newBase;
    OnNumModified?.Invoke(statType, newBase);
    return true;
  }

  // =========================================================
  // MODIFICATION TIER — ASYNC
  // =========================================================

  public bool ModifyStatImmediate<T>(StatType statType, ModifyType type, QualityTier tier)
    where T : IComparable
  {
    float multiplier = Tiers.GetMultiplier(tier);
    float direction = type == ModifyType.Positive ? 1f : -1f;

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
      SetStat(statType, type == ModifyType.Positive);
      activeModifications.Add(new StatModification(statType, tier, type, false));
      return true;
    }

    Debug.LogWarning($"[Stats] Tipo não suportado: {typeof(T)}");
    return false;
  }

  public async Task<bool> ModifyStatAsync<T>(
    StatType statType,
    ModifyType type,
    QualityTier tier,
    float duration,
    CancellationToken externalToken = default
  )
    where T : IComparable
  {
    if (duration <= 0f)
      return ModifyStatImmediate<T>(statType, type, tier);

    CancelModifications(statType);

    float multiplier = Tiers.GetMultiplier(tier);
    float direction = type == ModifyType.Positive ? 1f : -1f;

    using var cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
    _activeTokens[statType] = cts;

    try
    {
      if (typeof(T) == typeof(float))
      {
        if (!_numstats.ContainsKey(statType))
          return false;
        float original = numericBaseValues[statType];
        float modifiedValue = original + original * (multiplier - 1f) * direction;

        SetStat(statType, modifiedValue);
        activeModifications.Add(new StatModification(statType, tier, type, true, duration, cts));

        await RunTimerAsync(statType, duration, cts.Token);

        if (!cts.Token.IsCancellationRequested)
        {
          SetStat(statType, original);
        }
        return true;
      }

      if (typeof(T) == typeof(bool))
      {
        if (!_boolstats.ContainsKey(statType))
          return false;
        bool original = boolBaseValues[statType];

        SetStat(statType, type == ModifyType.Positive);
        activeModifications.Add(new StatModification(statType, tier, type, true, duration, cts));

        await RunTimerAsync(statType, duration, cts.Token);

        if (!cts.Token.IsCancellationRequested)
        {
          SetStat(statType, original);
        }
        return true;
      }

      Debug.LogWarning($"[Stats] Tipo não suportado: {typeof(T)}");
      return false;
    }
    catch (OperationCanceledException)
    {
      if (typeof(T) == typeof(float) && numericBaseValues.ContainsKey(statType))
        SetStat(statType, numericBaseValues[statType]);
      else if (typeof(T) == typeof(bool) && boolBaseValues.ContainsKey(statType))
        SetStat(statType, boolBaseValues[statType]);

      return false;
    }
    finally
    {
      activeModifications.RemoveAll(mod => mod.StatType == statType && mod.IsTemporary);
      _activeTokens.Remove(statType);
      cts.Dispose();
    }
  }

  // =========================================================
  // MODIFICATION CUSTOMIZED — ASYNC
  // =========================================================

  public bool ModifyStatByMultiplier(StatType statType, float multiplier)
  {
    if (!numericBaseValues.TryGetValue(statType, out float baseVal))
      return false;

    SetStat(statType, baseVal * multiplier);
    activeModifications.Add(
      new StatModification(statType, QualityTier.NONE, ModifyType.Custom, false)
    );
    return true;
  }

  public async Task<bool> ModifyStatByMultiplierAsync(
    StatType statType,
    float multiplier,
    float duration,
    CancellationToken ct = default
  )
  {
    if (!numericBaseValues.TryGetValue(statType, out float baseVal))
      return false;
    if (duration <= 0f)
      return ModifyStatByMultiplier(statType, multiplier);

    CancelModifications(statType);
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    _activeTokens[statType] = cts;

    try
    {
      activeModifications.Add(
        new StatModification(statType, QualityTier.NONE, ModifyType.Custom, true, duration, cts)
      );
      SetStat(statType, baseVal * multiplier);

      await RunTimerAsync(statType, duration, cts.Token);

      if (!cts.Token.IsCancellationRequested)
        SetStat(statType, baseVal);

      return true;
    }
    catch (OperationCanceledException)
    {
      SetStat(statType, baseVal);
      return false;
    }
    finally
    {
      activeModifications.RemoveAll(mod => mod.StatType == statType && mod.IsTemporary);
      _activeTokens.Remove(statType);
    }
  }

  public bool ModifyStatToTarget(StatType statType, float targetValue)
  {
    if (!_numstats.ContainsKey(statType))
      return false;

    SetStat(statType, targetValue);
    activeModifications.Add(
      new StatModification(statType, QualityTier.NONE, ModifyType.Custom, false)
    );
    return true;
  }

  public async Task<bool> ModifyStatToTargetAsync(
    StatType statType,
    float targetValue,
    float duration,
    CancellationToken ct = default
  )
  {
    if (!numericBaseValues.TryGetValue(statType, out float baseVal))
      return false;
    if (duration <= 0f)
      return ModifyStatToTarget(statType, targetValue);

    CancelModifications(statType);
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    _activeTokens[statType] = cts;

    try
    {
      activeModifications.Add(
        new StatModification(statType, QualityTier.NONE, ModifyType.Custom, true, duration, cts)
      );
      SetStat(statType, targetValue);

      await RunTimerAsync(statType, duration, cts.Token);

      if (!cts.Token.IsCancellationRequested)
        SetStat(statType, baseVal);

      return true;
    }
    catch (OperationCanceledException)
    {
      SetStat(statType, baseVal);
      return false;
    }
    finally
    {
      activeModifications.RemoveAll(mod => mod.StatType == statType && mod.IsTemporary);
      _activeTokens.Remove(statType);
    }
  }

  public bool ModifyStatByDelta(StatType statType, float delta)
  {
    if (!_numstats.TryGetValue(statType, out float current))
      return false;

    SetStat(statType, current + delta);
    activeModifications.Add(
      new StatModification(statType, QualityTier.NONE, ModifyType.Custom, false)
    );
    return true;
  }

  public async Task<bool> ModifyStatByDeltaAsync(
    StatType statType,
    float delta,
    float duration,
    CancellationToken ct = default
  )
  {
    if (!numericBaseValues.TryGetValue(statType, out float baseVal))
      return false;
    if (duration <= 0f)
      return ModifyStatByDelta(statType, delta);

    CancelModifications(statType);
    using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
    _activeTokens[statType] = cts;

    try
    {
      activeModifications.Add(
        new StatModification(statType, QualityTier.NONE, ModifyType.Custom, true, duration, cts)
      );
      SetStat(statType, baseVal + delta);

      await RunTimerAsync(statType, duration, cts.Token);

      if (!cts.Token.IsCancellationRequested)
        SetStat(statType, baseVal);

      return true;
    }
    catch (OperationCanceledException)
    {
      SetStat(statType, baseVal);
      return false;
    }
    finally
    {
      activeModifications.RemoveAll(mod => mod.StatType == statType && mod.IsTemporary);
      _activeTokens.Remove(statType);
    }
  }

  // =========================================================
  // ACTIVE MODIFICATIONS MANAGEMENT
  // =========================================================

  public void CancelModifications(StatType statType)
  {
    if (_activeTokens.TryGetValue(statType, out var cts))
    {
      cts?.Cancel();
    }

    activeModifications.RemoveAll(mod => mod.StatType == statType);

    if (numericBaseValues.ContainsKey(statType))
      SetStat(statType, numericBaseValues[statType]);
    else if (boolBaseValues.ContainsKey(statType))
      SetStat(statType, boolBaseValues[statType]);
  }

  public void CancelAllModifications()
  {
    foreach (var cts in _activeTokens.Values)
    {
      cts?.Cancel();
    }
    _activeTokens.Clear();
    activeModifications.Clear();
  }

  public IReadOnlyList<StatModification> GetActiveModifications() =>
    activeModifications.AsReadOnly();

  // =========================================================
  // SERIALIZATION
  // =========================================================

  public Dictionary<StatType, float> GetNumericStats() => new(_numstats);

  public Dictionary<StatType, bool> GetBoolStats() => new(_boolstats);

  public void LoadFromDictionaries(
    Dictionary<StatType, float> nums,
    Dictionary<StatType, bool> bools
  )
  {
    CancelAllModifications();

    _numstats.Clear();
    _boolstats.Clear();
    numericBaseValues.Clear();
    boolBaseValues.Clear();

    foreach (var kvp in nums)
    {
      _numstats[kvp.Key] = kvp.Value;
      numericBaseValues[kvp.Key] = kvp.Value;
    }

    foreach (var kvp in bools)
    {
      _boolstats[kvp.Key] = kvp.Value;
      boolBaseValues[kvp.Key] = kvp.Value;
    }
  }

  // =========================================================
  // PRIVATE HELPERS
  // =========================================================

  private async Task RunTimerAsync(StatType statType, float duration, CancellationToken ct)
  {
    float elapsed = 0f;

    while (elapsed < duration)
    {
      ct.ThrowIfCancellationRequested();

      elapsed += Time.deltaTime;
      float remaining = Mathf.Max(0f, duration - elapsed);
      UpdateTemporaryTime(statType, remaining);

      await Task.Yield();
    }
  }

  private void UpdateTemporaryTime(StatType statType, float timeLeft)
  {
    for (int i = 0; i < activeModifications.Count; i++)
    {
      if (activeModifications[i].StatType == statType && activeModifications[i].IsTemporary)
      {
        var old = activeModifications[i];
        activeModifications[i] = new StatModification(
          old.StatType,
          old.Tier,
          old.ModifyType,
          true,
          timeLeft,
          old.CancellationSource
        );
      }
    }
  }
}
