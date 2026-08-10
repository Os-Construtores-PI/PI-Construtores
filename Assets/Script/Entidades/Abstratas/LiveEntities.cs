using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public abstract class LiveEntities : Entities
{
  [Header("Debug - Vida Atual")]
  [SerializeField]
  private float _currentHealthDebug;

  #region --- Atributos de Vida ---

  [Header("Atributos de Vida")]
  [SerializeField, Min(1f)]
  protected float _maxHealth = 100f;

  private float _health;

  [HideInInspector]
  [Stat(StatType.Health)]
  public float Health
  {
    get => _health;
    set
    {
      float oldHealth = _health;
      _health = Mathf.Clamp(value, 0f, MaxHealth);

      if (_health < oldHealth)
      {
        _OnDamage.Invoke();
      }
      _OnHealthChanged.Invoke(_health / MaxHealth);

      if (_health <= 0f)
      {
        _OnDeath.Invoke();
      }
    }
  }

  [HideInInspector]
  [Stat(StatType.MaxHealth)]
  public float MaxHealth
  {
    get => _maxHealth;
    set => _maxHealth = Mathf.Max(1f, value);
  }

  #endregion

  #region --- Eventos ---

  public readonly UnityEvent<float> _OnHealthChanged = new();
  public readonly UnityEvent _OnDamage = new();
  public readonly UnityEvent _OnDeath = new();

  #endregion

  #region --- Sistema de Stats via reflexão ---

  public Stats Stats = new();

  public Dictionary<StatType, Action<float>> numericStatSetters = new();
  public Dictionary<StatType, Action<bool>> boolStatSetters = new();

  public Dictionary<StatType, Func<float>> numericStatGetters = new();
  public Dictionary<StatType, Func<bool>> boolStatGetters = new();

  public override void Awake()
  {
    base.Awake();
    _OnDamage.AddListener(DamageHandler);
  }

  public void AutoRegisterStats()
  {
    var properties = GetType()
      .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

    foreach (var prop in properties)
    {
      var attr = prop.GetCustomAttribute<StatAttribute>();
      if (attr == null)
        continue;

      StatType type = attr.Type;

      if (prop.PropertyType == typeof(float))
      {
        numericStatSetters[type] = value => prop.SetValue(this, value);
        numericStatGetters[type] = () => (float)prop.GetValue(this);
      }
      else if (prop.PropertyType == typeof(bool))
      {
        boolStatSetters[type] = value => prop.SetValue(this, value);
        boolStatGetters[type] = () => (bool)prop.GetValue(this);
      }
      else
      {
        Debug.LogWarning(
          $"Property {prop.Name} marcada com [Stat] tem tipo não suportado: {prop.PropertyType.Name}"
        );
      }
    }
  }

  public virtual void InitializeStats()
  {
    // No longer relying on strings or reflection here, fetching directly from the stored Funcs
    foreach (var kvp in numericStatGetters)
    {
      float value = kvp.Value.Invoke();
      Stats.AddStat(kvp.Key, value);
    }

    foreach (var kvp in boolStatGetters)
    {
      bool value = kvp.Value.Invoke();
      Stats.AddStat(kvp.Key, value);
    }
  }

  public virtual void HandleBoolStatChange(StatType type, bool value)
  {
    if (boolStatSetters.TryGetValue(type, out var setter))
      setter(value);
  }

  public virtual void HandleNumericStatChange(StatType type, float value)
  {
    if (numericStatSetters.TryGetValue(type, out var setter))
      setter(value);
  }

  public virtual void DeathHandler() { }

  public virtual void DamageHandler() { }

  #endregion

  #region --- Inicialização ---

  public override void Start()
  {
    base.Start();
    MaxHealth = _maxHealth;
    Health = _maxHealth;

    // Auto registra os setters e inicializa stats
    AutoRegisterStats();
    InitializeStats();

    // Conecta eventos do stats
    Stats.OnNumModified.AddListener(HandleNumericStatChange);
    Stats.OnBoolModified.AddListener(HandleBoolStatChange);

    //Conecta a função de morte
    _OnDeath.AddListener(DeathHandler);
  }
  #endregion
}

/// <summary>
/// Atributo para marcar propriedades que representam stats.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class StatAttribute : Attribute
{
  public StatType Type { get; }

  public StatAttribute(StatType type)
  {
    Type = type;
  }
}
