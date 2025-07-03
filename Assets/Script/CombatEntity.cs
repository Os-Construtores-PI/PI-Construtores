using UnityEngine;

public class CombatEntity : LiveEntity
{
    [SerializeField] private bool enableRegen = true;
    [SerializeField, Min(5)] private float CombatCD;
    private bool _inCombat;
    protected HealthHUDComponent _healthHUD;
    private float CombatWalker;
    [SerializeField, Min(3)] private float RegerationInterval;
    [SerializeField, Min(10)] private float damagedCD;
    private float damagedWalker = 0.0f;
    [HideInInspector] public bool Damaged;

    public virtual void Awake()
    {
        _OnDamage.AddListener(EnterCombat);
        if (enableRegen)
        {
            InvokeRepeating(nameof(RegenerateHealth), 0, 3f);
        }
    }
    private void RegenerateHealth()
    {
        if (!_inCombat)
        {
            // Regenera 6% da vida máxima
            float regenAmount = _maxHealth * 0.06f;
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
}
