using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BrainComponent))]
public class HealthComponent : ComponentBehaviour
{

    [Header("Parâmetros de Vida")]
    [SerializeField] private float health;
    [SerializeField] private float max_Health;
    [SerializeField] private float defense = 10;
    private const float max_Defense = 100f;

    private BrainComponent brain;
    private EntityType entity_type;
    private float CombatCD = 15;
    private bool InCombat;

    private void Start()
    {
        if (TryGetComponent(out BrainComponent cerebro))
        {
            entity_type = cerebro.identity.TipoEntidade;
            brain = cerebro;
        }

        SetAttribute(nameof(health), max_Health);
        SetAttribute("MAX_" + nameof(health), max_Health);
        SetAttribute(nameof(defense), defense);
        SetAttribute("MAX_" + nameof(defense), max_Defense);

        SubscribeToAttribute(nameof(health), (newHealth) =>
        {
            health = (float) newHealth;
            if (entity_type == EntityType.PLAYER)
            {
                HealthHUDComponent healthHUD = GameObject.FindWithTag("HealthHUD").GetComponent<HealthHUDComponent>();
                float div = health / max_Health;
                if (div >= 1)
                {
                    healthHUD.ChangeIcon(3);
                }
                else if (div >= .75f)
                {
                    healthHUD.ChangeIcon(2);
                }
                else if (div >= .50f)
                {
                    healthHUD.ChangeIcon(1);
                }
                else
                {
                    healthHUD.ChangeIcon(0);
                }
            }
        });
        SubscribeToAttribute(nameof(defense), (newDefense) =>
        {
            print("AtualizarUI");
            print("newdefense: " + newDefense);
        });
        InvokeRepeating(nameof(Regeneration), 0, 3f);
    }
    public void AddHealth(float amount)
    {
        float currentHealth = GetAttribute<float>(nameof(health));
        currentHealth += amount;
        SetAttribute(nameof(health), Mathf.Min(currentHealth, max_Health));
    }
    public void SubtractHealth(float amount)
    {
        float currentHealth = GetAttribute<float>(nameof(health));
        currentHealth -= amount * (1 - Mathf.Min(GetAttribute<float>(nameof(defense)) / max_Defense, .80f));
        if (currentHealth <= 0)
        {
            brain.MorteCerebral(entity_type);
        }
        SetAttribute(nameof(health), currentHealth);
        if (entity_type == EntityType.PLAYER)
        {
            InCombat = true;
            StartCoroutine(ExitCombat(CombatCD));
        }
    }
    IEnumerator ExitCombat(float combatcooldown)
    {
        yield return new WaitForSeconds(combatcooldown);
        InCombat = false;
    }
    private void Regeneration()
    {
        if (!InCombat && TryGetAttribute(nameof(health), out float currentHealth))
        {
            float percentage = .03f; // 3%
            float formula = currentHealth * percentage;
            AddHealth(formula);                    
        }
    }
}
