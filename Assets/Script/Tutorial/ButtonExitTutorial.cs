using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ButtonExitTutorial : MonoBehaviour
{
  [Header("UI")]
  [SerializeField]
  private Image buttonIcon;
  private PlayerInput playerInput;

  private void OnEnable()
  {
    if (PlayerInput.all.Count > 0)
      playerInput = PlayerInput.all[0];

    if (TutorialGlobal.Instance != null)
      TutorialGlobal.Instance.OnTutorialStateChanged += OnTutorialStateChanged;

    DeviceInputManager.Instance.ForceRefresh();
  }

  private void OnDisable() { }

  private void Update()
  {
    if (playerInput == null)
      return;
    if (TutorialGlobal.Instance == null)
      return;
    if (!TutorialGlobal.Instance.IsTutorialActive)
      return;

    if (playerInput.actions["Confirm"].WasPerformedThisFrame())
    {
      ClosedTutorial();
    }
  }

  public void ClosedTutorial()
  {
    TutorialGlobal.Instance.CloseTutorial();
  }

  private void OnTutorialStateChanged(bool ativo)
  {
    if (buttonIcon != null)
      buttonIcon.enabled = ativo;

    if (ativo)
      DeviceInputManager.Instance.ForceRefresh();
  }
}
