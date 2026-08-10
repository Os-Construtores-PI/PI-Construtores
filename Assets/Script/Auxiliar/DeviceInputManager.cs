using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class DeviceInputManager : MonoBehaviour
{
  [HideInInspector]
  public static DeviceInputManager Instance { get; private set; }

  [HideInInspector]
  public event Action<DeviceType> OnDeviceChanged;

  public DeviceType CurrentDevice { get; private set; } = DeviceType.Keyboard;

  private PlayerInput[] _playerInputs = Array.Empty<PlayerInput>();

  public void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);
  }

  public void Start()
  {
    GlobalEventBus.Instance.InputUpdate.AddListener(OnControlsChanged);

    InputSystem.onAnyButtonPress.Call(OnAnyButtonPress);

    if (_playerInputs.Length > 0)
      DetectDeviceFromPlayerInput(_playerInputs[0]);
  }

  public void OnDestroy()
  {
    foreach (var playerInput in _playerInputs)
      playerInput.onControlsChanged -= OnControlsChanged;
  }

  private void OnControlsChanged(PlayerInput input)
  {
    DetectDeviceFromPlayerInput(input);
  }

  private void OnAnyButtonPress(InputControl control)
  {
    if (control?.device == null)
      return;

    DeviceType detected;

    if (control.device is Keyboard || control.device is Mouse)
      detected = DeviceType.Keyboard;
    else if (control.device is Gamepad gamepad)
      detected = IsPlaystationGamepad(gamepad.displayName)
        ? DeviceType.Playstation
        : DeviceType.Xbox;
    else
      return;

    SetCurrentDevice(detected);
  }

  private void DetectDeviceFromPlayerInput(PlayerInput playerInput)
  {
    string scheme = playerInput.currentControlScheme?.ToLowerInvariant() ?? string.Empty;

    DeviceType detected;

    if (scheme.Contains("keyboard") || scheme.Contains("mouse"))
    {
      detected = DeviceType.Keyboard;
    }
    else if (scheme.Contains("gamepad") && playerInput.devices.Count > 0)
    {
      detected = IsPlaystationGamepad(playerInput.devices[0].displayName)
        ? DeviceType.Playstation
        : DeviceType.Xbox;
    }
    else
    {
      return;
    }

    SetCurrentDevice(detected);
  }

  private static bool IsPlaystationGamepad(string displayName)
  {
    string name = displayName?.ToLowerInvariant() ?? string.Empty;
    return name.Contains("dual") || name.Contains("ps") || name.Contains("playstation");
  }

  private void SetCurrentDevice(DeviceType device)
  {
    if (CurrentDevice == device)
      return;

    CurrentDevice = device;
    OnDeviceChanged?.Invoke(CurrentDevice);
  }

  public void ForceRefresh()
  {
    OnDeviceChanged?.Invoke(CurrentDevice);
  }
}
