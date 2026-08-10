using System;
using System.Collections.Generic;
using System.Linq;
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

  private readonly Dictionary<StatType, float> _numstats = new();
  private readonly Dictionary<StatType, bool> _boolstats = new();
  private readonly Dictionary<StatType, float> numericBaseValues = new();
  private readonly Dictionary<StatType, bool> boolBaseValues = new();

  private readonly Dictionary<StatType, List<ActiveMultiplier>> _activeMultipliers = new();
  private readonly Dictionary<StatType, List<ActiveDelta>> _activeDeltas = new();

  private bool _disposed;

  // =========================================================
  // STRUCTS
  // =========================================================

  private struct ActiveMultiplier
  {
    public float Value;
    public CancellationTokenSource Cts;
    public float ExpireTime;
    public string SourceId;
  }

  private struct ActiveDelta
  {
    public float Value;
    public CancellationTokenSource Cts;
    public float ExpireTime;
    public string SourceId;
  }

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

    foreach (var multipliers in _activeMultipliers.Values)
    {
      foreach (var m in multipliers)
      {
        m.Cts?.Cancel();
        m.Cts?.Dispose();
      }
    }
    _activeMultipliers.Clear();

    foreach (var deltas in _activeDeltas.Values)
    {
      foreach (var d in deltas)
      {
        d.Cts?.Cancel();
        d.Cts?.Dispose();
      }
    }
    _activeDeltas.Clear();

    OnNumModified?.RemoveAllListeners();
    OnBoolModified?.RemoveAllListeners();
  }

  // =========================================================
  // REGISTER (Add / Remove)
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
    CancelAllForStat(statType);

    if (typeof(T) == typeof(float))
    {
      numericBaseValues.Remove(statType);
      _activeMultipliers.Remove(statType);
      _activeDeltas.Remove(statType);
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
  // READ (Get)
  // =========================================================

  public bool TryGetNum(StatType statType, out float value) =>
    _numstats.TryGetValue(statType, out value);

  public bool TryGetBool(StatType statType, out bool value) =>
    _boolstats.TryGetValue(statType, out value);

  public bool TryGetBaseNum(StatType statType, out float value) =>
    numericBaseValues.TryGetValue(statType, out value);

  public float GetCurrentValue(StatType statType)
  {
    if (!numericBaseValues.TryGetValue(statType, out float baseVal))
      return 0f;

    float totalMultiplier = 1f;
    if (_activeMultipliers.TryGetValue(statType, out var multipliers))
    {
      foreach (var m in multipliers)
      {
        totalMultiplier *= m.Value;
      }
    }

    float totalDelta = 0f;
    if (_activeDeltas.TryGetValue(statType, out var deltas))
    {
      foreach (var d in deltas)
      {
        totalDelta += d.Value;
      }
    }

    return baseVal * totalMultiplier + totalDelta;
  }

  // =========================================================
  // DIRECT WRITE (Set)
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
    RecalculateStat(statType);
    return true;
  }

  // =========================================================
  // MULTIPLIER MODIFICATIONS (Stackable)
  // =========================================================

  public string ApplyMultiplier(StatType statType, float multiplier)
  {
    if (!numericBaseValues.ContainsKey(statType))
      return null;

    string sourceId = Guid.NewGuid().ToString();

    var activeMult = new ActiveMultiplier
    {
      Value = multiplier,
      Cts = new CancellationTokenSource(),
      ExpireTime = float.MaxValue,
      SourceId = sourceId,
    };

    if (!_activeMultipliers.ContainsKey(statType))
      _activeMultipliers[statType] = new List<ActiveMultiplier>();

    _activeMultipliers[statType].Add(activeMult);
    RecalculateStat(statType);

    return sourceId;
  }

  public async Task<string> ApplyMultiplierAsync(
    StatType statType,
    float multiplier,
    float duration,
    CancellationToken externalToken = default
  )
  {
    if (!numericBaseValues.ContainsKey(statType))
      return null;

    if (duration <= 0f || float.IsInfinity(duration) || float.IsNaN(duration))
    {
      return ApplyMultiplier(statType, multiplier);
    }

    string sourceId = Guid.NewGuid().ToString();
    var cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);

    var activeMult = new ActiveMultiplier
    {
      Value = multiplier,
      Cts = cts,
      ExpireTime = Time.time + duration,
      SourceId = sourceId,
    };

    if (!_activeMultipliers.ContainsKey(statType))
      _activeMultipliers[statType] = new List<ActiveMultiplier>();

    _activeMultipliers[statType].Add(activeMult);
    RecalculateStat(statType);

    try
    {
      float elapsed = 0f;
      while (elapsed < duration)
      {
        cts.Token.ThrowIfCancellationRequested();
        elapsed += Time.deltaTime;
        await Task.Yield();
      }

      RemoveMultiplier(statType, sourceId);
      return sourceId;
    }
    catch (OperationCanceledException)
    {
      RemoveMultiplier(statType, sourceId);
      throw;
    }
    finally
    {
      cts.Dispose();
    }
  }

  public bool RemoveMultiplier(StatType statType, string sourceId)
  {
    if (string.IsNullOrEmpty(sourceId))
      return false;
    if (!_activeMultipliers.TryGetValue(statType, out var multipliers))
      return false;

    var toRemove = multipliers.FirstOrDefault(m => m.SourceId == sourceId);
    if (toRemove.Cts == null)
      return false;

    multipliers.Remove(toRemove);
    toRemove.Cts?.Cancel();
    toRemove.Cts?.Dispose();

    if (multipliers.Count == 0)
      _activeMultipliers.Remove(statType);

    RecalculateStat(statType);
    return true;
  }

  // =========================================================
  // DELTA MODIFICATIONS (Stackable)
  // =========================================================

  public string ApplyDelta(StatType statType, float delta)
  {
    if (!numericBaseValues.ContainsKey(statType))
      return null;

    string sourceId = Guid.NewGuid().ToString();

    var activeDelta = new ActiveDelta
    {
      Value = delta,
      Cts = new CancellationTokenSource(),
      ExpireTime = float.MaxValue,
      SourceId = sourceId,
    };

    if (!_activeDeltas.ContainsKey(statType))
      _activeDeltas[statType] = new List<ActiveDelta>();

    _activeDeltas[statType].Add(activeDelta);
    RecalculateStat(statType);

    return sourceId;
  }

  public async Task<string> ApplyDeltaAsync(
    StatType statType,
    float delta,
    float duration,
    CancellationToken externalToken = default
  )
  {
    if (!numericBaseValues.ContainsKey(statType))
      return null;

    if (duration <= 0f || float.IsInfinity(duration) || float.IsNaN(duration))
    {
      return ApplyDelta(statType, delta);
    }

    string sourceId = Guid.NewGuid().ToString();
    var cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);

    var activeDelta = new ActiveDelta
    {
      Value = delta,
      Cts = cts,
      ExpireTime = Time.time + duration,
      SourceId = sourceId,
    };

    if (!_activeDeltas.ContainsKey(statType))
      _activeDeltas[statType] = new List<ActiveDelta>();

    _activeDeltas[statType].Add(activeDelta);
    RecalculateStat(statType);

    try
    {
      float elapsed = 0f;
      while (elapsed < duration)
      {
        cts.Token.ThrowIfCancellationRequested();
        elapsed += Time.deltaTime;
        await Task.Yield();
      }

      RemoveDelta(statType, sourceId);
      return sourceId;
    }
    catch (OperationCanceledException)
    {
      RemoveDelta(statType, sourceId);
      throw;
    }
    finally
    {
      cts.Dispose();
    }
  }

  public bool RemoveDelta(StatType statType, string sourceId)
  {
    if (string.IsNullOrEmpty(sourceId))
      return false;
    if (!_activeDeltas.TryGetValue(statType, out var deltas))
      return false;

    var toRemove = deltas.FirstOrDefault(d => d.SourceId == sourceId);
    if (toRemove.Cts == null)
      return false;

    deltas.Remove(toRemove);
    toRemove.Cts?.Cancel();
    toRemove.Cts?.Dispose();

    if (deltas.Count == 0)
      _activeDeltas.Remove(statType);

    RecalculateStat(statType);
    return true;
  }

  // =========================================================
  // TARGET VALUE (Set para um valor específico, temporário)
  // =========================================================

  public async Task<string> SetToTargetAsync(
    StatType statType,
    float targetValue,
    float duration,
    CancellationToken externalToken = default
  )
  {
    if (!numericBaseValues.ContainsKey(statType))
      return null;

    float currentValue = GetCurrentValue(statType);
    float delta = targetValue - currentValue;

    return await ApplyDeltaAsync(statType, delta, duration, externalToken);
  }

  // =========================================================
  // BOOL MODIFICATIONS
  // =========================================================

  public bool SetBool(StatType statType, bool value)
  {
    if (!_boolstats.ContainsKey(statType))
      return false;

    _boolstats[statType] = value;
    OnBoolModified?.Invoke(statType, value);
    return true;
  }

  public bool ToggleBool(StatType statType)
  {
    if (!_boolstats.TryGetValue(statType, out bool current))
      return false;

    return SetBool(statType, !current);
  }

  public async Task<bool> SetBoolAsync(
    StatType statType,
    bool value,
    float duration,
    CancellationToken externalToken = default
  )
  {
    if (!_boolstats.ContainsKey(statType))
      return false;

    bool original = boolBaseValues[statType];

    SetBool(statType, value);

    try
    {
      float elapsed = 0f;
      while (elapsed < duration)
      {
        externalToken.ThrowIfCancellationRequested();
        elapsed += Time.deltaTime;
        await Task.Yield();
      }

      SetBool(statType, original);
      return true;
    }
    catch (OperationCanceledException)
    {
      SetBool(statType, original);
      throw;
    }
  }

  // =========================================================
  // CANCELLATION
  // =========================================================

  public void CancelAllForStat(StatType statType)
  {
    if (_activeMultipliers.TryGetValue(statType, out var multipliers))
    {
      foreach (var m in multipliers)
      {
        m.Cts?.Cancel();
        m.Cts?.Dispose();
      }
      _activeMultipliers.Remove(statType);
    }

    if (_activeDeltas.TryGetValue(statType, out var deltas))
    {
      foreach (var d in deltas)
      {
        d.Cts?.Cancel();
        d.Cts?.Dispose();
      }
      _activeDeltas.Remove(statType);
    }

    if (numericBaseValues.ContainsKey(statType))
    {
      _numstats[statType] = numericBaseValues[statType];
      OnNumModified?.Invoke(statType, numericBaseValues[statType]);
    }
  }

  public void CancelAllModifications()
  {
    var allStats = new List<StatType>();
    allStats.AddRange(_activeMultipliers.Keys);
    allStats.AddRange(_activeDeltas.Keys);

    foreach (var statType in allStats.Distinct())
    {
      CancelAllForStat(statType);
    }
  }

  // =========================================================
  // QUERIES
  // =========================================================

  public int GetActiveMultiplierCount(StatType statType) =>
    _activeMultipliers.TryGetValue(statType, out var m) ? m.Count : 0;

  public int GetActiveDeltaCount(StatType statType) =>
    _activeDeltas.TryGetValue(statType, out var d) ? d.Count : 0;

  public bool HasActiveModifications(StatType statType) =>
    GetActiveMultiplierCount(statType) > 0 || GetActiveDeltaCount(statType) > 0;

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
    _activeMultipliers.Clear();
    _activeDeltas.Clear();

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

  private void RecalculateStat(StatType statType)
  {
    if (!numericBaseValues.ContainsKey(statType))
      return;

    float newValue = GetCurrentValue(statType);
    _numstats[statType] = newValue;
    OnNumModified?.Invoke(statType, newValue);
  }
}
