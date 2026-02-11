using UnityEngine;
using UnityEngine.InputSystem;

public abstract class InteractableObject : MonoBehaviour
{

    [Header("Sprites de Intera��o")]
    public Sprite _keyboardSprites; //F
    public Sprite _playstationSprites; //X
    public Sprite _xboxSprites; //A
    
    [SerializeField] public float range = 10;

    public virtual void Interaction(InfoPlayerInteraction info)
    {
        
    }

    public virtual Sprite GetCorrentSprite(Player player)
   {  
    if (player == null)
        return _keyboardSprites;
    Debug.Log($"[GetCorrentSprite] _ultimoDispositivo do player = {player._ultimoDispositivo}");

    return player._ultimoDispositivo switch
    {
        InputType.Keyboard => _keyboardSprites,
        InputType.JoystickPlaystation => _playstationSprites,
        InputType.JoystickXbox => _xboxSprites,
        _ => _keyboardSprites
    };
   }
}


