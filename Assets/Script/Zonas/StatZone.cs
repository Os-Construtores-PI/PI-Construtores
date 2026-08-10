using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum ExitBehavior
{
  CancelImmediately,
  PersistForDuration,
  PersistWithGracePeriod,
}

public class StatZone : MonoBehaviour
{
  [Header("Configuração do Efeito")]
  [SerializeField]
  private StatType _statType;

  [Tooltip("Tipo de modificação: Multiplier (x2.0, x0.5) ou Delta (+10, -5)")]
  [SerializeField]
  private ModifyType _modifyType = ModifyType.Multiplier;

  [Tooltip(
    "Valor da modificação. Para Multiplier: 2.0 = dobro, 0.5 = metade. Para Delta: valor a somar/subtrair."
  )]
  [SerializeField]
  private float _value = 0.5f;

  [Header("Modo de Operação")]
  [Tooltip("Temporary = efeito reverte após duração. Permanent = altera valor base para sempre.")]
  [SerializeField]
  private TimeType _timeType = TimeType.Temporary;

  [Header("Comportamento na Saída (só para Temporary)")]
  [Tooltip(
    "CancelImmediately = some na hora. PersistForDuration = dura o tempo do effectDuration. PersistWithGracePeriod = dura o gracePeriod após sair."
  )]
  [SerializeField]
  private ExitBehavior _exitBehavior = ExitBehavior.PersistWithGracePeriod;

  [Header("Durações")]
  [Tooltip("Para Temporary: tempo total do efeito (ou grace period).")]
  [SerializeField]
  private float _effectDuration = 5f;

  [Tooltip("Cooldown antes que a mesma entidade possa reativar a zona.")]
  [SerializeField]
  private float _statCooldown = 10f;

  [Header("Debug")]
  [SerializeField]
  private bool _debug = false;

  private readonly Dictionary<CombatEntities, EntityEffectData> _activeEffects = new();
  private readonly Dictionary<CombatEntities, float> _cooldowns = new();

  private struct EntityEffectData
  {
    public CancellationTokenSource Cts;
    public bool IsInside;
    public float ExitTime;
    public string SourceId;
  }

  private void Awake()
  {
    if (_timeType == TimeType.Permanent && _exitBehavior != ExitBehavior.CancelImmediately)
    {
      if (_debug)
        Debug.LogWarning($"[StatZone] {_statType} é Permanent — _exitBehavior será ignorado.");
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    if (!IsValidEntity(other, out var entity))
      return;
    if (IsOnCooldown(entity))
      return;

    if (_activeEffects.TryGetValue(entity, out var existing))
    {
      existing.IsInside = true;
      _activeEffects[entity] = existing;
      return;
    }

    var cts = new CancellationTokenSource();
    _activeEffects[entity] = new EntityEffectData
    {
      Cts = cts,
      IsInside = true,
      ExitTime = -1f,
      SourceId = null,
    };

    _ = RunEffectLifecycleAsync(entity, cts);
  }

  private void OnTriggerExit(Collider other)
  {
    if (!IsValidEntity(other, out var entity))
      return;
    if (!_activeEffects.TryGetValue(entity, out var data))
      return;

    data.IsInside = false;
    data.ExitTime = Time.time;
    _activeEffects[entity] = data;

    if (_timeType == TimeType.Permanent)
    {
      _activeEffects.Remove(entity);
      data.Cts?.Dispose();
      return;
    }

    if (_exitBehavior == ExitBehavior.CancelImmediately)
    {
      data.Cts?.Cancel();
    }
  }

  private void OnDestroy()
  {
    foreach (var data in _activeEffects.Values)
    {
      data.Cts?.Cancel();
      data.Cts?.Dispose();
    }
    _activeEffects.Clear();
    _cooldowns.Clear();
  }

  private async Task RunEffectLifecycleAsync(CombatEntities entity, CancellationTokenSource cts)
  {
    Stats stats = entity.Stats;
    if (stats == null)
      return;

    string sourceId = null;

    try
    {
      if (_debug)
        Debug.Log(
          $"[StatZone] +{_statType} ({_modifyType}: {_value}) em {entity.name} | Mode: {_timeType} | Exit: {_exitBehavior}"
        );

      if (_timeType == TimeType.Temporary)
      {
        sourceId = await ApplyTemporaryEffectAsync(stats, cts.Token);

        // Atualiza o sourceId no dicionário
        if (_activeEffects.TryGetValue(entity, out var data))
        {
          data.SourceId = sourceId;
          _activeEffects[entity] = data;
        }
      }
      else
      {
        ApplyPermanentEffect(stats);
      }

      if (_timeType == TimeType.Temporary)
      {
        await WaitForEffectEndAsync(entity, cts.Token);
      }

      _cooldowns[entity] = Time.time + _statCooldown;
    }
    catch (OperationCanceledException)
    {
      if (_debug)
        Debug.Log($"[StatZone] -{_statType} cancelado para {entity.name}");
    }
    finally
    {
      if (!string.IsNullOrEmpty(sourceId))
      {
        if (_modifyType == ModifyType.Multiplier)
        {
          stats.RemoveMultiplier(_statType, sourceId);
        }
        else
        {
          stats.RemoveDelta(_statType, sourceId);
        }
      }

      if (_activeEffects.TryGetValue(entity, out var current) && current.Cts == cts)
      {
        _activeEffects.Remove(entity);
      }
      cts.Dispose();
    }
  }

  private async Task<string> ApplyTemporaryEffectAsync(Stats stats, CancellationToken ct)
  {
    if (_modifyType == ModifyType.Multiplier)
    {
      return await stats.ApplyMultiplierAsync(_statType, _value, _effectDuration, ct);
    }
    else
    {
      return await stats.ApplyDeltaAsync(_statType, _value, _effectDuration, ct);
    }
  }

  private void ApplyPermanentEffect(Stats stats)
  {
    if (_modifyType == ModifyType.Multiplier)
    {
      if (stats.TryGetBaseNum(_statType, out float baseValue))
      {
        float newBase = baseValue * _value;
        stats.SetBaseStat(_statType, newBase);
      }
    }
    else
    {
      if (stats.TryGetBaseNum(_statType, out float baseValue))
      {
        float newBase = baseValue + _value;
        stats.SetBaseStat(_statType, newBase);
      }
    }
  }

  private async Task WaitForEffectEndAsync(CombatEntities entity, CancellationToken ct)
  {
    switch (_exitBehavior)
    {
      case ExitBehavior.CancelImmediately:
        await Task.Delay(Timeout.Infinite, ct);
        break;

      case ExitBehavior.PersistForDuration:
        await Task.Delay(Timeout.Infinite, ct);
        break;

      case ExitBehavior.PersistWithGracePeriod:
        await WaitWithGracePeriodAsync(entity, ct);
        break;
    }
  }

  private async Task WaitWithGracePeriodAsync(CombatEntities entity, CancellationToken ct)
  {
    while (IsEntityInside(entity))
    {
      ct.ThrowIfCancellationRequested();
      await Task.Yield();
    }

    if (_debug)
      Debug.Log($"[StatZone] {entity.name} saiu da zona. Grace period: {_effectDuration}s");

    float exitTime = Time.time;
    while (Time.time - exitTime < _effectDuration)
    {
      ct.ThrowIfCancellationRequested();

      if (IsEntityInside(entity))
      {
        if (_debug)
          Debug.Log($"[StatZone] {entity.name} voltou — grace cancelado.");
        return;
      }

      await Task.Yield();
    }

    if (_debug)
      Debug.Log($"[StatZone] Grace period expirado para {entity.name}");
  }

  private bool IsEntityInside(CombatEntities entity) =>
    _activeEffects.TryGetValue(entity, out var data) && data.IsInside;

  private bool IsValidEntity(Collider other, out CombatEntities entity)
  {
    entity = null;
    return other.TryGetComponent(out entity) && entity.Stats != null;
  }

  private bool IsOnCooldown(CombatEntities entity)
  {
    if (_cooldowns.TryGetValue(entity, out float cooldownEnd))
    {
      if (Time.time < cooldownEnd)
        return true;
      _cooldowns.Remove(entity);
    }
    return false;
  }

  private void OnDrawGizmos()
  {
    Collider col = GetComponent<Collider>();
    if (col == null)
      return;

    Color baseColor =
      _timeType == TimeType.Permanent ? new Color(0.6f, 0.3f, 1f)
      : _exitBehavior == ExitBehavior.CancelImmediately ? new Color(1f, 0.3f, 0.3f)
      : _exitBehavior == ExitBehavior.PersistForDuration ? new Color(0.3f, 1f, 0.3f)
      : new Color(0.3f, 0.6f, 1f);

    if (_activeEffects.Count > 0)
      baseColor *= 1.5f;

    Gizmos.color = baseColor;
    Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);

    foreach (var kvp in _activeEffects)
    {
      var entity = kvp.Key;
      var data = kvp.Value;

      if (!data.IsInside && _exitBehavior == ExitBehavior.PersistWithGracePeriod)
      {
        Gizmos.color = Color.yellow;
        Vector3 midPoint = (transform.position + entity.transform.position) * 0.5f;
        Gizmos.DrawLine(transform.position, midPoint);
        Gizmos.color = Color.Lerp(Color.yellow, Color.red, 0.5f);
        Gizmos.DrawLine(midPoint, entity.transform.position);
      }
      else if (data.IsInside)
      {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, entity.transform.position);
      }
    }
  }
}
