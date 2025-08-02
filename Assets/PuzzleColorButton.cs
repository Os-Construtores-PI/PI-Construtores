using UnityEngine;
using UnityEngine.Events;

public class PuzzleColorButton : InteractableObject
{
    [SerializeField] ColorCODE buttonEnumColor = ColorCODE.RED;
    private Color buttonColor = Color.red;
    [SerializeField] private CodeCapturer targetobject;


    private readonly UnityEvent<object> buttonPressed = new();

    public virtual void Start()
    {
        buttonColor = EnumtoColor.Colors[buttonEnumColor];
        Transform button = transform.Find("BOTAOHOLDER/BOTAOMOVEL");
        if (button)
        {
            button.TryGetComponent(out MeshRenderer buttonMesh);
            buttonMesh.material.color = buttonColor;
        }
        if (targetobject)
        {
            buttonPressed.AddListener(targetobject.ObjectAction);
        }
    }
    public override void Interaction()
    {
        buttonPressed.Invoke(buttonColor);
    }
}
