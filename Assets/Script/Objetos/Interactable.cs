using UnityEngine;
using UnityEngine.InputSystem;

public abstract class InteractableObject : MonoBehaviour
{

    [Header("Sprites de Intera��o")]
    public Sprite _keyboardSprites; //F
    public Sprite _playstationSprites; //X
    public Sprite _xboxSprites; //A

    [SerializeField] protected float range = 10;

    public virtual void Interaction(InfoPlayerInteraction info)
    {
        
    }

    public virtual Sprite GetCorrentSprite(PlayerInput _playerInput)
    {

        if (_playerInput == null)
            return _keyboardSprites;
        string device = _playerInput.currentControlScheme;
        switch (device)
        {
            case "Keyboard&Mouse" :
            case "Keyboard":
                return _keyboardSprites;

            case "Playstation":
                return _playstationSprites;
            case "Xbox" :
                return _xboxSprites;

            default:
                return _keyboardSprites;
        }
    }
}
