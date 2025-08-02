using UnityEngine;
using UnityEngine.Events;

public class PuzzleColorButton : InteractableObject
{
    [SerializeField] Color buttonColor = Color.white;
    [SerializeField] private CodeCapturer targetobject;


    private readonly UnityEvent<object> buttonPressed = new();

    public virtual void Start()
    {
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
        print("Butão interação");
        buttonPressed.Invoke(buttonColor);
    }
}
