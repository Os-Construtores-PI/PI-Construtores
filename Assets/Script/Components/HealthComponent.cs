using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BrainComponent))]
public class HealthComponent : ComponentBehaviour
{

    [Header("Parâmetros de Vida")]
    [SerializeField] private float Health;
    [SerializeField] private float Max_Health;

    private EntityType entity_type;

    private void Start()
    {
        if (TryGetComponent(out BrainComponent cerebro))
        {
            entity_type = cerebro.entity;
        }
        SetAttribute(nameof(Health), Max_Health);
        SetAttribute(nameof(Max_Health), Max_Health);

        SubscribeToAttribute(nameof(Health), (newValue) =>
        {
            print("AtualizarUI");
        });
    }
    public void AddHealth(float amount)
    {
        float currentHealth = GetAttribute<float>("Health");
        currentHealth += amount;
        SetAttribute(nameof(Health), Mathf.Min(currentHealth, Max_Health));
    }
    public void SubtractHealth(float amount)
    {
        float currentHealth = GetAttribute<float>("Health");
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            DeathEvent(entity_type);
        }
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
