using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;



public class EnemyHUDManager : MonoBehaviour
{
    // Array para armazenar todos os GameObjects com a tag "Creature"
    private GameObject[] creatures;

    // Lista para armazenar especificamente os inimigos (do tipo ENEMY)
    private List<GameObject> enemies = new();

    // Prefab do HUD que será instanciado para cada inimigo
    [SerializeField] private GameObject HUD;

    // Objeto pai onde os HUDs serão organizados na hierarquia da cena
    [SerializeField] private Transform pai;

    // Nome do filho do inimigo onde o HUD será posicionado (ex: barra de vida)
    private const string target_name = "HealthBarTarget";


    private void Start()
    {
        // Busca e filtra inimigos na cena
        GetEnemies();

        // Instancia o HUD de vida para cada inimigo encontrado
        InstantiateHealthHUD();
    }

    // Método para buscar todos os GameObjects com tag "Creature" e filtrar inimigos
    private void GetEnemies()
    {
        // Encontra todos os objetos na cena com a tag "Creature"
        creatures = GameObject.FindGameObjectsWithTag("Creature");

        // Percorre todos os objetos encontrados
        foreach (GameObject creature in creatures)
        {
            // Verifica se o objeto possui um componente BrainComponent e se é um inimigo
            if (creature.TryGetComponent(out BrainComponent brain) && brain.identity.TipoEntidade == EntityType.ENEMY)
            {
                // Adiciona o inimigo filtrado na lista de inimigos
                enemies.Add(creature);
            }
        }
    }

    // Método para instanciar o HUD de vida em cima do inimigo
    private void InstantiateHealthHUD()
    {
        // Verifica se o prefab do HUD foi atribuído
        if (!HUD) return;

        // Para cada inimigo encontrado na lista
        foreach (GameObject enemy in enemies)
        {
            // Tenta encontrar o filho com nome "HealthBarTarget" para posicionar o HUD
            Transform target = enemy.transform.Find(target_name);
            if (target)
            {
                // Instancia o HUD na posição do alvo e como filho do objeto "pai"
                GameObject hud = Instantiate(HUD, target.position, Quaternion.identity, pai);

                // Se o HUD possui componente HealthHUDComponent e o inimigo possui HealthComponent
                if (hud.TryGetComponent(out HealthHUDComponent healthHUD) && enemy.TryGetComponent(out HealthComponent health) && enemy.TryGetComponent(out BrainComponent brain))
                {

                    // Define o alvo que o HUD vai seguir (posição do inimigo)
                    healthHUD.enemy_target = target;
                    healthHUD.id_health = brain.identity.ID;

                    // Guarda referência do HUD dentro do componente de saúde para atualizações futuras
                    health.SetHealthHUD(healthHUD);
                    // Atualiza a barra de vida com o valor atual do inimigo
                    if (health.TryGetAttribute("health", out float health_value))
                    {
                        healthHUD.UpdateSlider(health_value);
                    }
                        
                }
            }
        }
    }
}
