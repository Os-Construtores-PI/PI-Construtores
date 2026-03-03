using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static TutorialGlobal;

public class TutorialTrigger : MonoBehaviour
{
  [Header("UI")]
  [SerializeField]
  private ImageTriggerEvent interactionIcon;

  [SerializeField]
  private Image interactionSprite;

  [Header("Config")]
  [SerializeField]
  private bool apenasUmaVez;

  [Header("Tutorial")]
  [SerializeField]
  private TutorialType tutorialType;

  private PlayerInput playerInput;
  private bool jogadorDentro;
  private bool tutorialConsumido;

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    if (interactionSprite != null)
      interactionSprite.gameObject.SetActive(false);

    if (DeviceSpriteManager.Instance != null)
      DeviceSpriteManager.Instance.OnDeviceChanged += AtualizarSprite;

    Debug.Log($"[TutorialTrigger] Start ativo em {gameObject.name}");
  }

  private void OnDestroy()
  {
    if (DeviceSpriteManager.Instance != null)
      DeviceSpriteManager.Instance.OnDeviceChanged -= AtualizarSprite;
  }

  private void OnTriggerEnter(Collider other)
  {
    if (!other.CompareTag("Player"))
      return;
    if (apenasUmaVez && tutorialConsumido)
      return;

    playerInput = other.GetComponent<PlayerInput>();
    jogadorDentro = true;

    AtualizarSprite(DeviceSpriteManager.Instance.GetCurrentDevice());

    if (interactionSprite != null)
      interactionSprite.gameObject.SetActive(true);

    if (interactionIcon != null)
      interactionIcon.Hide();
  }

  private void OnTriggerExit(Collider other)
  {
    if (!other.CompareTag("Player"))
      return;

    jogadorDentro = false;

    if (interactionSprite != null)
      interactionSprite.gameObject.SetActive(false);

    if (interactionIcon != null)
      interactionIcon.Show();
  }

  // Update is called once per frame
  void Update()
  {
    if (!jogadorDentro || playerInput == null)
      return;
    if (TutorialGlobal.Instance == null)
      return;
    if (TutorialGlobal.Instance.IsTutorialActive)
      return;
    if (GameState.IsPaused)
      return;

    var ctx = playerInput.GetComponent<Player>()?.Context;

    if (ctx != null && ctx.IgnoreGameplayInputThisFrame)
      return;

    if (playerInput.actions["Interaction"].WasPerformedThisFrame())
    {
      Debug.Log("[TutorialTrigger] Interaction triggered");
      AbriirTutorial();
    }
  }

  public void AbriirTutorial()
  {
    if (GameState.IsPaused)
      return;

    if (TutorialGlobal.Instance == null)
      return;

    if(TutorialGlobal.Instance.IsTutorialActive)
    tutorialConsumido = true;

    if (interactionSprite != null)
      interactionSprite.gameObject.SetActive(false);

    if (interactionIcon != null)
      interactionIcon.Hide();

    TutorialGlobal.Instance.AbrirTutorial(tutorialType);
  }

  private void AtualizarSprite(string device)
  {
    if (interactionSprite == null)
      return;

    interactionSprite.sprite = DeviceSpriteManager.Instance.GetSprite(
      DeviceSpriteManager.InputIconType.Interact
    );
  }

  public enum TutorialType
  {
    Movimento,
    Combate,
    Dash,
    WallRun,
  }
}
