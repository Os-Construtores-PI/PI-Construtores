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
  private bool _selecaoInicial;

  private void Awake()
  {
    _podeTocarHover = false;
    _selecaoInicial = true;
  }

  private void Update()
    {
        // Depois que o jogador já começou a interagir,
        // não precisamos mais verificar a primeira interação.
        if (_podeTocarHover)
            return;

        if (TeveInputDeNavegacao())
        {
            _podeTocarHover = true;
            _selecaoInicial = false;
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
        // ==========================================
        // PRIMEIRA SELEÇÃO AUTOMÁTICA
        // ==========================================

        if (_selecaoInicial)
        {
            // Se chegou aqui porque o jogador acabou
            // de apertar uma direção, já libera o som.
            if (TeveInputDeNavegacao())
            {
                _podeTocarHover = true;
                _selecaoInicial = false;

                TocarHover();
            }

            return;
        }

        // ==========================================
        // SELEÇÃO NORMAL
        // ==========================================

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
