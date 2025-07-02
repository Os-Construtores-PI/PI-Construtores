using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Requer que o GameObject tenha um BrainComponent
[RequireComponent(typeof(BrainComponent))]
public class HealthComponent : ComponentBehaviour
{
    // ==== Chaves para atributos no sistema ====
    private const string HealthKey = "health";          // Vida atual
    private const string MaxHealthKey = "MAX_health";   // Vida máxima
    private const string DefenseKey = "defense";        // Defesa atual
    private const string MaxDefenseKey = "MAX_defense"; // Defesa máxima

    // ==== Constantes ====
    private const float MaxDefenseDefault = 100f; // Defesa máxima padrão
    private const float CombatCooldown = 15f;     // Cooldown para sair do combate

    // ==== Estados internos ====
    private bool _inCombat;
    private Coroutine _exitCombatCoroutine;

    // ==== Parâmetros configuráveis no Inspector ====
    [Header("Parâmetros de Vida")]
    [SerializeField, Min(0)] private float health;              // Vida atual (inicial)
    [SerializeField, Min(0)] private float maxHealth = 100f;    // Vida máxima (inicial)
    [SerializeField, Min(0)] private float defense = 10f;       // Defesa atual (inicial)
    [SerializeField] private bool enableRegen = true;           // Regeneração habilitada fora de combate

    // ==== Referências ====
    private BrainComponent _brain;
    private HealthHUDComponent _healthHUD;

    // Eventos customizáveis para dano e morte
    private readonly UnityEvent _onDamage = new();
    private readonly UnityEvent _onDeath = new();

    private void Start()
    {
        // Inicializa atributos no sistema
        SetAttribute(HealthKey, maxHealth);
        SetAttribute(MaxHealthKey, maxHealth);
        print(name+" : "+GetAttribute<float>(MaxHealthKey));
        SetAttribute(DefenseKey, defense);
        SetAttribute(MaxDefenseKey, MaxDefenseDefault);

        // Tenta obter BrainComponent
        if (!TryGetComponent(out _brain)) return;

        // Se for jogador, busca HUD e adiciona eventos de dano e morte
        if (_brain.identity.TipoEntidade == EntityType.PLAYER)
        {
            var huds = GameObject.FindGameObjectsWithTag("HealthHUD");
            foreach (var hudObj in huds)
            {
                if (hudObj.TryGetComponent<HealthHUDComponent>(out var hud) &&
                    hud.IdHealth == _brain.identity.ID &&
                    hud.HUDType == HealthHUDType.PLAYER)
                {
                    _healthHUD = hud;
                    break;
                }
            }

            _onDamage.AddListener(_brain.EventoDano);
            _onDeath.AddListener(_brain.MorteCerebral);
        }

        // Assina mudanças nos atributos para manter valores locais sincronizados
        SubscribeToAttribute(MaxHealthKey, value => maxHealth = (float)value);
        SubscribeToAttribute(HealthKey, OnHealthChanged);
        SubscribeToAttribute(DefenseKey, value => defense = (float)value);

        // Inicia regeneração periódica para NPCs, se habilitado
        if (_brain.identity.TipoEntidade == EntityType.PLAYER || enableRegen)
        {
            InvokeRepeating(nameof(RegenerateHealth), 0f, 2f);
        }
    }

    private void OnHealthChanged(object newHealthObj)
    {
        float newHealth = (float)newHealthObj;

        // Se vida diminuiu, disparar evento de dano
        if (newHealth < health)
        {
            _onDamage.Invoke();
        }

        health = newHealth;

        // Atualiza HUD, se existir
        if (_healthHUD != null)
        {
            float currentMaxHealth = GetAttribute<float>(MaxHealthKey);
            _healthHUD.UpdateSlider(health / currentMaxHealth);
        }

        // Verifica se morreu
        if (health <= 0)
        {
            Debug.Log($"{gameObject.name} MORREU!");
            _onDeath.Invoke();
        }
    }

    /// <summary>
    /// Aumenta a vida atual da entidade, sem ultrapassar o máximo
    /// </summary>
    public void AddHealth(float amount)
    {
        if (amount <= 0) return;

        float currentHealth = GetAttribute<float>(HealthKey);
        float newHealth = Mathf.Min(currentHealth + amount, maxHealth);
        SetAttribute(HealthKey, newHealth);
    }

    /// <summary>
    /// Aplica dano, levando em conta a defesa (mitigação até 80%)
    /// </summary>
    public void SubtractHealth(float amount)
    {
        if (amount <= 0) return;

        float currentHealth = GetAttribute<float>(HealthKey);
        float defenseValue = GetAttribute<float>(DefenseKey);
        float defenseFactor = Mathf.Clamp(defenseValue / MaxDefenseDefault, 0f, 0.80f);

        float effectiveDamage = amount * (1f - defenseFactor);
        float newHealth = currentHealth - effectiveDamage;

        SetAttribute(HealthKey, newHealth);

        EnterCombat();
    }

    /// <summary>
    /// Entra no estado de combate e reseta o cooldown para sair dele
    /// </summary>
    private void EnterCombat()
    {
        _inCombat = true;

        if (_exitCombatCoroutine != null)
        {
            StopCoroutine(_exitCombatCoroutine);
        }

        _exitCombatCoroutine = StartCoroutine(ExitCombatAfterCooldown());
    }

    private IEnumerator ExitCombatAfterCooldown()
    {
        yield return new WaitForSeconds(CombatCooldown);
        _inCombat = false;
        _exitCombatCoroutine = null;
    }

    /// <summary>
    /// Regenera vida periodicamente quando fora de combate
    /// </summary>
    private void RegenerateHealth()
    {
        if (!_inCombat && enableRegen)
        {
            // Regenera 6% da vida máxima
            float regenAmount = maxHealth * 0.06f;
            AddHealth(regenAmount);
        }
    }

    /// <summary>
    /// Atribui manualmente o HUD de vida
    /// </summary>
    public void SetHealthHUD(HealthHUDComponent hud)
    {
        if (hud == null) return;

        _healthHUD = hud;
        _healthHUD.UpdateSlider(health / maxHealth);
    }
}
