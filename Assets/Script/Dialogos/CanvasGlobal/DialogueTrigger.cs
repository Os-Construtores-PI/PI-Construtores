using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueTrigger : MonoBehaviour
{
  [SerializeField]
  private string[] _dialogueLines = { };

  [SerializeField]
  private TextMeshProUGUI _tutorialText;

  [SerializeField]
  private Image _interactionIcon;

  [SerializeField]
  private bool _dialogueOnlyOnce;

  [SerializeField]
  private ImageTriggerEvent _imageTriggerEvent;

  [SerializeField]
  private DialogueLayoutType _layoutType = DialogueLayoutType.Pandora;

  private PlayerInput _playerInput;
  private bool _playerInside;
  private bool _canInteractAgain = true;
  private bool _isPaused;
  private bool _dialogueConsumed;

  public PlayerInput PlayerInput => _playerInput;
  public string[] DialogueLines => _dialogueLines;
  public DialogueLayoutType LayoutType => _layoutType;

  private void OnEnable()
  {
    GlobalEventBus.Instance?.Pause.AddListener(OnPauseChanged);
  }

  private void OnDisable()
  {
    GlobalEventBus.Instance?.Pause.RemoveListener(OnPauseChanged);
  }

  private void OnPauseChanged(bool isPaused)
  {
    _isPaused = isPaused;

    if (_interactionIcon == null)
      return;

    if (isPaused)
    {
      _interactionIcon.gameObject.SetActive(false);
      _playerInput?.actions["Interaction"]?.Disable();
    }
    else
    {
      _playerInput?.actions["Interaction"]?.Enable();

      if (_playerInside && !_dialogueConsumed)
        _interactionIcon.gameObject.SetActive(true);
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    if (!other.CompareTag("Player"))
      return;

    if (_dialogueOnlyOnce && _dialogueConsumed)
      return;

    _playerInput = other.GetComponent<PlayerInput>();
    _playerInside = true;

    if (_tutorialText != null && _dialogueLines != null && _dialogueLines.Length > 0)
      _tutorialText.text = _dialogueLines[0];

    if (_interactionIcon != null)
      _interactionIcon.gameObject.SetActive(true);

    DialogueGlobal.Instance?.SetTrigger(this);
  }

  private void OnTriggerExit(Collider other)
  {
    if (!other.CompareTag("Player"))
      return;

    _playerInside = false;

    if (_interactionIcon != null)
      _interactionIcon.gameObject.SetActive(false);

    if (DialogueGlobal.Instance?.CurrentTrigger == this)
      DialogueGlobal.Instance.CurrentTrigger = null;
  }

  private void Update()
  {
    if (_isPaused || DialogueGlobal.Instance == null || !_playerInside || _playerInput == null)
      return;

    if (_dialogueOnlyOnce && _dialogueConsumed)
      return;

    if (DialogueGlobal.Instance.IsDialogueActive)
      return;

    if (_canInteractAgain && _playerInput.actions["Interaction"].WasPerformedThisFrame())
      OpenDialogue();
  }

  public void OpenDialogue()
  {
    try
    {
      _playerInput?.actions["Interaction"]?.Reset();
    }
    catch { }

    if (_interactionIcon != null)
      _interactionIcon.gameObject.SetActive(false);

    _imageTriggerEvent?.Hide();

    DialogueGlobal.Instance.SetTrigger(this);
    DialogueGlobal.Instance.StartDialogue(_dialogueLines);
  }

  public void OnDialogueClosed()
  {
    if (_dialogueOnlyOnce)
    {
      _dialogueConsumed = true;

      if (_interactionIcon != null)
        _interactionIcon.gameObject.SetActive(false);

      return;
    }

    if (_playerInside)
    {
      if (_interactionIcon != null)
        _interactionIcon.gameObject.SetActive(true);

      _imageTriggerEvent?.Show();
    }

    BlockInteraction();
  }

  private void BlockInteraction()
  {
    _canInteractAgain = false;
    Invoke(nameof(UnblockInteraction), 0.15f);
  }

  private void UnblockInteraction()
  {
    _canInteractAgain = true;
  }
}
