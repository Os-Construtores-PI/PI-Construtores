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

  [SerializeField] private float selectableScale = 1.08f;
  [SerializeField] private float scaleSpeed = 10f;

  [SerializeField] private Transform _painelPause;
  [SerializeField] private float _duracaEntrada = 0.4f;
  [SerializeField] private float _distanciaEntrada = 1000f;

  [SerializeField] private CanvasGroup _botoesPause;
  [SerializeField] private CanvasGroup _fundoPreto;

  [SerializeField] private Transform _referenciasPause;
  [SerializeField] private float _distanciaReferencias = 200f;

  private Vector3 _posicaoReferencias;

  private Vector3 _posicaoOriginal;


  private Button _currentButton;
  private Vector3 _currentTargetScale = Vector3.one;

  [SerializeField] private LoadingScreen _loadingScreen;

  private Sequence _pauseSequence;
  private bool _pauseAberto;


  #region  === MENU GAMEOVER ===
  private void Awake()
  {
    if(_painelPause != null)
       _posicaoOriginal = _painelPause.localPosition;

    if (_referenciasPause != null)
      _posicaoReferencias = _referenciasPause.localPosition;
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

  private void KillSequenceAnimation()
  {
    if(_pauseSequence != null && _pauseSequence.IsActive())
    {
      _pauseSequence.Kill();
      _pauseSequence = null;
    }

    _painelPause?.DOKill();
    _botoesPause?.DOKill();
    _fundoPreto?.DOKill();
    _referenciasPause?.DOKill();
  }

  private void OnPauseChanged(bool isPaused)
  {
    if(AudioManager.Instance == null || _uiAudioConfig == null)
       return;
    
    KillSequenceAnimation();

    _pauseAberto = isPaused;

    if (isPaused)
    {
      AudioManager.Instance.PlaySFX(_uiAudioConfig.Pause);

      AbrirPause();
    }
    else
    {
      FecharPause();
    }
  }

  private void AbrirPause()
  {
    if (_painelPause == null)
        return;

    // --------------------------------------------------
    // ESTADO INICIAL
    // --------------------------------------------------

    _painelPause.localPosition =
        _posicaoOriginal + Vector3.left * _distanciaEntrada;

    if (_referenciasPause != null)
    {
        _referenciasPause.localPosition =
            _posicaoReferencias + Vector3.down * _distanciaReferencias;
    }

    if (_botoesPause != null)
    {
        _botoesPause.alpha = 0f;
        _botoesPause.interactable = false;
        _botoesPause.blocksRaycasts = false;
    }

    if (_fundoPreto != null)
    {
        _fundoPreto.alpha = 0f;
        _fundoPreto.interactable = false;
        _fundoPreto.blocksRaycasts = false;
    }

    // --------------------------------------------------
    // SEQUENCE
    // --------------------------------------------------

    _pauseSequence = DOTween.Sequence()
        .SetUpdate(true);

    // Painel entra pela esquerda
    _pauseSequence.Append(
        _painelPause
            .DOLocalMove(_posicaoOriginal, _duracaEntrada)
            .SetEase(Ease.OutCubic)
    );

    // Fundo escuro aparece
    if (_fundoPreto != null)
    {
        _pauseSequence.Join(
            _fundoPreto
                .DOFade(1f, _duracaEntrada)
                .SetEase(Ease.OutQuad)
        );
    }

    // Referências sobem
    if (_referenciasPause != null)
    {
        _pauseSequence.Join(
            _referenciasPause
                .DOLocalMove(_posicaoReferencias, 0.3f)
                .SetEase(Ease.OutCubic)
        );
    }

    // Botões aparecem
    if (_botoesPause != null)
    {
        _pauseSequence.Append(
            _botoesPause
                .DOFade(1f, 0.2f)
                .SetEase(Ease.OutQuad)
        );

        _pauseSequence.AppendCallback(() =>
        {
            if (!_pauseAberto)
                return;

            _botoesPause.interactable = true;
            _botoesPause.blocksRaycasts = true;
        });
    }

    // Fundo recebe raycast
    if (_fundoPreto != null)
    {
        _pauseSequence.AppendCallback(() =>
        {
            if (!_pauseAberto)
                return;

            _fundoPreto.interactable = true;
            _fundoPreto.blocksRaycasts = true;
        });
    }

    _pauseSequence.OnComplete(() =>
    {
        _pauseSequence = null;
    });
  }

  private void FecharPause()
  {
    if (_painelPause == null)
        return;

    // --------------------------------------------------
    // BLOQUEIA INTERAÇÃO IMEDIATAMENTE
    // --------------------------------------------------

    if (_botoesPause != null)
    {
        _botoesPause.interactable = false;
        _botoesPause.blocksRaycasts = false;
    }

    if (_fundoPreto != null)
    {
        _fundoPreto.interactable = false;
        _fundoPreto.blocksRaycasts = false;
    }

    // --------------------------------------------------
    // SEQUENCE
    // --------------------------------------------------

    _pauseSequence = DOTween.Sequence()
        .SetUpdate(true);

    // Botões desaparecem
    if (_botoesPause != null)
    {
        _pauseSequence.Join(
            _botoesPause
                .DOFade(0f, 0.2f)
                .SetEase(Ease.OutQuad)
        );
    }

    // Referências descem
    if (_referenciasPause != null)
    {
        _pauseSequence.Join(
            _referenciasPause
                .DOLocalMove(
                    _posicaoReferencias +
                    Vector3.down * _distanciaReferencias,
                    0.3f
                )
                .SetEase(Ease.InCubic)
        );
    }

    // Fundo desaparece
    if (_fundoPreto != null)
    {
        _pauseSequence.Join(
            _fundoPreto
                .DOFade(0f, _duracaEntrada)
                .SetEase(Ease.OutQuad)
        );
    }

    // Depois de tudo, painel sai
    _pauseSequence.Append(
        _painelPause
            .DOLocalMove(
                _posicaoOriginal +
                Vector3.left * _distanciaEntrada,
                _duracaEntrada
            )
            .SetEase(Ease.InCubic)
    );

    _pauseSequence.OnComplete(() =>
    {
        // Estado final garantido
        if (_botoesPause != null)
        {
            _botoesPause.alpha = 0f;
            _botoesPause.interactable = false;
            _botoesPause.blocksRaycasts = false;
        }

        if (_fundoPreto != null)
        {
            _fundoPreto.alpha = 0f;
            _fundoPreto.interactable = false;
            _fundoPreto.blocksRaycasts = false;
        }

        if (_referenciasPause != null)
        {
            _referenciasPause.localPosition =
                _posicaoReferencias +
                Vector3.down * _distanciaReferencias;
        }

        _painelPause.localPosition =
            _posicaoOriginal +
            Vector3.left * _distanciaEntrada;

        _pauseSequence = null;

    
    GlobalEventBus.Instance.PauseAnimationFinished.Invoke();
    });
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
