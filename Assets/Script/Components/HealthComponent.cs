using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.SceneManagement;

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
    private const float max_Defense = 100f; // Defesa máxima padrão
    private readonly float CombatCD = 15;   // Cooldown para sair do estado de combate
    private bool InCombat;                 // Indica se a entidade está em combate

    // ==== Parâmetros configuráveis no Inspector ====
    [Header("Parâmetros de Vida")]
    [SerializeField] private float health;           // Vida atual
    [SerializeField] private float max_Health;       // Vida máxima
    [SerializeField] private float defense = 10;     // Defesa atual
    [SerializeField] private bool enableRegen = true; // Ativa a regeneração de vida fora de combate

    // ==== Referências ====
    private BrainComponent brain;              // Referência ao "cérebro" da entidade
    private HealthHUDComponent healthHUD;      // HUD de vida (barra de HP na tela)
    private Coroutine exitcombatcoro;          // Controle da corrotina de combate
    private UnityEvent eventodano;

    private void Start()
    {
        eventodano ??= new();

        // === Inicialização dos atributos ===
        SetAttribute(HealthKey, max_Health);
        SetAttribute(MaxHealthKey, max_Health);
        SetAttribute(DefenseKey, defense);
        SetAttribute(MaxDefenseKey, max_Defense);

        // === Pega o BrainComponent ===
        if (!TryGetComponent(out brain))
            return;

        // === Tenta localizar e associar o HUD de vida se for um jogador ===
        switch (brain.identity.TipoEntidade)
        {
            case EntityType.PLAYER:
                GameObject[] healthHUDs = GameObject.FindGameObjectsWithTag("HealthHUD");
                foreach (GameObject hud in healthHUDs)
                {
                    if (hud.TryGetComponent(out HealthHUDComponent hudhealth) &&
                        hudhealth.id_health == brain.identity.ID &&
                        hudhealth.HUDType == HealthHUDType.PLAYER)
                    {
                        healthHUD = hudhealth;
                    }
                }
                eventodano.AddListener(brain.EventoDano);
                break;
        }

        // === Escuta mudanças nos atributos ===

        // Atualiza o valor local da vida máxima sempre que ela mudar
        SubscribeToAttribute(MaxHealthKey, (newMaxHealth) =>
        {
            max_Health = (float)newMaxHealth;
        });

        // Atualiza a vida atual e atualiza a HUD
        SubscribeToAttribute(HealthKey, (newHealth) =>
        {
            if (health > (float) newHealth)
            {
                eventodano.Invoke();  
            }
            health = (float)newHealth;
            if (healthHUD != null)
            {
                float maxHealth = GetAttribute<float>(MaxHealthKey);
                healthHUD.UpdateSlider(health / maxHealth); // Atualiza barra de vida
            }
        });

        // Atualiza o valor de defesa local sempre que mudar
        SubscribeToAttribute(DefenseKey, (newDefense) =>
        {
            defense = (float)newDefense;
        });

        // Inicia a regeneração periódica
        InvokeRepeating(nameof(Regeneration), 0, 2f);
    }

    /// <summary>
    /// Aumenta a vida atual da entidade
    /// </summary>
    public void AddHealth(float amount)
    {
        float currentHealth = GetAttribute<float>(HealthKey);
        currentHealth += amount;
        SetAttribute(HealthKey, Mathf.Min(currentHealth, max_Health)); // Garante que não passe do máximo
    }

    /// <summary>
    /// Aplica dano, considerando a defesa
    /// </summary>
    public void SubtractHealth(float amount)
    {
        float currentHealth = GetAttribute<float>(HealthKey);

        // Calcula quanto do dano será mitigado pela defesa (máx 80%)
        float defenseFactor = Mathf.Min(GetAttribute<float>(DefenseKey) / max_Defense, 0.80f);
        currentHealth -= amount * (1 - defenseFactor);

        // Verifica se a entidade morreu
        if (currentHealth <= 0)
        {
            print($"{gameObject.name} MORREU!");
            brain.MorteCerebral(); // Ativa lógica de morte
        }

        // Atualiza a vida
        SetAttribute(HealthKey, currentHealth);

        // Entra em estado de combate
        EnterCombat();
    }

    /// <summary>
    /// Coloca a entidade em combate e inicia cooldown para sair
    /// </summary>
    private void EnterCombat()
    {
        InCombat = true;

        // Para o cooldown anterior, se existir
        if (exitcombatcoro != null)
        {
            StopCoroutine(exitcombatcoro);
        }

        // Inicia nova contagem para sair do estado de combate
        exitcombatcoro = StartCoroutine(ExitCombat(CombatCD));
    }

    /// <summary>
    /// Sai do combate após um tempo
    /// </summary>
    IEnumerator ExitCombat(float combatcooldown)
    {
        yield return new WaitForSeconds(combatcooldown);
        InCombat = false;
    }

    /// <summary>
    /// Regenera vida gradualmente fora de combate
    /// </summary>
    private void Regeneration()
    {
        // Só regenera se:
        // - Habilitado
        // - Fora de combate
        // - Entidade for jogador
        if (!enableRegen || InCombat || brain.identity.TipoEntidade != EntityType.PLAYER) return;

        float percentage = .06f;
        float formula = max_Health * percentage;
        AddHealth(formula);
    }

    /// <summary>
    /// Atribui o HUD manualmente (ex: via spawn dinâmico)
    /// </summary>
    public void SetHealthHUD(HealthHUDComponent hud)
    {
        if (hud == null) return;

        healthHUD = hud;

        if (TryGetAttribute(MaxHealthKey, out float maxHealth))
        {
            healthHUD.UpdateSlider(health / maxHealth);
        }
    }
}
