using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Base para entidades vivas que possuem saúde, defesa e sistema de stats com reflexão.
/// </summary>
public abstract class LiveEntities : Entities
{
    [Header("Debug - Vida Atual")]
    [SerializeField] private float _currentHealthDebug;
    #region --- Atributos de Vida ---

    [Header("Atributos de Vida")]
    [SerializeField, Min(1f)] protected float _maxHealth = 100f;
    [SerializeField, Min(0f)] protected float _defense = 10f;

    private float _health;

    [HideInInspector]
    [Stat(nameof(Health))]
    public float Health
    {
        get => _health;
        set
        {
            float oldHealth = _health;
            _health = Mathf.Clamp(value, 0f, MaxHealth);

            //Debug.Log($"{name} // Health changed: {oldHealth} -> {_health}");

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
    [Stat(nameof(Defense))]
    public float Defense
    {
        get => _defense;
        set
        {
            Debug.Log($"{name} // Defense changed: {_defense} -> {value}");
            _defense = Mathf.Clamp(value, 0f, MAX_DEFENSE);
        }
    }

    [HideInInspector]
    [Stat(nameof(MaxHealth))]
    public float MaxHealth
    {
        get => _maxHealth;
        set => _maxHealth = Mathf.Max(1f, value);
    }

    [HideInInspector] public readonly float MAX_DEFENSE = 100f;

    #endregion

    #region --- Eventos ---

    public readonly UnityEvent<float> _OnHealthChanged = new();
    public readonly UnityEvent _OnDamage = new();
    public readonly UnityEvent _OnDeath = new();

    #endregion

    #region --- Sistema de Stats via reflexão ---

    public Stats stats = new();

    public Dictionary<string, Action<float>> numericStatSetters = new();
    public Dictionary<string, Action<bool>> boolStatSetters = new();


    public override void Awake()
    {
        base.Awake();
        _OnDamage.AddListener(DamageHandler);
    }
    public void AutoRegisterStats()
    {
        var properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var prop in properties)
        {
            var attr = prop.GetCustomAttribute<StatAttribute>();
            if (attr == null)
                continue;

            string name = attr.Name;

            if (prop.PropertyType == typeof(float))
            {
                numericStatSetters[name] = value => prop.SetValue(this, value);
            }
            else if (prop.PropertyType == typeof(bool))
            {
                boolStatSetters[name] = value => prop.SetValue(this, value);
            }
            else
            {
                Debug.LogWarning($"Property {prop.Name} marcada com [Stat] tem tipo não suportado: {prop.PropertyType.Name}");
            }
        }
    }

    /// <summary>
    /// Inicializa stats no sistema Stats — pode ser sobrescrito para incluir mais.
    /// </summary>
    public virtual void InitializeStats()
    {
        foreach (var kvp in numericStatSetters)
        {
            var prop = GetType().GetPropertyByStatName(kvp.Key);
            if (prop != null)
            {
                var value = (float)prop.GetValue(this);
                stats.AddStat(kvp.Key, value);
            }
        }

        foreach (var kvp in boolStatSetters)
        {
            var prop = GetType().GetPropertyByStatName(kvp.Key);
            if (prop != null)
            {
                var value = (bool)prop.GetValue(this);
                stats.AddStat(kvp.Key, value);
            }
        }
    }

    /// <summary>
    /// Handle updates from stats system.
    /// </summary>
    public virtual void HandleBoolStatChange(string name, bool value)
    {
        if (boolStatSetters.TryGetValue(name, out var setter))
            setter(value);
    }

    public virtual void HandleNumericStatChange(string name, float value)
    {
        if (numericStatSetters.TryGetValue(name, out var setter))
            setter(value);
    }
    public virtual void DeathHandler()
    {

    }
    public virtual void DamageHandler()
    {

    }


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
        stats.OnNumModified.AddListener(HandleNumericStatChange);
        stats.OnBoolModified.AddListener(HandleBoolStatChange);

        //Conecta a função de morte
        _OnDeath.AddListener(DeathHandler);
    }
    #endregion


}

public class LiveEntityContext : EntityContext
{
    private readonly LiveEntities liveEntity;

    public LiveEntityContext(LiveEntities entity) : base(entity)
    {
        liveEntity = entity;
    }

    public Stats LiveEntityStats { get => liveEntity.stats; }
    public float LiveEntityHealth { get => liveEntity.Health; set => liveEntity.Health = value; }
    public float LiveEntityMaxHealth { get => liveEntity.MaxHealth; set => liveEntity.MaxHealth = value; }
    public float LiveEntityDefense { get => liveEntity.Defense; set => liveEntity.Defense = value; } 
}








/// <summary>
/// Atributo para marcar propriedades que representam stats.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class StatAttribute : Attribute
{
    public string Name { get; }

    public StatAttribute(string name)
    {
        Name = name;
    }
}





