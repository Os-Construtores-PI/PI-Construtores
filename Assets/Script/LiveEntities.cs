using UnityEngine;
using UnityEngine.Events;


public abstract class LiveEntities : Entities
{
    public virtual void Start()
    {
        MaxHealth = _maxHealth;
        Health = _maxHealth;
    }

    protected float _health;
    [HideInInspector] public float Health
    {
        get
        {
            return _health;
        }
        set
        {
            float _inithealth = _health;
            _health = Mathf.Clamp(value, 0f, MaxHealth);
            if (_health < _inithealth)
            {
                _OnDamage.Invoke();
            }
            _OnHealthChanged.Invoke(_health/MaxHealth);
        }
    }
    protected float _defense = 10f;
    [HideInInspector] public float Defense
    {
        get
        {
            return _defense;
        }
        set
        {
            print("old: " + _defense + " // new: " + value);
            _defense = value;
        }
    }
    
    [Header("Atributos de Vida")]
    [SerializeField] protected float _maxHealth;
    [HideInInspector] public float MaxHealth
    {
        get
        {
            return _maxHealth;
        }
        set
        {
            _maxHealth = value;
        }
    } 
    [HideInInspector] public readonly float MAXDEFENSE = 100;
    protected readonly UnityEvent<float> _OnHealthChanged = new();
    protected readonly UnityEvent _OnDamage = new();
    protected readonly UnityEvent _OnDeath = new();
    




}
