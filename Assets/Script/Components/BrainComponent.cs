using TMPro;
using UnityEngine;

public class BrainComponent : ComponentBehaviour
{
    public enum Behavior
    {
        AGRESSIVE,FRIENDLY,NEUTRAL,INDIVIDUAL    
    }


    [Header("Características")]
    [SerializeField] public Entities identity;
    [SerializeField] public Behavior comportamento;
}
