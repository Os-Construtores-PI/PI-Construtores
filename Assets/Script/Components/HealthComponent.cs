using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(BrainComponent))]
public class HealthComponent : ComponentBehaviour
{
    private const string HealthKey = "health";
    private const string MaxHealthKey = "MAX_health";
    private const string DefenseKey = "defense";
    private const string MaxDefenseKey = "MAX_defense";
    private const float max_Defense = 100f;
    private readonly float CombatCD = 15;
    private bool InCombat;



    [Header("Parâmetros de Vida")]
    [SerializeField] private float health;
    [SerializeField] private float max_Health;
    [SerializeField] private float defense = 10;
    [SerializeField] private bool enableRegen = true;


    private BrainComponent brain;
    public HealthHUDComponent healthHUD;
    private Coroutine exitcombatcoro;


    private void Start()
    {
        if (TryGetComponent(out BrainComponent cerebro))
        {
            brain = cerebro;
        }
        else
        {
            return;
        }


        switch (brain.identity.TipoEntidade)
        {
            case EntityType.PLAYER:
                healthHUD = GameObject.FindWithTag("HealthHUD").GetComponent<HealthHUDComponent>();
                break;
        }
        SetAttribute(HealthKey, max_Health);
        SetAttribute(MaxHealthKey, max_Health);
        SetAttribute(DefenseKey, defense);
        SetAttribute(MaxDefenseKey, max_Defense);

        SubscribeToAttribute(MaxHealthKey, (newMaxHealth) =>
        {
            max_Health = (float)newMaxHealth;
        });

        SubscribeToAttribute(HealthKey, (newHealth) =>
        {
            health = (float)newHealth;
            if (healthHUD != null && brain.identity.ID == healthHUD.id_health)
            {
                float maxHealth = GetAttribute<float>(MaxHealthKey);
                switch (brain.identity.TipoEntidade)
                {
                    case EntityType.PLAYER:
                            healthHUD.UpdateSlider(health / maxHealth);
                        break;
                    case EntityType.ENEMY:
                            healthHUD.UpdateSlider(health / maxHealth);
                        break;
                }
            }
        });
        SubscribeToAttribute(DefenseKey, (newDefense) =>
        {
            print("AtualizarUI");
            print("newdefense: " + newDefense);
        });
        InvokeRepeating(nameof(Regeneration), 0, 2f);
    }

    public void AddHealth(float amount)
    {
        float currentHealth = GetAttribute<float>(HealthKey);
        currentHealth += amount;
        SetAttribute(HealthKey, Mathf.Min(currentHealth, max_Health));
    }
    public void SubtractHealth(float amount)
    {
        float currentHealth = GetAttribute<float>(HealthKey);
        currentHealth -= amount * (1 - Mathf.Min(GetAttribute<float>(DefenseKey) / max_Defense, .80f));
        if (currentHealth <= 0)
        {
            print($"{gameObject.name} MORREU!");
            brain.MorteCerebral();
        }
        SetAttribute(HealthKey, currentHealth);
        if (brain.identity.TipoEntidade == EntityType.PLAYER)
        {
             EnterCombat();
        }
    }
    private void EnterCombat()
    {
        InCombat = true;
        if (exitcombatcoro != null)
        {
            StopCoroutine(exitcombatcoro);
        }
        exitcombatcoro = StartCoroutine(ExitCombat(CombatCD));
    }
    IEnumerator ExitCombat(float combatcooldown)
    {
        yield return new WaitForSeconds(combatcooldown);
        InCombat = false;
    }
    private void Regeneration()
    {
        if (!enableRegen || InCombat) return;
        float percentage = .06f; // 6%
        float formula = max_Health * percentage;
        AddHealth(formula);                    
    }
}
