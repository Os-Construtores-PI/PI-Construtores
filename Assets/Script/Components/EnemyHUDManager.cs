using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyHUDManager : MonoBehaviour
{
    private GameObject[] creatures;
    private List<GameObject> enemies = new();
    [SerializeField] private GameObject HUD;


    private void Start()
    {
        GetEnemies();
        InstantiateHealthHUD();
    }

    private void GetEnemies()
    {
        creatures = GameObject.FindGameObjectsWithTag("Creature");
        foreach (GameObject creature in creatures)
        {
            if (creature.TryGetComponent(out BrainComponent brain) && brain.identity.TipoEntidade == EntityType.ENEMY)
            {
                enemies.Add(creature);
            }
        }
    }
    private void InstantiateHealthHUD()
    {
        if (HUD)
        {
            foreach (GameObject enemy in enemies)
            {
                Canvas enemy_canvas = enemy.GetComponentInChildren<Canvas>(); // pega o canvas
                if (enemy_canvas)
                {
                    GameObject hud = Instantiate(HUD, enemy.transform.position + new Vector3(0, 2.5f, 0), Quaternion.identity, enemy_canvas.transform);
                    if (hud.TryGetComponent(out HealthHUDComponent healthHUD) && enemy.TryGetComponent(out HealthComponent health))
                    {
                        healthHUD.enemy_target = enemy.transform;
                        health.healthHUD = healthHUD;
                    }   
                    
                }
            }
        }
    }
}
