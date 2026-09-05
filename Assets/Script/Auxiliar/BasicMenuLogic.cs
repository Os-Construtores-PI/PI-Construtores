using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class BasicMenuLogic : MonoBehaviour
{
  [Header("Audio")]
  [SerializeField]
  private BackgroundMusicConfig _backgroundMusicConfig;

  [SerializeField]
  private UIAudioConfig _uiAudioConfig;

  [SerializeField]
  private float selectableScale = 1.08f;

  [SerializeField]
  private float scaleSpeed = 10f;

  private Button _currentButton;
  private Vector3 _currentTargetScale = Vector3.one;

  [SerializeField]
  private LoadingScreen _loadingScreen;

  [Header("Fundo dos Botoes")]
  [SerializeField] private RectTransform _pauseButtonsBackground;

  [SerializeField] private float _pauseBackGroundDistance = 4000f;

  [SerializeField] private float _pauseBackgroundDuration = 0.45f;

  private Vector2 _pauseBackgroundOriginalPosition;

  private bool _pauseVisualsInitialized;

  [Header("Botoes do Pause")]
  [SerializeField] private RectTransform[] _pauseButtons;

  [SerializeField] private float _pauseButtonsDelay = 0.08f;
  [SerializeField] private float _pauseButtonDuration = 0.25f;
  [SerializeField] private float _pauseButtonInitialScale = 0.75f;

  [Header("Indicador de Confirmar / Voltar")]
  [SerializeField] private RectTransform _pauseBottomHint;

  [SerializeField] private float _pauseHintDistance = 300f;
  [SerializeField] private float _pauseHintDuration = 0.35f;

  private Vector2 _pauseHintOriginalPosition;

  private GameObject _lastSelectedButton;

  private bool _pauseButtonsAnimating;

  #region  === MENU GAMEOVER ===

  private void Update()
  {

    CheckButtonHover();

    GameObject selected = EventSystem.current.currentSelectedGameObject;

    if (selected == null)
      return;

    Button button = selected.GetComponent<Button>();

    if (button != _currentButton)
    {
      if (_currentButton != null)
        _currentButton.transform.localScale = Vector3.one;

      _currentButton = button;

      if (_currentButton != null)
        _currentTargetScale = Vector3.one * selectableScale;
    }

    if (_currentButton != null)
    {
      _currentButton.transform.localScale = Vector3.Lerp(
        _currentButton.transform.localScale,
        _currentTargetScale,
        Time.unscaledDeltaTime * scaleSpeed
      );
    }
  }

  private void InitializedPauseBackGround()
  {
     if (_pauseVisualsInitialized)
        return;

    // =========================
    // FUNDO
    // =========================

    if (_pauseButtonsBackground != null)
    {
        _pauseBackgroundOriginalPosition =
            _pauseButtonsBackground.anchoredPosition;
    }

    // =========================
    // HINT
    // =========================

    if (_pauseBottomHint != null)
    {
        _pauseHintOriginalPosition =
            _pauseBottomHint.anchoredPosition;
    }

    _pauseVisualsInitialized = true;

  }

  private void AnimatedPauseButtonsIn()
  {
    if (_pauseButtons == null || _pauseButtons.Length == 0)
    {
      _pauseButtonsAnimating = false;

      // Se não existem botões, mostra o Hint imediatamente
      AnimatedPauseBottomHintIn();

      return;
    }

    _pauseButtonsAnimating = true;

    float totalDuration = 0f;

    for (int i = 0; i < _pauseButtons.Length; i++)
    {
      RectTransform button = _pauseButtons[i];

      if (button == null)
        continue;

      button.DOKill();

      // Começa completamente invisível
      button.localScale = Vector3.zero;

      Button uiButton = button.GetComponent<Button>();

      if (uiButton != null)
        uiButton.interactable = false;

      float delay = i * _pauseButtonsDelay;

      button
        .DOScale(Vector3.one, _pauseButtonDuration)
        .SetEase(Ease.OutBack)
        .SetDelay(delay)
        .SetUpdate(true);

      totalDuration = Mathf.Max(
        totalDuration,
        delay + _pauseButtonDuration
      );
    }

    // Espera TODOS os botões terminarem
    DOVirtual
      .DelayedCall(
        totalDuration,
        () =>
        {
          _pauseButtonsAnimating = false;

          foreach (RectTransform button in _pauseButtons)
          {
            if (button == null)
              continue;

            Button uiButton = button.GetComponent<Button>();

            if (uiButton != null)
              uiButton.interactable = true;
          }

          // AGORA SIM o Confirmar / Voltar aparece
          AnimatedPauseBottomHintIn();
        }
      )
      .SetUpdate(true);
  }

  private void AnimatedPauseBottomHintIn()
  {
    if (_pauseBottomHint == null)
      return;

    _pauseBottomHint.DOKill();

    _pauseBottomHint
      .DOAnchorPos(
        _pauseHintOriginalPosition,
        _pauseHintDuration
      )
      .SetEase(Ease.OutCubic)
      .SetUpdate(true);
  }

  public void Respawn()
  {
    if (AudioManager.Instance != null && _backgroundMusicConfig != null)
      AudioManager.Instance.PlaySFX(_backgroundMusicConfig.GameOverMusic);
    GlobalEventBus.Instance.Respawn.Invoke();
  }

  public void OpenMenuGameOver()
  {
    if (AudioManager.Instance != null && _backgroundMusicConfig != null)
    {
      AudioManager.Instance.PlaySFX(_backgroundMusicConfig.GameOverMusic);
    }
  }

  public void ResetScene()
  {
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    GameContext.ShowStageIntro = true;
  }
  #endregion === MENU GAMEOVER ===

  #region  === MENU PAUSE ===
  public void OpenOptions()
  {
    Time.timeScale = 1f;

    AnimatedPauseButtonsOut(() =>
    {
        GlobalEventBus.Instance.Pause.Invoke(false);

        if (DataDirector.Instance != null)
        {
            DataDirector.Instance.ResetRunTimeState();
            DataDirector.Instance.RestartCurrentLevel();
        }

        GameContext.ShowStageIntro = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    });
  }

  private void HidePauseButtons()
  {
    if (_pauseButtons == null || _pauseButtons.Length == 0)
      return;

    _pauseButtonsAnimating = true;

    foreach (RectTransform button in _pauseButtons)
    {
      if (button == null)
        continue;

      button.DOKill();
      button.localScale = Vector3.zero;

      Button uiButton = button.GetComponent<Button>();

      if (uiButton != null)
        uiButton.interactable = false;
    }
  }

  public void ContinueGame()
  {
    AnimatedPauseButtonsOut(() =>
    {
      GlobalEventBus.Instance.Pause.Invoke(false);
    });
  }

  public void OnEnable()
  {
    InitializedPauseBackGround();

    ResetPauseVisuals();

    if (GlobalEventBus.Instance != null)
        GlobalEventBus.Instance.Pause.AddListener(OnPauseChanged);
  }

  public void OnDisable()
  {
    if (GlobalEventBus.Instance != null)
      GlobalEventBus.Instance.Pause.RemoveListener(OnPauseChanged);
  }

  private void OnPauseChanged(bool isPaused)
  {
    

    if (isPaused)
    {
        PlayPauseSound();

        HidePauseButtons();

        ResetPauseVisuals();

        AnimatedPauseBackgroundIn();
    }
    else
    {
        PlayPauseSound();
    }
  }



  #endregion === MENU PAUSE ===

  #region  === COMUNS ===
  public void ExitToMainMenu()
  {
    Time.timeScale = 1f;

    AnimatedPauseButtonsOut(() =>
    {
        GlobalEventBus.Instance.Pause.Invoke(false);

        if (DataDirector.Instance != null)
            DataDirector.Instance.ResetRunTimeState();

        Resources.UnloadUnusedAssets();

        AudioManager.Instance.PlaySFX(_uiAudioConfig.Pause);

        _loadingScreen.LoadScene(Constants.SceneNames.MainMenu);
    });
  }

  public void ClosePauseWithAnimation()
  {
    if (_pauseButtonsAnimating)
        return;

    AnimatedPauseButtonsOut(() =>
    {
        GlobalEventBus.Instance.Pause.Invoke(false);
    });
  }

  private void AnimatedPauseBackgroundIn()
  {
    if (_pauseButtonsBackground == null)
      return;

    _pauseButtonsBackground.DOKill();

    _pauseButtonsBackground
      .DOAnchorPos(
        _pauseBackgroundOriginalPosition,
        _pauseBackgroundDuration
      )
      .SetEase(Ease.OutCubic)
      .SetUpdate(true)
      .OnComplete(() =>
      {
        // Primeiro os botões
        AnimatedPauseButtonsIn();
      });
  }

  private void PlayPauseSound()
  {
    if (AudioManager.Instance == null)
      return;

    if (_uiAudioConfig == null)
      return;

    if (_uiAudioConfig.Pause == null)
      return;

    AudioManager.Instance.PlaySFX(_uiAudioConfig.Pause);
  }

  private void PlayHoverSound()
  {
    if (AudioManager.Instance == null)
      return;

    if (_uiAudioConfig == null)
      return;

    if (_uiAudioConfig.Hover == null)
      return;

    AudioManager.Instance.PlaySFX(_uiAudioConfig.Hover);
  }

  private void CheckButtonHover()
  {
    if (EventSystem.current == null)
      return;

    GameObject selected = EventSystem.current.currentSelectedGameObject;

    if (selected == null)
      return;

    // So toca o som quando a seleção realmente mudou

    if (selected == _lastSelectedButton)
      return;

    _lastSelectedButton = selected;

    Button button = selected.GetComponent<Button>();

    if (button == null)
      return;

    if (!button.interactable)
      return;

    PlayHoverSound();
  }

  private void ResetPauseVisuals()
  {
    // =========================
    // FUNDO
    // =========================

    if (_pauseButtonsBackground != null)
    {
      _pauseButtonsBackground.DOKill();

      _pauseButtonsBackground.anchoredPosition =
        _pauseBackgroundOriginalPosition +
        Vector2.left * _pauseBackGroundDistance;
    }


    // =========================
    // BOTÕES
    // =========================

    if (_pauseButtons != null)
    {
      foreach (RectTransform button in _pauseButtons)
      {
        if (button == null)
          continue;

        button.DOKill();

        button.localScale = Vector3.zero;

        Button uiButton = button.GetComponent<Button>();

        if (uiButton != null)
          uiButton.interactable = false;
      }
    }


    // =========================
    // HINT INFERIOR
    // =========================

    if (_pauseBottomHint != null)
    {
      _pauseBottomHint.DOKill();

      _pauseBottomHint.anchoredPosition =
        _pauseHintOriginalPosition +
        Vector2.down * _pauseHintDistance;
    }

    _pauseButtonsAnimating = false;
  }

  public void AnimatedPauseBackgroundOut(Action onComplete = null)
{
  if (_pauseButtonsBackground == null)
  {
    onComplete?.Invoke();
    return;
  }

  _pauseButtonsBackground.DOKill();

  _pauseButtonsBackground
    .DOAnchorPos(
      _pauseBackgroundOriginalPosition +
      Vector2.left * _pauseBackGroundDistance,
      _pauseBackgroundDuration
    )
    .SetEase(Ease.InCubic)
    .SetUpdate(true)
    .OnComplete(() =>
    {
      onComplete?.Invoke();
    });
}

private void AnimatedPauseButtonsOut(Action onComplete = null)
  {
    if(_pauseButtons == null || _pauseButtons.Length == 0)
    {
      onComplete?.Invoke();
      return;
    }

    _pauseButtonsAnimating = true;

    float totalDuration = 0;

    for (int i = 0; i < _pauseButtons.Length; i++)
    {
      RectTransform button = _pauseButtons[i];

      if(button == null)
         continue;
      
      button.DOKill();

      Button uiButton = button.GetComponent<Button>();

      if(uiButton != null)
         uiButton.interactable = false;
        
      float delay = i * _pauseButtonsDelay;

      button
         .DOScale(Vector3.zero, _pauseButtonDuration)
         .SetEase(Ease.InBack)
         .SetDelay(delay)
         .SetUpdate(true);
      
      totalDuration = Mathf.Max(
        totalDuration, 
        delay + _pauseButtonDuration
      );
    }

    DOVirtual
         .DelayedCall(
          totalDuration,
          () =>
          {
            _pauseButtonsAnimating = false;

            AnimatedPauseBackgroundOut(() =>
            {
              AnimatedPauseBottomHintOut(onComplete);
            });
          }
         )
         .SetUpdate(true);
  }

  private void AnimatedPauseBottomHintOut(Action onComplete = null)
  {
    if(_pauseBottomHint == null)
    {
      onComplete?.Invoke();
      return;
    }

    _pauseBottomHint.DOKill();

     _pauseBottomHint
        .DOAnchorPos(
            _pauseHintOriginalPosition +
            Vector2.down * _pauseHintDistance,
            _pauseHintDuration
        )
        .SetEase(Ease.InCubic)
        .SetUpdate(true)
        .OnComplete(() =>
        {
            onComplete?.Invoke();
        });
  }
  #endregion
}
