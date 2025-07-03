using UnityEngine;
using UnityEngine.Events;


public class LiveEntity : Entity
{
    protected float _health;
    [HideInInspector] public float Health
    {
        get
        {
            return _health;
        }
        set
        {
            float factor = Mathf.Clamp(Defense / _maxDefense, 0f, .80f);
            if (value < _health)
            {
                _OnDamage.Invoke();
            }
            _health = value * (1 - factor);
            _OnHealthChanged.Invoke();
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
            _defense = value;
        }
    }
    
    [SerializeField] protected float _maxHealth;
    [SerializeField] protected float _maxDefense;
    protected readonly UnityEvent _OnHealthChanged = new();
    protected readonly UnityEvent _OnDamage = new();
    protected readonly UnityEvent _OnDeath = new();
    




}
