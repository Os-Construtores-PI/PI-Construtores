using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueGlobal : MonoBehaviour
{
  private enum DialogueState
  {
    Closed,
    Opening,
    Open,
    Closing,
  }

  [SerializeField]
  private DialogueAudioConfig _dialogueAudioConfig;

  [SerializeField]
  private float _timePerLetter = 0.015f;

  [SerializeField]
  private GameObject _pandoraLayout;

  [SerializeField]
  private GameObject _enemyLayout;

  [SerializeField]
  private TMP_Text _pandoraText;

  [SerializeField]
  private TMP_Text _enemyText;

  [SerializeField]
  private GameObject _dialoguePanel;

  [SerializeField]
  private GameObject _dialogueButtons;

  [SerializeField]
  private GameObject _gameplayButtons;

  [SerializeField]
  private Button _advanceButton;

  [SerializeField]
  private Button _returnButton;

  public static DialogueGlobal Instance { get; private set; }
  public DialogueTrigger CurrentTrigger { get; set; }
  public bool IsDialogueActive => _dialogueActive;

  public event Action OnDialogueStart;
  public event Action OnDialogueEnd;

  private DialogueState _state = DialogueState.Closed;
  private Tween _tweenPanel;
  private Tween _tweenText;
  private TMP_Text _dialogueText;
  private string[] _currentLines;
  private int _index;
  private bool _dialogueActive;
  private bool _dialogueReady;
  private bool _blockAdvanceInput;
  private bool _waitingForButtonRelease;
  private bool _isTyping;

  private float _nextInputTime;
  private readonly float _inputDelay = 0.15f;

  private PlayerDirector _playerDirector;
  private GameDirector _gameDirector;
  private Player _player;
  private Player _lockedPlayer;
  private PlayerInput _interactableInput;
  private PlayerInput _defaultPlayerInput;

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }
    Instance = this;

    _playerDirector = FindAnyObjectByType<PlayerDirector>();
    _gameDirector = FindAnyObjectByType<GameDirector>();

    if (_playerDirector != null)
      _player = _playerDirector.FirstPlayerContext;

    if (_player != null)
      _defaultPlayerInput = _player.PlayerInput;

    if (_dialoguePanel != null)
      _dialoguePanel.SetActive(false);
  }

  public void SetTrigger(DialogueTrigger trigger)
  {
    CurrentTrigger = trigger;
    if (trigger != null)
      ApplyLayout(trigger.LayoutType);
  }

  public void StartDialogue(string[] lines)
  {
    _blockAdvanceInput = true;
    if (_state != DialogueState.Closed || lines == null || lines.Length == 0)
      return;

    _state = DialogueState.Opening;
    _dialogueActive = true;
    _dialogueReady = false;
    Time.timeScale = 0f;

    SetupInput(true);
    _waitingForButtonRelease = true;

    _interactableInput?.actions["AdvanceDialogue"]?.Reset();
    _interactableInput?.actions["ReturnDialogue"]?.Reset();

    OnDialogueStart?.Invoke();

    if (AudioManager.Instance != null && _dialogueAudioConfig != null)
      AudioManager.Instance.PlaySFX(_dialogueAudioConfig.DialogueOpen);

    _currentLines = lines;
    _index = 0;

    UpdateButtonsVisibility();
    ClearLine();

    if (_dialoguePanel != null)
    {
      _dialoguePanel.SetActive(true);
      _dialoguePanel.transform.localScale = Vector3.zero;
    }

    if (_gameplayButtons != null)
      _gameplayButtons.SetActive(false);

    if (_dialogueButtons != null)
      _dialogueButtons.SetActive(true);

    _tweenPanel?.Kill();
    _tweenText?.Kill();
    StopAllCoroutines();

    if (_dialoguePanel != null)
    {
      _tweenPanel = _dialoguePanel
        .transform.DOScale(1f, 0.30f)
        .SetEase(Ease.OutBack)
        .SetUpdate(true)
        .OnComplete(() =>
        {
          _state = DialogueState.Open;
          _dialogueReady = true;
          _blockAdvanceInput = false;
          UpdateLine();
        });
    }

    _lockedPlayer = _player;

    if (_lockedPlayer != null)
      _lockedPlayer.BlockJumpByDialogue = true;

    if (_gameDirector != null && _lockedPlayer != null)
      _gameDirector.SetLockPlayer(_lockedPlayer, true);
  }

  private void SetupInput(bool isDialogue)
  {
    _interactableInput = CurrentTrigger?.PlayerInput ?? _defaultPlayerInput;
    if (_interactableInput == null)
      return;

    var actions = _interactableInput.actions;
    actions.Enable();

    if (isDialogue)
    {
      actions["AdvanceDialogue"]?.Enable();
      actions["ReturnDialogue"]?.Enable();
      actions["Move"]?.Disable();
      actions["Attack"]?.Disable();
      actions["Dash"]?.Disable();
      _interactableInput.SwitchCurrentActionMap("Dialogue");
    }
    else
    {
      actions["AdvanceDialogue"]?.Disable();
      actions["ReturnDialogue"]?.Disable();
      actions["Move"]?.Enable();
      actions["Attack"]?.Enable();
      actions["Dash"]?.Enable();
      _interactableInput.SwitchCurrentActionMap("Player");
    }
  }

  public void NextLine()
  {
    if (!_dialogueActive || !_dialogueReady || _state != DialogueState.Open)
      return;

    if (_isTyping)
    {
      CompleteTextInstantly();
      return;
    }

    if (_index >= _currentLines.Length - 1)
    {
      CloseDialogue();
      return;
    }

    if (AudioManager.Instance != null && _dialogueAudioConfig != null)
      AudioManager.Instance.PlaySFX(_dialogueAudioConfig.DialogueNext);

    _index++;
    UpdateLine();
  }

  public void PreviousLine()
  {
    if (!_dialogueActive || !_dialogueReady || _state != DialogueState.Open || _index <= 0)
      return;

    if (AudioManager.Instance != null && _dialogueAudioConfig != null)
      AudioManager.Instance.PlaySFX(_dialogueAudioConfig.DialogueBack);

    _index--;
    UpdateLine();
  }

  public void CloseDialogue()
  {
    if (_state == DialogueState.Closed || _state == DialogueState.Closing)
      return;

    _state = DialogueState.Closing;
    _dialogueActive = false;
    _dialogueReady = false;

    UpdateButtonsVisibility();
    OnDialogueEnd?.Invoke();

    if (_dialogueButtons != null)
      _dialogueButtons.SetActive(false);
    if (_gameplayButtons != null)
      _gameplayButtons.SetActive(true);

    if (_gameDirector != null && _lockedPlayer != null)
      _gameDirector.SetLockPlayer(_lockedPlayer, false);

    if (_lockedPlayer != null)
      _lockedPlayer.BlockJumpByDialogue = false;

    CurrentTrigger?.OnDialogueClosed();
    StartCoroutine(CloseDialogueSafely());
  }

  private void Update()
  {
    if (_state != DialogueState.Open || !_dialogueReady || _interactableInput == null)
      return;

    var actions = _interactableInput.actions;
    if (actions == null)
      return;

    var advanceAction = actions["AdvanceDialogue"];
    var returnAction = actions["ReturnDialogue"];

    if (_waitingForButtonRelease)
    {
      if (!advanceAction.IsPressed())
        _waitingForButtonRelease = false;
      return;
    }

    if (Time.unscaledTime < _nextInputTime)
      return;

    if (!_blockAdvanceInput && advanceAction.WasPressedThisFrame())
    {
      _nextInputTime = Time.unscaledTime + _inputDelay;
      NextLine();
      return;
    }

    if (returnAction.WasPressedThisFrame())
    {
      _nextInputTime = Time.unscaledTime + _inputDelay;
      PreviousLine();
    }
  }

  private void UpdateLine()
  {
    if (_currentLines == null || _index < 0 || _index >= _currentLines.Length)
      return;

    ShowLine(_currentLines[_index]);
    UpdateButtonsVisibility();
  }

  private void ShowLine(string text)
  {
    if (_dialogueText == null)
      return;

    _dialogueText.text = text;
    _dialogueText.maxVisibleCharacters = 0;
    _dialogueText.ForceMeshUpdate();

    float duration = Mathf.Clamp(text.Length * _timePerLetter, 0.10f, 1.0f);
    _isTyping = true;

    _tweenText = DOTween
      .To(
        () => _dialogueText.maxVisibleCharacters,
        v => _dialogueText.maxVisibleCharacters = v,
        text.Length,
        duration
      )
      .SetEase(Ease.Linear)
      .SetUpdate(true)
      .OnComplete(() => _isTyping = false);
  }

  private void ClearLine()
  {
    if (_dialogueText == null)
      return;

    _tweenText?.Kill();
    _dialogueText.text = string.Empty;
    _dialogueText.maxVisibleCharacters = 0;
    _dialogueText.ForceMeshUpdate();
  }

  private void ApplyLayout(DialogueLayoutType type)
  {
    if (_pandoraLayout != null)
      _pandoraLayout.SetActive(type == DialogueLayoutType.Pandora);

    if (_enemyLayout != null)
      _enemyLayout.SetActive(type == DialogueLayoutType.Enemy);

    _dialogueText = (type == DialogueLayoutType.Pandora) ? _pandoraText : _enemyText;
  }

  private void UpdateButtonsVisibility()
  {
    if (_returnButton != null)
      _returnButton.gameObject.SetActive(_index > 0);

    if (_advanceButton != null)
      _advanceButton.gameObject.SetActive(true);
  }

  private void OnDisable()
  {
    StopAllCoroutines();
    _tweenPanel?.Kill();
    _tweenText?.Kill();
  }

  private IEnumerator CloseDialogueSafely()
  {
    var dialogueAction = _interactableInput?.actions?["AdvanceDialogue"];

    while (dialogueAction != null && dialogueAction.IsPressed())
      yield return null;

    yield return null;

    SetupInput(false);

    if (_lockedPlayer != null)
      _lockedPlayer.WaitForJumpRelease = true;

    if (_dialoguePanel != null)
    {
      _dialoguePanel
        .transform.DOScale(0f, 0.2f)
        .SetEase(Ease.InBack)
        .SetUpdate(true)
        .OnComplete(() =>
        {
          _dialoguePanel.SetActive(false);
          _state = DialogueState.Closed;
          Time.timeScale = 1f;
        });
    }
  }

  private void CompleteTextInstantly()
  {
    if (_dialogueText == null)
      return;

    _tweenText?.Kill();
    _dialogueText.maxVisibleCharacters = _dialogueText.text.Length;
    _isTyping = false;
  }
}
