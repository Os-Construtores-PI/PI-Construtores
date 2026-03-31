using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class FinalSequenceDialogue : MonoBehaviour
{
  [SerializeField]
  private DialogueTrigger finalDialogueTrigger;

  [SerializeField]
  private GameObject _continueScreen;

  [SerializeField]
  private float _continueDuration = 5f;

  [SerializeField]
  private CanvasGroup _continueCanvasGroup;

  [SerializeField]
  private float _fadeDuration = 1f;

  private bool isRunning = false;

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
    if (isRunning)
      return;
    if (trigger == null)
    {
      Debug.LogError("trigger passado � null");
      return;
    }

    StartCoroutine(FinalFlow(trigger));
  }

  private IEnumerator FinalFlow(DialogueTrigger trigger)
  {
    isRunning = true;

    GameDirector director = FindAnyObjectByType<GameDirector>();

    if (director != null && director.playerDirector != null)
    {
      director.SetLockPlayer(director.playerDirector.FirstPlayerContext, true);
    }

    PlayerInput playerInput = trigger._playerInput;
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
    DialogueGlobal.Instance.IniciarDialogo(trigger._dialogo);

    bool dialogueFinished = false;

    void handler()
    {
      dialogueFinished = true;
      DataDirector.Instance.SetSlotCompleted(DataDirector.Instance.GetCurrentSlot(), true);
      DialogueGlobal.Instance.OndialogueEnd -= handler;
    }

    DialogueGlobal.Instance.OndialogueEnd += handler;

    yield return new WaitUntil(() => dialogueFinished);

    _continueCanvasGroup.DOFade(1f, _fadeDuration).SetUpdate(true);

    yield return new WaitForSecondsRealtime(_continueDuration);

    GoToMainMenu();
  }

  private void GoToMainMenu()
  {
    Time.timeScale = 1f;
    GameContext.IsPaused = false;

    if (DataDirector.Instance != null)
      DataDirector.Instance.ResetRunTimeState();
    SceneManager.LoadScene("MainMenu");
  }
}
