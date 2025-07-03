using System;
using System.Collections.Generic;

using UnityEngine;

public abstract class CombatEntities : LiveEntities
{
    [Header("Atributos de intervalo do estado de combate")]
    [SerializeField, Min(5f)] private float CombatCD;
    [Header("Atributos de intervalo de dano tomado")]
    [SerializeField, Min(2f)] private float damagedCD;
    [Header("Atributos de Regeneração")]
    [SerializeField] private bool Enableregen = true;
    [SerializeField, Min(3)] private float RegerationInterval;
    private bool _inCombat;
    protected HealthHUDComponent _healthHUD;
    private float CombatWalker;
    private float damagedWalker = 0.0f;
    public Stats stats = new();
    public Dictionary<string, Action<float>> numvariablesdictionary = new();
    public Dictionary<string, Action<bool>> boolvariablesdictionary = new();

    [HideInInspector] public bool Damaged;

    public virtual void Awake()
    {
        _OnDamage.AddListener(EnterCombat);
        stats._boolModified.AddListener(BoolListener);
        stats._numModified.AddListener(NumListener);
        if (Enableregen)
        {
            InvokeRepeating(nameof(RegenerateHealth), 0, RegerationInterval);
        }
        AddtoDictionaryStat();
        AddtoStat();
    }
    private void RegenerateHealth()
    {
        if (!_inCombat)
        {
            // Regenera 6% da vida máxima
            float regenAmount = MaxHealth * 0.06f;
            Health += regenAmount;
        }
    }
    public virtual void Update()
    {
        if (_inCombat)
        {
            CombatWalker += Time.deltaTime;
            if (CombatWalker >= CombatCD)
            {
                CombatWalker = 0f;
                _inCombat = false;
            }
        }
        if (Damaged)
        {
            damagedWalker += Time.deltaTime;
            if (damagedWalker >= damagedCD)
            {
                Damaged = false;
            }
        }
    }
    private void EnterCombat()
    {
        _inCombat = true;
    }
    public void SetHealthHUD(HealthHUDComponent hud)
    {
        if (hud == null) return;

        _healthHUD = hud;
        _healthHUD.UpdateSlider(Health / _maxHealth);
    }
    public virtual void AddtoStat()
    {
        stats.AddStat(nameof(Health), Health);
        stats.AddStat(nameof(Defense), Defense);
        stats.AddStat(nameof(Enableregen), Enableregen);
    }
    public virtual void AddtoDictionaryStat()
    {
        numvariablesdictionary.Add(nameof(Health), (value) => Health = value);
        numvariablesdictionary.Add(nameof(Defense), (value) => Defense = value);
        boolvariablesdictionary.Add(nameof(Enableregen), (value) => Enableregen = value);
    }
    public virtual void BoolListener(string name, bool value)
    {
        if (!boolvariablesdictionary.ContainsKey(name)) return;
        boolvariablesdictionary[name](value);
    }
    public virtual void NumListener(string name, float value)
    {
        if (!numvariablesdictionary.ContainsKey(name)) return;
        numvariablesdictionary[name](value);
    }
}
