using UnityEngine;
using UnityEngine.Events;

public class PuzzleColorButton : InteractableObject
{
  [SerializeField]
  private ColorCode buttonColorEnum = ColorCode.RED;

  [SerializeField]
  private CodeCapturer targetobject;

  public Code buttonCode;
  private readonly UnityEvent<object> buttonPressed = new();

  public virtual void Start()
  {
    buttonCode = CodeBaseFour.Codes.Find(i => i.Number == (int)buttonColorEnum);
    Transform button = transform.Find("BOTAOHOLDER/BOTAOMOVEL");
    if (button)
    {
      button.TryGetComponent(out MeshRenderer buttonMesh);
      buttonMesh.material.color = buttonCode.Color;
    }
    if (targetobject)
    {
      buttonPressed.AddListener(targetobject.ObjectAction);
    }
  }

  public override void Interaction(InfoPlayerInteraction info)
  {
    if (buttonCode.Equals(null))
      return;
    buttonPressed.Invoke(buttonCode);
  }
}
