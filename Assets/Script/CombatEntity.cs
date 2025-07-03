using System.Collections.Generic;
using UnityEngine;

public class CombatEntity : LiveEntity
{
    [Header("Atributos de intervalo do estado de combate")]
    [SerializeField, Min(5f)] private float CombatCD;
    [Header("Atributos de intervalo de dano tomado")]
    [SerializeField, Min(2f)] private float damagedCD;
    [Header("Atributos de Regeneração")]
    [SerializeField] private bool enableRegen = true;
    [SerializeField, Min(3)] private float RegerationInterval;
    private bool _inCombat;
    protected HealthHUDComponent _healthHUD;
    private float CombatWalker;
    private float damagedWalker = 0.0f;
    [HideInInspector] public bool Damaged;

    public virtual void Awake()
    {
        _OnDamage.AddListener(EnterCombat);
        if (enableRegen)
        {
            InvokeRepeating(nameof(RegenerateHealth), 0, RegerationInterval);
        }
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
    public class Inventory
    {
        // Lista de itens no inventário
        [SerializeField]
        private List<InventoryItem> items = new();
        public List<InventoryItem> GetItems() => items;
        public void ClearItems() => items.Clear();
        public void AddItem(ItemDataBase data, int quantity = 1)
        {
            // Se o item não é único, tenta acumular com outro igual
            if (!data.Isunique)
            {
                var existing = items.Find(i => i.data == data);
                if (existing != null)
                {
                    existing.quantity += quantity;
                    return;
                }
            }

            // Caso contrário, adiciona um novo item à lista
            items.Add(new InventoryItem(data, quantity));
            Debug.Log($"Adicionado: {data.itemName} x{quantity}");
        }
    }
}
