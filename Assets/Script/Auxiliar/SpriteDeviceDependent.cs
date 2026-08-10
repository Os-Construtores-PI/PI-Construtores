using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SpriteDeviceDependent : MonoBehaviour
{
  [Header("Sprites per device")]
  [SerializeField]
  private Sprite _keyboardSprite;

  [SerializeField]
  private Sprite _xboxSprite;

  [SerializeField]
  private Sprite _playstationSprite;

  [Header("Targets (optional — auto-detected if left empty)")]
  [SerializeField]
  private Image _targetImage;

  [SerializeField]
  private SpriteRenderer _targetSpriteRenderer;

  public void Awake()
  {
    if (_targetImage == null)
      _targetImage = GetComponent<Image>();

    if (_targetSpriteRenderer == null)
      _targetSpriteRenderer = GetComponent<SpriteRenderer>();
  }

  public void OnEnable()
  {
    if (DeviceInputManager.Instance != null)
    {
      DeviceInputManager.Instance.OnDeviceChanged += ApplySprite;
      ApplySprite(DeviceInputManager.Instance.CurrentDevice);
    }
  }

  public void OnDisable()
  {
    if (DeviceInputManager.Instance != null)
      DeviceInputManager.Instance.OnDeviceChanged -= ApplySprite;
  }

  private void ApplySprite(DeviceType device)
  {
    Sprite sprite = device switch
    {
      DeviceType.Keyboard => _keyboardSprite,
      DeviceType.Xbox => _xboxSprite,
      DeviceType.Playstation => _playstationSprite,
      _ => _keyboardSprite,
    };

    if (sprite == null)
      return;

    if (_targetImage != null)
      _targetImage.sprite = sprite;

    if (_targetSpriteRenderer != null)
      _targetSpriteRenderer.sprite = sprite;
  }
}
