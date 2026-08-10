using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class FinalSequenceDialogue : MonoBehaviour
{
  [SerializeField]
  private float _continueDuration = 5f;

  [SerializeField]
  private float _fadeDuration = 1f;

  [SerializeField]
  private GameObject _continueScreen;

  [SerializeField]
  private CanvasGroup _continueCanvasGroup;

  private bool _isRunning;

  private void Awake()
  {
    if (_continueScreen != null)
      _continueScreen.SetActive(true);

    if (_continueCanvasGroup != null)
    {
      _continueCanvasGroup.alpha = 0f;
      _continueCanvasGroup.interactable = false;
      _continueCanvasGroup.blocksRaycasts = false;
    }
  }

  public void StartFinalSequence(DialogueTrigger trigger)
  {
    if (_isRunning || trigger == null)
    {
      Debug.LogError("Trigger is null or sequence is already running.");
      return;
    }

    StartCoroutine(FinalFlow(trigger));
  }

  private IEnumerator FinalFlow(DialogueTrigger trigger)
  {
    _isRunning = true;

    GameDirector director = FindAnyObjectByType<GameDirector>();
    if (director?.playerDirector != null)
    {
      director.SetLockPlayer(director.playerDirector.FirstPlayerContext, true);
    }

    PlayerInput playerInput = trigger.PlayerInput;
    if (playerInput != null)
    {
      playerInput.actions["AdvanceDialogue"]?.Reset();
      playerInput.actions["Jump"]?.Reset();
    }

    yield return new WaitUntil(() =>
    {
      if (playerInput == null)
        return true;

      var jump = playerInput.actions["Jump"];
      var advance = playerInput.actions["AdvanceDialogue"];

      return (jump == null || !jump.IsPressed()) && (advance == null || !advance.IsPressed());
    });

    DialogueGlobal.Instance.SetTrigger(trigger);
    DialogueGlobal.Instance.StartDialogue(trigger.DialogueLines);

    bool dialogueFinished = false;

    void OnDialogueEnded()
    {
      dialogueFinished = true;
      DataDirector.Instance.SetSlotCompleted(DataDirector.Instance.GetCurrentSlot(), true);
      DialogueGlobal.Instance.OnDialogueEnd -= OnDialogueEnded;
    }

    DialogueGlobal.Instance.OnDialogueEnd += OnDialogueEnded;

    yield return new WaitUntil(() => dialogueFinished);

    if (_continueCanvasGroup != null)
      _continueCanvasGroup.DOFade(1f, _fadeDuration).SetUpdate(true);

    yield return new WaitForSecondsRealtime(_continueDuration);

    GoToMainMenu();
  }

  private void GoToMainMenu()
  {
    Time.timeScale = 1f;
    GameContext.IsPaused = false;

    DataDirector.Instance?.ResetRunTimeState();
    SceneManager.LoadScene("MainMenu");
  }
}
