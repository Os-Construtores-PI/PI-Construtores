using UnityEngine;
using UnityEngine.Events;

public class BasicButton : InteractableObject
{
  private readonly UnityEvent<object> buttonPressed = new();

  [SerializeField]
  private ActivatableObject targetobject;

  public virtual void Start()
  {
    if (targetobject)
    {
      buttonPressed.AddListener(targetobject.ObjectAction);
    }
  }

  public override void Interaction(Player _)
  {
    buttonPressed.Invoke(default);
  }
}
