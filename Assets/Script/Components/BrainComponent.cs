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
    [SerializeField] public Entities identity;
    [SerializeField] public Behavior comportamento;
    [SerializeField] public List<SkillData> skills;
    
    public void MorteCerebral(EntityType type)
    {
        switch (type)
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
