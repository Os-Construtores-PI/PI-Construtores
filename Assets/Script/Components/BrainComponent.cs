using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BrainComponent : ComponentBehaviour
{
    public enum Behavior
    {
        AGRESSIVE,
        FRIENDLY,
        NEUTRAL,
        INDIVIDUAL
    }

    [Header("Características")]
    [SerializeField] public Entidade identity;
    public Behavior comportamento;
    public List<SkillData> skills;

    private InventoryComponent inventory;
    public InventoryComponent Inventory => inventory;

    private static readonly Dictionary<EntityType, System.Action<GameObject>> onDeathActions = 
        new()
    {
        {
            EntityType.PLAYER, static go =>
            {
                var director = GameObject.FindWithTag("GameController")?.GetComponent<GameDirector>();
                if (director != null)
                    director.ShutdownWorld();
                else
                    SceneManager.LoadScene("MenuGame");
            }
        },
        {
            EntityType.ENEMY, static go => go.SetActive(false)
        },
        {
            EntityType.ENTITY, static go => go.SetActive(false)
        }
    };

    private void Awake()
    {
        TryGetComponent(out inventory);
        DebugChecks();
    }

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

    public void MorteCerebral()
    {
        if (!onDeathActions.TryGetValue(identity.TipoEntidade, out var action))
        {
            Debug.LogWarning($"Tipo de entidade inválido ou não definido para {gameObject.name}");
            return;
        }
        action.Invoke(gameObject);
    }

    public void AddItem(ItemDataBase item, int quantity)
    {
        if (Inventory != null)
        {
            Inventory.AddItem(item, quantity);
        }
    }

    private void DebugChecks()
    {
        ErrorType status = ErrorType.SUCCESS;

        if (gameObject.CompareTag("Player") && identity.TipoEntidade != EntityType.PLAYER)
            status = ErrorType.ENTITYTYPE_ERROR;
        else if (identity.TipoEntidade == EntityType.ENEMY && identity.TipoInimigo == EnemyType.NONE)
            status = ErrorType.ENEMYTYPE_ERROR;

        if (status != ErrorType.SUCCESS)
            Debug.LogError($"Erro: {status}, objeto: {gameObject.name}");
    }
}
