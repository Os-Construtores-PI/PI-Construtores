using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.SceneManagement;

// Requer o BrainComponent no mesmo GameObject
[RequireComponent(typeof(BrainComponent))]
public class HealthComponent : ComponentBehaviour
{
    // Chaves para o sistema de atributos
    private const string HealthKey = "health";          // Vida atual
    private const string MaxHealthKey = "MAX_health";   // Vida máxima
    private const string DefenseKey = "defense";        // Defesa atual
    private const string MaxDefenseKey = "MAX_defense"; // Defesa máxima

    // Constantes de defesa
    private const float max_Defense = 100f; // Valor máximo de defesa
    private readonly float CombatCD = 15;   // Tempo de cooldown de combate em segundos
    private bool InCombat;                 // Flag que indica se está em combate

    [Header("Parâmetros de Vida")]
    [SerializeField] private float health;      // Vida atual
    [SerializeField] private float max_Health;  // Vida máxima
    [SerializeField] private float defense = 10; // Defesa atual
    [SerializeField] private bool enableRegen = true; // Ativa/desativa regeneração

    // Referências
    private BrainComponent brain;            // Componente cerebral da entidade
    private HealthHUDComponent healthHUD;     // HUD de vida (barra de vida)
    private Coroutine exitcombatcoro;       // Referência para a corrotina de combate

    private void Start()
    {
        // Inicializa os atributos com valores iniciais
        SetAttribute(HealthKey, max_Health);
        SetAttribute(MaxHealthKey, max_Health);
        SetAttribute(DefenseKey, defense);
        SetAttribute(MaxDefenseKey, max_Defense);


        // Obtém o BrainComponent
        if (!TryGetComponent(out brain))
        {
            return; // Sai se não encontrar
        }

        // Configura o HUD baseado no tipo de entidade
        switch (brain.identity.TipoEntidade)
        {
            case EntityType.PLAYER:
                // Encontra e atribui o HUD do jogador
                GameObject[] healthHUDs = GameObject.FindGameObjectsWithTag("HealthHUD");
                foreach (GameObject hud in healthHUDs)
                {
                    if (hud.TryGetComponent(out HealthHUDComponent hudhealth) && hudhealth.id_health == brain.identity.ID
                    && hudhealth.HUDType == HealthHUDType.PLAYER)
                    {
                        healthHUD = hudhealth;
                    }
                }
                break;
        }

        // Assina as mudanças de atributos
        SubscribeToAttribute(MaxHealthKey, (newMaxHealth) =>
        {
            max_Health = (float)newMaxHealth; // Atualiza vida máxima quando mudar
        });

        SubscribeToAttribute(HealthKey, (newHealth) =>
        {
            health = (float)newHealth;
            // Atualiza HUD se for a entidade correspondente
            if (healthHUD != null)
            {
                float maxHealth = GetAttribute<float>(MaxHealthKey);
                healthHUD.UpdateSlider(health / maxHealth); // Atualiza a barra de vida
            }
        });

        SubscribeToAttribute(DefenseKey, (newDefense) =>
        {
            // Logs para debug de mudanças de defesa
            print("AtualizarUI");
            print("newdefense: " + newDefense);
        });




        // Inicia a regeneração periódica de vida
        InvokeRepeating(nameof(Regeneration), 0, 2f);
    }

    /// <summary>
    /// Adiciona vida à entidade
    /// </summary>
    /// <param name="amount">Quantidade de vida a adicionar</param>
    public void AddHealth(float amount)
    {
        float currentHealth = GetAttribute<float>(HealthKey);
        currentHealth += amount;
        // Garante que não ultrapasse o máximo
        SetAttribute(HealthKey, Mathf.Min(currentHealth, max_Health));
    }

    /// <summary>
    /// Remove vida da entidade, considerando a defesa
    /// </summary>
    /// <param name="amount">Dano bruto</param>
    public void SubtractHealth(float amount)
    {
        float currentHealth = GetAttribute<float>(HealthKey);
        // Calcula redução de dano (limitada a 80%)
        float defenseFactor = Mathf.Min(GetAttribute<float>(DefenseKey) / max_Defense, .80f);
        currentHealth -= amount * (1 - defenseFactor);

        // Verifica morte
        if (currentHealth <= 0)
        {
            print($"{gameObject.name} MORREU!");
            brain.MorteCerebral(); // Ativa a morte
        }

        SetAttribute(HealthKey, currentHealth);
        EnterCombat(); // Coloca em estado de combate
    }

    /// <summary>
    /// Coloca a entidade em estado de combate e inicia o timer
    /// </summary>
    private void EnterCombat()
    {
        InCombat = true;
        // Para o cooldown existente se estiver ativo
        if (exitcombatcoro != null)
        {
            StopCoroutine(exitcombatcoro);
        }
        // Inicia novo timer de cooldown
        exitcombatcoro = StartCoroutine(ExitCombat(CombatCD));
    }

    /// <summary>
    /// Corrotina que sai do estado de combate após o tempo
    /// </summary>
    /// <param name="combatcooldown">Tempo em segundos</param>
    IEnumerator ExitCombat(float combatcooldown)
    {
        yield return new WaitForSeconds(combatcooldown);
        InCombat = false;
    }

    /// <summary>
    /// Regenera vida periodicamente quando as condições são atendidas
    /// </summary>
    private void Regeneration()
    {
        // Só regenera se:
        // - Regeneração ativada
        // - Não estiver em combate
        // - For uma entidade do tipo jogador
        if (!enableRegen || InCombat || brain.identity.TipoEntidade != EntityType.PLAYER) return;

        float percentage = .06f; // Taxa de regeneração (6%)
        float formula = max_Health * percentage;
        AddHealth(formula);
    }
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