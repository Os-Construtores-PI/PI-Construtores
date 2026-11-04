using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ButtonExitTutorial : MonoBehaviour
{
  [Header("UI")]
  [SerializeField]
  private Image buttonIcon;
  private PlayerInput playerInput;
  private DeviceSpriteManager deviceSpriteManager;
  private TutorialGlobal tutorialGlobal;

  private bool _confirmacaoBloqueada;

  private void OnEnable()
  {
    if (PlayerInput.all.Count > 0)
      playerInput = PlayerInput.all[0];

    deviceSpriteManager = DeviceSpriteManager.Instance;
    tutorialGlobal = TutorialGlobal.Instance;

    if (deviceSpriteManager != null)
        deviceSpriteManager.OnDeviceChanged += OnDeviceChanged;

    if (TutorialGlobal.Instance != null)
      TutorialGlobal.Instance.OnTutorialStateChanged += OnTutorialStateChanged;

    UpdateButtonSprite();

    if (tutorialGlobal != null)
        OnTutorialStateChanged(tutorialGlobal.IsTutorialActive);

    //DeviceInputManager.Instance.ForceRefresh();
  }

  private void OnDisable()
  {
    if (deviceSpriteManager != null)
        deviceSpriteManager.OnDeviceChanged -= OnDeviceChanged;

    if (tutorialGlobal != null)
        tutorialGlobal.OnTutorialStateChanged -= OnTutorialStateChanged;
  }

  private void Update()
  {
    if (playerInput == null || tutorialGlobal == null)
        return;

    if (!tutorialGlobal.IsTutorialActive)
        return;

    InputAction confirmAction =
          playerInput.actions?.FindAction("Confirm");

    if (confirmAction != null &&
    confirmAction.WasPerformedThisFrame())
    {
      ClosedTutorial();
    }
  }

  public void ClosedTutorial()
  {
    if (tutorialGlobal == null ||
        !tutorialGlobal.IsTutorialActive ||
        _confirmacaoBloqueada)
        return;

    _confirmacaoBloqueada = true;

    // Localiza o Player associado ao PlayerInput.
    Player player = null;

    if (playerInput != null)
    {
        player = playerInput.GetComponent<Player>();

        if (player == null)
            player = playerInput.GetComponentInParent<Player>();
    }

    // Impede que o botão de fechar seja reaproveitado para pular.
    if (player != null)
        player.BlockJumpUntilReleased();

    tutorialGlobal.CloseTutorial();

    CancelInvoke(nameof(LiberarConfirmacao));
    Invoke(nameof(LiberarConfirmacao), 0.2f);
  }

  private void LiberarConfirmacao()
  {
    _confirmacaoBloqueada = false;
  }

  private void OnDeviceChanged(string device)
  {
    UpdateButtonSprite();
  }

  private void UpdateButtonSprite()
  {
    if (buttonIcon == null)
        return;
    
    if (deviceSpriteManager == null)
       deviceSpriteManager = DeviceSpriteManager.Instance;

    if (deviceSpriteManager == null)
    {
      Debug.LogWarning(
        "[ButtonExitTutorial] DeviceSpriteManager não encontrado."
      );

      return;
    }

    Sprite sprite = deviceSpriteManager.GetSprite(
      DeviceSpriteManager.InputIconType.Interact
    );

    if (sprite != null)
    {
      buttonIcon.sprite = sprite;
      buttonIcon.enabled = true;
    }
    else
    {
      Debug.LogWarning(
        "[ButtonExitTutorial] Sprite de fechar não configurado" +
        "para dispositivo" + deviceSpriteManager.GetCurrentDevice()
      );
    }
  }

  private void OnTutorialStateChanged(bool ativo)
  {
    if (buttonIcon != null)
      buttonIcon.enabled = ativo;

    if (ativo)
      DeviceInputManager.Instance.ForceRefresh();
  }
}
