using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BrainComponent))]
public class HealthComponent : ComponentBehaviour
{

    [Header("Parâmetros de Vida")]
    [SerializeField] private float health;
    [SerializeField] private float max_Health;
    [SerializeField] private float defense = 10;
    private float max_Defense = 100f;

    private EntityType entity_type;

    private void Start()
    {
        if (TryGetComponent(out BrainComponent cerebro))
        {
            entity_type = cerebro.identity.TipoEntidade;
        }

        SetAttribute(nameof(health), max_Health);
        SetAttribute(nameof(max_Health), max_Health);
        SetAttribute(nameof(defense), defense);
        SetAttribute(nameof(max_Defense), max_Defense);

        SubscribeToAttribute(nameof(health), (newValue) =>
        {
            print("AtualizarUI");
            print("health:" + newValue);
        });
        SubscribeToAttribute(nameof(defense), (newValue) =>
        {
            print("AtualizarUI");
            print("newdefense: "+newValue);
        });
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
        currentHealth -= amount * (1-Mathf.Min(GetAttribute<float>(nameof(defense)) / max_Defense, .80f));
        if (currentHealth <= 0)
        {
            DeathEvent(entity_type);
        }
        SetAttribute(nameof(health), currentHealth);
    }
    private void DeathEvent(EntityType type)
    {
        switch (type)
        {
            case EntityType.player:
                GameObject Director = GameObject.FindWithTag("GameController");
                if (Director && Director.TryGetComponent(out GameDirector directorscript))
                {
                    directorscript.ShutdownWorld();
                }
                else
                {
                    SceneManager.LoadScene("MenuGame");
                }
                break;
            case EntityType.enemy:
                // Animação de morte e desativamento...
                gameObject.SetActive(false);
                break;
            case EntityType.entity:
                // Flick branco e desativamento...
                gameObject.SetActive(false);
                break;
            default:
                Debug.Log("Você precisa colocar um tipo para este gameobj");
                break;
        }
    }
}
