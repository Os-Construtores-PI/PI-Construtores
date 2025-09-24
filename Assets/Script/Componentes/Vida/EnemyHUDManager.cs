using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyHUDManager : MonoBehaviour
{
    [Header("Configurações do HUD")]
    [SerializeField] private GameObject hudPrefab;
    [SerializeField] private Transform hudParent;

    private const string HealthBarTargetName = "HealthBarTarget";

    // Lista de inimigos ativos ou inativos na cena
    private List<Enemies> enemyObjects = new();

    private void Start()
    {
        FindEnemies();
        SpawnEnemyHUDs();
    }

    /// <summary>
    /// Encontra todos os componentes Enemies na cena, mesmo que estejam inativos
    /// </summary>
    private void FindEnemies()
    {
        enemyObjects = GameObject.FindObjectsByType<Enemies>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        ).ToList();
    }

    /// <summary>
    /// Cria e configura HUDs para cada inimigo encontrado
    /// </summary>
    private void SpawnEnemyHUDs()
    {
        if (hudPrefab == null)
        {
            Debug.LogWarning("HUD Prefab não atribuído no EnemyHUDManager.");
            return;
        }

        foreach (Enemies enemy in enemyObjects)
        {
            // Encontra o transform filho usado como âncora do HUD
            Transform healthBarTarget = enemy.transform.Find(HealthBarTargetName);
            if (healthBarTarget == null)
            {
                Debug.LogWarning($"Objeto '{enemy.name}' não possui filho '{HealthBarTargetName}' para posicionar HUD.");
                continue;
            }

            // Instancia o HUD na posição do alvo e como filho do hudParent
            GameObject hudInstance = Instantiate(hudPrefab, healthBarTarget.position, Quaternion.identity, hudParent);

            if (!hudInstance.TryGetComponent(out HealthHUDComponent healthHUD))
            {
                Debug.LogWarning("O prefab HUD não possui o componente HealthHUDComponent.");
                Destroy(hudInstance);
                continue;
            }

            // Configura o HUD
            healthHUD.EnemyTarget = healthBarTarget;
            healthHUD.IdHealth = enemy.ID;

            // Conecta o HUD com o sistema de vida do inimigo
            enemy.SetHealthHUD(healthHUD);
            healthHUD.UptadeHealthImagens(enemy.Health / enemy.MaxHealth);
        }
    }
}
