using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Componente que representa o "cérebro" de uma entidade (jogador, inimigo, NPC etc.)
public class BrainComponent : ComponentBehaviour
{
    // Enum para definir o comportamento da entidade (usado provavelmente por IA)
    public enum Behavior
    {
        AGRESSIVE,
        FRIENDLY,
        NEUTRAL,
        INDIVIDUAL
    }

    [Header("Características")]
    // Identidade da entidade (script que provavelmente guarda tipo, nome, status etc.)
    [SerializeField] public Entidade identity;

    // Comportamento da entidade (definido no enum acima)
    public Behavior comportamento;

    // Referência ao componente de inventário
    private InventoryComponent inventory;

    // Propriedade pública de acesso ao inventário
    public InventoryComponent Inventory => inventory;
    private HUDDirector huddirector;
    private GameDirector director;


    // Dicionário com ações a serem executadas ao morrer, dependendo do tipo da entidade
    private static readonly Dictionary<EntityType, System.Action<GameObject>> onDeathActions =
        new()
    {
        {
            // Quando o jogador morre, tenta desligar o mundo ou volta para o menu
            EntityType.PLAYER, static go =>
            {
                GameObject directorgo = GameObject.FindWithTag("GameController");
                if(directorgo != null & directorgo.TryGetComponent(out HUDDirector huddir))
                {
                    huddir.ShowGameOver();
                }
                else
                    SceneManager.LoadScene("MenuGame"); // Alternativa de fallback
            }
        },
        {
            // Quando inimigo morre, simplesmente desativa o GameObject
            EntityType.ENEMY, static go => go.SetActive(false)
        },
        {
            // Para entidades genéricas, mesma ação do inimigo
            EntityType.ENTITY, static go => go.SetActive(false)
        }
    };

    // Ao iniciar o componente, tenta obter o InventoryComponent e faz verificações de debug
    private void Awake()
    {
        GameObject directorgo = GameObject.FindWithTag("GameController");

        if (directorgo != null)
        {
            directorgo.TryGetComponent(out huddirector);
            directorgo.TryGetComponent(out director);
        };

        TryGetComponent(out inventory);
        DebugChecks();
    }

    // Faz a entidade usar um item do inventário
    public void CerebroUsarItem(ItemDataBase item)
    {
        if (Inventory != null)
        {
            Inventory.UseItem(item);
        }
        else
        {
            Debug.LogWarning($"Inventário não encontrado para usar o item {(item != null ? item.itemName : "null")}");
        }
    }

    // Método para ser chamado quando a entidade "morre"
    public void MorteCerebral()
    {
        // Busca a ação correspondente ao tipo de entidade no dicionário
        if (!onDeathActions.TryGetValue(identity.TipoEntidade, out var action))
        {
            Debug.LogWarning($"Tipo de entidade inválido ou não definido para {gameObject.name}");
            return;
        }

        // Executa a ação apropriada para o tipo da entidade
        action.Invoke(gameObject);
    }

    // Adiciona um item ao inventário da entidade
    public void AddItem(ItemDataBase item, int quantity)
    {
        if (Inventory != null)
        {
            Inventory.AddItem(item, quantity);
        }
    }

    public void EventoDano()
    {
        if (huddirector != null)
        {
            huddirector.ShakeCamera();
        }
    }


    // Verificações para garantir que a identidade da entidade esteja correta
    private void DebugChecks()
    {
        ErrorType status = ErrorType.SUCCESS;

        // Se o objeto for um jogador mas o tipo da entidade não for PLAYER
        if (gameObject.CompareTag("Player") && identity.TipoEntidade != EntityType.PLAYER)
            status = ErrorType.ENTITYTYPE_ERROR;

        // Se for um inimigo mas o tipo do inimigo não foi definido
        else if (identity.TipoEntidade == EntityType.ENEMY && identity.TipoInimigo == EnemyType.NONE)
            status = ErrorType.ENEMYTYPE_ERROR;

        // Se houve erro, registra no console
        if (status != ErrorType.SUCCESS)
            Debug.LogError($"Erro: {status}, objeto: {gameObject.name}");
    }
}
