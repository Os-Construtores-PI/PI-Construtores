using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

  [Header("Botoes do Pause")]
  [SerializeField] private RectTransform[] _pauseButtons;

  [SerializeField] private float _pauseButtonsDelay = 0.08f;
  [SerializeField] private float _pauseButtonDuration = 0.25f;
  [SerializeField] private float _pauseButtonInitialScale = 0.75f;

  private bool _pauseButtonsAnimating;

  #region  === MENU GAMEOVER ===

  private void Update()
  {
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
    if (_pauseButtonsBackground == null)
      return;

    _pauseBackgroundOriginalPosition = _pauseButtonsBackground.anchoredPosition;
  }

  private void AnimatedPauseButtonsIn()
  {
    if (_pauseButtons == null || _pauseButtons.Length == 0)
    {
      _pauseButtonsAnimating = false;
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

      // Começa invisível
      button.localScale = Vector3.zero;

      // Continua desativado enquanto anima
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

    // Quando TODOS os botões terminarem de aparecer,
    // libera a navegação.
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
        }
      )
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

    GlobalEventBus.Instance.Pause.Invoke(false);

    DOTween.KillAll();

    if (DataDirector.Instance != null)
    {
      DataDirector.Instance.ResetRunTimeState();
      DataDirector.Instance.RestartCurrentLevel();
    }

    GameContext.ShowStageIntro = true;
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
    GlobalEventBus.Instance.Pause.Invoke(false);
    AudioManager.Instance.PlaySFX(_uiAudioConfig.Click);
  }

  public void OnEnable()
  {
    InitializedPauseBackGround();

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
    if (AudioManager.Instance == null || _uiAudioConfig == null)
      return;

    if (isPaused)
    {
      // Esconde os botões imediatamente
      HidePauseButtons();

      // Depois inicia o fundo
      AnimatedPauseBackgroundIn();

      AudioManager.Instance.PlaySFX(_uiAudioConfig.Pause);
    }
    else
    {
      _pauseButtonsAnimating = false;

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

      if (AudioManager.Instance != null && _uiAudioConfig != null)
        AudioManager.Instance.PlaySFX(_uiAudioConfig.Back);
    }
  }



  #endregion === MENU PAUSE ===

  #region  === COMUNS ===
  public void ExitToMainMenu()
  {
    Time.timeScale = 1f;

    // DOTween.Kill(transform);

    GlobalEventBus.Instance.Pause.Invoke(false);

    DOTween.KillAll();

    if (DataDirector.Instance != null)
      DataDirector.Instance.ResetRunTimeState();

    Resources.UnloadUnusedAssets();

    AudioManager.Instance.PlaySFX(_uiAudioConfig.Back);

    _loadingScreen.LoadScene(Constants.SceneNames.MainMenu);
  }

  private void AnimatedPauseBackgroundIn()
  {
    if (_pauseButtonsBackground == null)
      return;

    _pauseButtonsBackground.DOKill();

    // Começa fora da tela, à esquerda
    _pauseButtonsBackground.anchoredPosition =
      _pauseBackgroundOriginalPosition +
      Vector2.left * _pauseBackGroundDistance;

    // Entra até sua posição original
    _pauseButtonsBackground
      .DOAnchorPos(
        _pauseBackgroundOriginalPosition,
        _pauseBackgroundDuration
      )
      .SetEase(Ease.OutCubic)
      .SetUpdate(true)
      .OnComplete(() =>
      {
        AnimatedPauseButtonsIn();
      });
  }
  #endregion
}
