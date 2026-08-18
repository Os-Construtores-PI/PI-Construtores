using DG.Tweening;
using Unity.VisualScripting;
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

  [SerializeField] private float selectableScale = 1.08f;
  [SerializeField] private float scaleSpeed = 10f;

  [SerializeField] private Transform _painelPause;
  [SerializeField] private float _duracaEntrada = 0.4f;
  [SerializeField] private float _distanciaEntrada = 1000f;

  private Vector3 _posicaoOriginal;


  private Button _currentButton;
  private Vector3 _currentTargetScale = Vector3.one;

  [SerializeField] private LoadingScreen _loadingScreen;


  #region  === MENU GAMEOVER ===
  private void Awake()
  {
    if(_painelPause != null)
       _posicaoOriginal = _painelPause.localPosition;
  }



  private void Update()
  {
    GameObject selected = EventSystem.current.currentSelectedGameObject;

    if(selected == null)
       return;
    
    Button button = selected.GetComponent<Button>();

    if(button != _currentButton)
    {
      if(_currentButton != null)
         _currentButton.transform.localScale = Vector3.one;
        
      _currentButton = button;

      if(_currentButton != null)
         _currentTargetScale = Vector3.one * selectableScale;
    }

    if(_currentButton != null)
    {
      _currentButton.transform.localScale = Vector3.Lerp(
        _currentButton.transform.localScale,
        _currentTargetScale,
        Time.unscaledDeltaTime * scaleSpeed
      );
    }
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
  }
  #endregion === MENU GAMEOVER ===

  #region  === MENU PAUSE ===
  public void OpenOptions()
  {
    Time.timeScale = 1f;

    GlobalEventBus.Instance.Pause.Invoke(false);

    DOTween.KillAll();

    if(DataDirector.Instance != null)
    {
      DataDirector.Instance.ResetRunTimeState();
      DataDirector.Instance.RestartCurrentLevel();

      DataDirector.Instance.ShowStageIntro = false;
    }

    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
  }

  public void ContinueGame()
  {
    GlobalEventBus.Instance.Pause.Invoke(false);
    AudioManager.Instance.PlaySFX(_uiAudioConfig.Click);
  }

  public void OnEnable()
  {
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
      AudioManager.Instance.PlaySFX(_uiAudioConfig.Pause);

      if(_painelPause != null)
    {
      _painelPause.DOKill();

      _painelPause.localPosition += Vector3.left * _distanciaEntrada;

      _painelPause.DOLocalMove(_posicaoOriginal, _duracaEntrada)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }
    }
    else
    {
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
  #endregion
}
