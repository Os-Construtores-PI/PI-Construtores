using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthComponent : EntityBehavior
{
    [SerializeField] public EntityType entity;
    [SerializeField] private float Health;
    [SerializeField] private float Max_Health;


    private void Start()
    {
        Health = Max_Health;
    }
    public void AddHealth(int amount)
    {
        Health += amount;
        if (Health > Max_Health)
        {
            Health = Max_Health;
        }
    }
    public void SubtractHealth(int amount)
    {
        Health -= amount;
        if (Health <= 0)
        {
            DeathEvent(entity);
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
