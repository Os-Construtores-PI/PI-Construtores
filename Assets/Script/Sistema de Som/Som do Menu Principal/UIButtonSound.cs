using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIButtonSound
  : MonoBehaviour,
    IPointerEnterHandler,
    IPointerClickHandler,
    ISelectHandler,
    ISubmitHandler
{
  [SerializeField]
    private UIAudioConfig _uiAudioConfig;

    private bool _podeTocarHover;

    private void Awake()
    {
        _podeTocarHover = false;
    }

    private void OnEnable()
    {
        if (GlobalEventBus.Instance != null)
        {
            GlobalEventBus.Instance.MenuInteraction.AddListener(OnMenuInteractionChanged);
        }

        _podeTocarHover = false;
    }

    private void OnDisable()
    {
        if (GlobalEventBus.Instance != null)
        {
            GlobalEventBus.Instance.MenuInteraction.RemoveListener(OnMenuInteractionChanged);
        }
    }

    private void OnMenuInteractionChanged(bool podeInteragir)
    {
        _podeTocarHover = podeInteragir;
    }

    private void Update()
    {
        // Já liberado.
        if (_podeTocarHover)
            return;

        // Detecta a primeira interação.
        if (TeveInputDeNavegacao())
        {
            _podeTocarHover = true;

            // A partir daqui, qualquer mudança de botão
            // pode emitir o som normalmente.
            TocarHover();
        }
    }

    private bool TeveInputDeNavegacao()
    {
        // =========================
        // TECLADO
        // =========================

        if (Keyboard.current != null)
        {
            if (
                Keyboard.current.upArrowKey.wasPressedThisFrame ||
                Keyboard.current.downArrowKey.wasPressedThisFrame ||
                Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                Keyboard.current.rightArrowKey.wasPressedThisFrame ||
                Keyboard.current.wKey.wasPressedThisFrame ||
                Keyboard.current.sKey.wasPressedThisFrame ||
                Keyboard.current.aKey.wasPressedThisFrame ||
                Keyboard.current.dKey.wasPressedThisFrame
            )
            {
                return true;
            }
        }

        // =========================
        // GAMEPAD
        // =========================

        if (Gamepad.current != null)
        {
            if (
                Gamepad.current.dpad.up.wasPressedThisFrame ||
                Gamepad.current.dpad.down.wasPressedThisFrame ||
                Gamepad.current.dpad.left.wasPressedThisFrame ||
                Gamepad.current.dpad.right.wasPressedThisFrame
            )
            {
                return true;
            }

            Vector2 stick = Gamepad.current.leftStick.ReadValue();

            if (stick.magnitude > 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_podeTocarHover)
            return;

        TocarHover();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TocarClick();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!_podeTocarHover)
            return;

        TocarHover();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        TocarClick();
    }

    private void TocarHover()
    {
        if (
            AudioManager.Instance != null &&
            _uiAudioConfig != null &&
            _uiAudioConfig.Hover != null
        )
        {
            AudioManager.Instance.PlaySFX(_uiAudioConfig.Hover);
        }
    }

    private void TocarClick()
    {
        if (
            AudioManager.Instance != null &&
            _uiAudioConfig != null &&
            _uiAudioConfig.Click != null
        )
        {
            AudioManager.Instance.PlaySFX(_uiAudioConfig.Click);
        }
    }
}
