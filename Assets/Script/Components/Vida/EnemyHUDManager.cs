using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyHUDManager : MonoBehaviour
{
    [Header("Configurações do HUD")]
    [SerializeField] private GameObject hudPrefab;
    [SerializeField] private Transform hudParent;

    private const string HealthBarTargetName = "HealthBarTarget";

    private List<GameObject> enemyObjects = new();

    private void Start()
    {
        FindEnemies();
        SpawnEnemyHUDs();
    }

    /// <summary>
    /// Busca todos os GameObjects com tag "Creature" e filtra aqueles que são inimigos
    /// </summary>
    private void FindEnemies()
    {
        var creatures = GameObject.FindGameObjectsWithTag("Creature");

        enemyObjects = creatures
            .Where(creature => creature.TryGetComponent<Enemies>(out var enemy))
            .ToList();
    }

    /// <summary>
    /// Instancia o HUD para cada inimigo encontrado e configura os componentes relacionados
    /// </summary>
    private void SpawnEnemyHUDs()
    {
        if (hudPrefab == null)
        {
            Debug.LogWarning("HUD Prefab não atribuído no EnemyHUDManager.");
            return;
        }

        foreach (var enemy in enemyObjects)
        {
            // Busca o transform alvo para posicionar o HUD
            Transform healthBarTarget = enemy.transform.Find(HealthBarTargetName);
            if (healthBarTarget == null)
            {
                Debug.LogWarning($"Objeto '{enemy.name}' não possui filho '{HealthBarTargetName}' para posicionar HUD.");
                continue;
            }

            // Instancia o HUD como filho do hudParent, mantendo a posição do alvo
            GameObject hudInstance = Instantiate(hudPrefab, healthBarTarget.position, Quaternion.identity, hudParent);

            if (!hudInstance.TryGetComponent<HealthHUDComponent>(out var healthHUD))
            {
                Debug.LogWarning($"O prefab HUD não possui o componente HealthHUDComponent.");
                Destroy(hudInstance);
                continue;
            }

            if (!enemy.TryGetComponent(out Enemies enemyclass))
            {
                Destroy(hudInstance);
                continue;
            }

            // Configura o HUD para seguir o inimigo
            healthHUD.EnemyTarget = healthBarTarget;
            healthHUD.IdHealth = enemyclass.ID;

            // Associa o HUD ao componente de vida para atualizações
            enemyclass.SetHealthHUD(healthHUD);
            healthHUD.UpdateSlider(enemyclass.Health / enemyclass.MaxHealth);
        }
    }
}
