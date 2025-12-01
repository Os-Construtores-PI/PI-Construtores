using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{

    [SerializeField] protected float range = 10;
    public virtual void Interaction(InfoPlayerInteraction info)
    {
        
    }
}
