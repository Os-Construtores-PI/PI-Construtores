using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class BasicButton : ObjectInteractable
{
    private readonly UnityEvent buttonPressed = new();
    [SerializeField] private ActivatableObject targetobject;

    public virtual void Start()
    {
        if (targetobject)
        {
            buttonPressed.AddListener(targetobject.ObjectAction);
        }
    }
    public override void Interaction()
    {
        print("Butão interação");
        buttonPressed.Invoke();
    }
}
