using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static TutorialGlobal;

public class TutorialTrigger : MonoBehaviour
{
  [Header("UI")]
  [SerializeField]
  private ImageTriggerEvent _interactionIcon;

  [SerializeField]
  private Image _interactionSprite;

  [Header("Config")]
  [SerializeField]
  private bool _onlyOnce;

  [Header("Tutorial")]
  [SerializeField]
  private TutorialType _tutorialType;

  private PlayerInput _playerInput;
  private bool _playerInside;
  private bool _tutorialConsumed;

  public void Start()
  {
    if (_interactionSprite != null)
      _interactionSprite.gameObject.SetActive(false);

    Debug.Log($"[TutorialTrigger] Start ativo em {gameObject.name}");
  }

  public void OnTriggerEnter(Collider other)
  {
    if (!other.CompareTag("Player"))
      return;
    if (_onlyOnce && _tutorialConsumed)
      return;

    _playerInput = other.GetComponent<PlayerInput>();
    _playerInside = true;

    DeviceInputManager.Instance.ForceRefresh();

    if (_interactionSprite != null)
      _interactionSprite.gameObject.SetActive(true);

    if (_interactionIcon != null)
      _interactionIcon.Hide();
  }

  public void OnTriggerExit(Collider other)
  {
    if (!other.CompareTag("Player"))
      return;

    _playerInside = false;

    if (_interactionSprite != null)
      _interactionSprite.gameObject.SetActive(false);

    if (_interactionIcon != null)
      _interactionIcon.Show();
  }

  public void Update()
  {
    if (!_playerInside || _playerInput == null)
      return;
    if (TutorialGlobal.Instance == null)
      return;
    if (TutorialGlobal.Instance.IsTutorialActive)
      return;
    if (GameContext.IsPaused)
      return;

    Player player = _playerInput.GetComponent<Player>();

    if (player != null && player.IgnoreGameplayInputThisFrame)
      return;

    if (_playerInput.actions["Interaction"].WasPerformedThisFrame())
    {
      Debug.Log("[TutorialTrigger] Interaction triggered");
      OpenTutorial();
    }
  }

  public void OpenTutorial()
  {
    if (GameContext.CanPause())
      return;

    if (Instance == null)
      return;

    if (Instance.IsTutorialActive)
      _tutorialConsumed = true;

    if (_interactionSprite != null)
      _interactionSprite.gameObject.SetActive(false);

    if (_interactionIcon != null)
      _interactionIcon.Hide();

    Instance.OpenTutorial(_tutorialType);
  }

  public enum TutorialType
  {
    Movement,
    Combat,
    Dash,
    WallRun,
  }
}
