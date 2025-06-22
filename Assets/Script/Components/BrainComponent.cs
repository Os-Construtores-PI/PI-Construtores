using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BrainComponent : ComponentBehaviour
{
    public enum Behavior
    {
        AGRESSIVE, FRIENDLY, NEUTRAL, INDIVIDUAL
    }


    [Header("Características")]
    [SerializeField]
    public readonly Entities identity;
    public Behavior comportamento;
    public List<SkillData> skills;
    private InventoryComponent inventory;

    private void Awake()
    {
        TryGetComponent(out inventory);
    }

    public void CerebroUsarItem(ItemData item)
    {
        if (inventory != null)
        {
            inventory.UseItem(item);
        }
        else
        {
            Debug.LogWarning("Nenhum inventário encontrado para usar o item.");
        }
    }
    public void MorteCerebral()
    {
        switch (identity.TipoEntidade)
        {
            case EntityType.PLAYER:
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
            case EntityType.ENEMY:
                // Animação de morte e desativamento...
                gameObject.SetActive(false);
                break;
            case EntityType.ENTITY:
                // Flick branco e desativamento...
                gameObject.SetActive(false);
                break;
            default:
                Debug.Log("Você precisa colocar um tipo para este gameobj");
                break;
        }
    }
}
