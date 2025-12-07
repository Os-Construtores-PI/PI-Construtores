using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class DeviceSpriteManager : MonoBehaviour
{
    public static DeviceSpriteManager Instance;

    [Header("Sprites")]
    [SerializeField] private Sprite _KeyBoardSprite;
    [SerializeField] private Sprite _xboxSprite;
    [SerializeField] private Sprite _playstationSprite;

    private string _currentDevice = "Keyboard";
    private void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        InputSystem.onAnyButtonPress.Call(OnInputDeteced);
    }

    

    private void OnInputDeteced(InputControl control)
    {
        if(control.device is Keyboard || control.device is Mouse)
        {
            _currentDevice = "Keyboard";
        }
        else if (control.device is Gamepad gamepad)
        {
            string name = gamepad.displayName.ToLower();

            if(name.Contains("dual") || name.Contains("ps"))
            _currentDevice = "Playstation";
            else
            _currentDevice = "Xbox";
        }
        Debug.Log($"[DeviceManager] Atual device : {_currentDevice}");
        
    }

    public Sprite GetCurrentSprite()
    {
        return _currentDevice switch
        {
            "Keyboard"     => _KeyBoardSprite,
            "Playstation"  => _playstationSprite,
            "Xbox"         => _xboxSprite,
            _ => _KeyBoardSprite
        };
    }
}
