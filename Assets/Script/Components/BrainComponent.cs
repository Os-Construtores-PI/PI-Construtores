using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BrainComponent : ComponentBehaviour
{
    // Enum que define os comportamentos possíveis da entidade
    public enum Behavior
    {
        AGRESSIVE,  // Comportamento agressivo (ataca automaticamente)
        FRIENDLY,   // Comportamento amigável (aliado)
        NEUTRAL,    // Comportamento neutro (reage apenas quando atacado)
        INDIVIDUAL  // Comportamento individual (lógica personalizada)
    }

    [Header("Características")]
    [SerializeField]
    public Entidade identity;      // Identidade da entidade (tipo, ID, etc.)
    public Behavior comportamento; // Comportamento atual da entidade
    public List<SkillData> skills; // Lista de habilidades disponíveis
    private InventoryComponent inventory; // Referência ao componente de inventário

    private void Awake()
    {
        // Tenta obter o componente de inventário
        TryGetComponent(out inventory);
        // Executa verificações de debug
        DebugChecks();
    }

    /// <summary>
    /// Método para usar um item do inventário
    /// </summary>
    /// <param name="item">Dados do item a ser usado</param>
    public void CerebroUsarItem(ItemData item)
    {
        if (inventory != null)
        {
            inventory.UseItem(item); // Delega o uso do item para o inventário
        }
        else
        {
            Debug.LogWarning("Nenhum inventário encontrado para usar o item.");
        }
    }

    /// <summary>
    /// Método chamado quando a entidade morre
    /// </summary>
    public void MorteCerebral()
    {
        switch (identity.TipoEntidade)
        {
            case EntityType.PLAYER:
                // Lógica de morte do jogador
                GameObject Director = GameObject.FindWithTag("GameController");
                if (Director && Director.TryGetComponent(out GameDirector directorscript))
                {
                    directorscript.ShutdownWorld(); // Chama o game over
                }
                else
                {
                    SceneManager.LoadScene("MenuGame"); // Volta ao menu se não encontrar o director
                }
                break;
                
            case EntityType.ENEMY:
                // Lógica de morte para inimigos (simples desativação)
                gameObject.SetActive(false);
                break;
                
            case EntityType.ENTITY:
                // Lógica de morte para entidades genéricas
                gameObject.SetActive(false);
                break;
                
            default:
                Debug.Log("Você precisa colocar um tipo para este gameobj");
                break;
        }
    }

    /// <summary>
    /// Realiza verificações de consistência no setup da entidade
    /// </summary>
    private void DebugChecks()
    {
        ErrorType status;
        
        // Verifica se objetos marcados como Player tem o tipo correto
        if (gameObject.CompareTag("Player") && identity.TipoEntidade != EntityType.PLAYER)
        {
            status = ErrorType.ENTITYTYPE_ERROR;
        }
        // Verifica se inimigos tem um tipo definido
        else if (identity.TipoEntidade == EntityType.ENEMY && identity.TipoInimigo == EnemyType.NONE)
        {
            status = ErrorType.ENEMYTYPE_ERROR;
        }
        else
        {
            status = ErrorType.SUCCESS;
        }
        
        // Loga qualquer erro encontrado
        if (status != ErrorType.SUCCESS)
        {
            print($"Erro: {status}, Culpado: {gameObject.name}");
        }
    }
}