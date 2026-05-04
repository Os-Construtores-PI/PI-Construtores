using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyHUDManager : MonoBehaviour
{
  [Header("Configurações do HUD")]
  [SerializeField]
  private GameObject hudPrefab;

  [SerializeField]
  private Transform hudParent;

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
    enemyObjects = GameObject
      .FindObjectsByType<Enemies>(FindObjectsInactive.Include, FindObjectsSortMode.None)
      .ToList();
  }

  /// <summary>
  /// Cria e configura HUDs para cada inimigo encontrado
  /// </summary>
  private void SpawnEnemyHUDs()
  {
    // foreach (Enemies enemy in enemyObjects)
    // {
    // TODO: Fazer o player spawnar a hud
    // }
  }
}
