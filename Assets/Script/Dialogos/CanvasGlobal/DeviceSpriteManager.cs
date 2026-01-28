using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using static DeviceSpriteManager;

public class DeviceSpriteManager : MonoBehaviour
{
    public static DeviceSpriteManager Instance;

    [Header("Sprites")]
    [SerializeField] private Sprite _KeyBoardSprite;
    [SerializeField] private Sprite _xboxSprite;
    [SerializeField] private Sprite _playstationSprite;

    [Header("Sprites Dash")]
    [SerializeField] private Sprite _dashKeyboard;
    [SerializeField] private Sprite _dashXbox;
    [SerializeField] private Sprite _dashPlaystation;

    [Header("Sprites Dialogue Buttons - Keyboard")]
    [SerializeField] private Sprite _dialogueAdvancedKeyboard;
    [SerializeField] private Sprite _dialogueReturnKeyboard;

    [Header("Sprites Dialogue Buttons - Playstation")]
    [SerializeField] private Sprite _dialogueAdvancedPlayButton;
    [SerializeField] private Sprite _dialogueReturnPlayButton;

    [Header("Sprites Dialogue Buttons - Xbox")]
    [SerializeField] private Sprite _dialogueAdvancedXboxButton;
    [SerializeField] private Sprite _dialogueReturnXboxButton;

    public event Action<string> OnDeviceChanged;

    private string _currentDevice = "Keyboard";

    private PlayerInput[] _playerInputs = Array.Empty<PlayerInput>();

    
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DontDestroyOnLoad (gameObject);
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

    public Sprite GetSprite(InputIconType type)
    {
        return type switch
        {
            InputIconType.Interact => GetCurrentSprite(),

            InputIconType.Dash => _currentDevice switch
            {
                "Keyboard" => _dashKeyboard,
                "Playstation" => _dashPlaystation,
                "Xbox" => _dashXbox,

                _ => _dashKeyboard
            },

            _ => _KeyBoardSprite
        };
    }

    public Sprite GetDialogueButtonSprite(DialogueButtonType type)
    {
        return _currentDevice switch
        {
            "Keyboard" => type switch
            {
                DialogueButtonType.Advance => _dialogueAdvancedKeyboard,
                DialogueButtonType.Back => _dialogueReturnKeyboard,
                _ => null
            },

            "Playstation" => type switch
            {
                DialogueButtonType.Advance => _dialogueAdvancedPlayButton,
                DialogueButtonType.Back => _dialogueReturnPlayButton,
                _ => null
            },

            "Xbox" => type switch
            {
                DialogueButtonType.Advance => _dialogueAdvancedXboxButton,
                DialogueButtonType.Back => _dialogueReturnXboxButton,
                _ => null
            },

            _ => null

        }; 
    }

    public enum InputIconType
    {
        Interact,
        Dash,
        Attack,
        Jump,
        
    }

    public enum DialogueButtonType
    {
        Advance,
        Back
    }

    public string GetCurrentDevice()
    {
        return _currentDevice;
    }

    public void ForceRefresh()
    {
        OnDeviceChanged?.Invoke(_currentDevice);
    }
}

