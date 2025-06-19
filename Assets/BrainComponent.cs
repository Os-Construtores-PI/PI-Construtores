using TMPro;
using UnityEngine;

public class BrainComponent : ComponentBehaviour
{
    public enum Behavior
    {
        Agressive,Friendly,Neutral,Individual    
    }


    [Header("Características")]
    [SerializeField] public Entities identity;
    [SerializeField] public Behavior comportamento;
}
