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

  [Header("Pause Animation")]
  [SerializeField] GameObject _pausePanel;

  [SerializeField] private float _pauseEntranceDistance = 1200f;

  [SerializeField] private float _pauseEntranceSpeed = 10f;

  [SerializeField] private Button[] _pauseButtons;

  [SerializeField] private float _buttonsDelay = 0.08f;

  [SerializeField] private float _buttonAnimationSpeed = 12f;


  private Button _currentButton;
  private Vector3 _currentTargetScale = Vector3.one;

  [SerializeField] private LoadingScreen _loadingScreen;

  private Vector3 _pauseFinalPosition;
  private Vector3 _pauseStartPosition;

  private bool _pauseAnimating;
  private bool _pauseButtonsAnimating;

  private float _pauseTimer;

  private CanvasGroup[] _buttonsGroup;

  private Vector3[] _buttonsStartScales;

  private bool _pauseInitialized;

  #region  === MENU GAMEOVER ===

  private void Awake()
  {
    
  }

  private void SetupPauseAnimation()
  {
    if(_pausePanel == null)
    {
      Debug.LogWarning(
        "[BasicMenuLogic] Pause Panel não foi definido");
      return;
    }

    // Guarda a posição ORIGINAL do prefab
    _pauseFinalPosition = 
      _pausePanel.transform.localPosition;

    //Posição inicial: esquerda.
    _pauseStartPosition =
      _pauseFinalPosition +
      Vector3.left * _pauseEntranceDistance;

    //CanvasGroups do botões
    if(_pauseButtons != null &&
      _pauseButtons.Length > 0)
    {
      _buttonsGroup =
        new CanvasGroup[_pauseButtons.Length];

      _buttonsStartScales = 
        new Vector3[_pauseButtons.Length];

      for(int i = 0; i < _pauseButtons.Length; i++)
      {
        if (_pauseButtons[i] == null)
          continue;

        CanvasGroup buttonObject =
          _pauseButtons[i].GetComponent<CanvasGroup>();

        CanvasGroup group = 
          buttonObject.GetComponent<CanvasGroup>();

        if(group == null)
        {
          group = 
            buttonObject.AddComponent<CanvasGroup>();
        }

        _buttonsGroup[i] = group;

        _buttonsStartScales[i] =
          _pauseButtons[i].transform.localScale;
      }
    }

    _pauseInitialized = true;
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
