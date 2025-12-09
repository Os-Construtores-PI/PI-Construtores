using System;
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

    public event Action<string> OnDeviceChanged;

    private string _currentDevice = "Keyboard";

    private PlayerInput[] _playerInputs = Array.Empty<PlayerInput>();

    
    private void Awake()
    {
        if(Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        _playerInputs = UnityEngine.Object.FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);

        foreach (var p in _playerInputs)
            p.onControlsChanged += OnControlsChanged;

        // callback global para qualquer botão pressionado
        InputSystem.onAnyButtonPress.Call(OnAnyButtonPress);

        // detectar estado inicial
        if (_playerInputs.Length > 0)
            DetectarDevice(_playerInputs[0]);
    }

    private void OnDestroy()
    {
        foreach (var p in _playerInputs)
           p.onControlsChanged -= OnControlsChanged;
        
        InputSystem.onAnyButtonPress.Call(OnAnyButtonPress);
    }
    private void OnControlsChanged(PlayerInput input)
    {
        DetectarDevice(input);
    }

    private void OnAnyButtonPress(InputControl control)
    {
        if (control == null || control.device == null)
            return;

        string previous = _currentDevice;

        if (control.device is Keyboard || control.device is Mouse)
            _currentDevice = "Keyboard";
        else if (control.device is Gamepad)
        {
            var name = control.device.displayName.ToLower();

            if (name.Contains("dual") || name.Contains("ps") || name.Contains("playstation"))
                _currentDevice = "Playstation";
            else
                _currentDevice = "Xbox";
        }

        if (previous != _currentDevice)
            OnDeviceChanged?.Invoke(_currentDevice);
    }

    

    private void DetectarDevice(PlayerInput p)
    {
        string scheme = p.currentControlScheme.ToLower();

        string previous = _currentDevice;

        if (scheme.Contains("keyboard") || scheme.Contains("mouse"))
            _currentDevice = "Keyboard";
        else if (scheme.Contains("gamepad"))
        {
            var d = p.devices[0].displayName.ToLower();
            if (d.Contains("ps") || d.Contains("dual") || d.Contains("playstation"))
                _currentDevice = "Playstation";
            else
                _currentDevice = "Xbox";
        }

        if (previous != _currentDevice)
            OnDeviceChanged?.Invoke(_currentDevice);
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

