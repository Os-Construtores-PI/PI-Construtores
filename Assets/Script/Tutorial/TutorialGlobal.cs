using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class TutorialGlobal : MonoBehaviour
{
  public static TutorialGlobal Instance { get; private set; }


  [Header("Tutorial UI")]
  [SerializeField]
  private GameObject tutorialHUD;

  [SerializeField]
  private RectTransform interfaceLeft;

  [SerializeField]
  private RectTransform interfaceRight;

  [SerializeField]
  private GameObject videoPanel;


  [Header("Video")]
  [SerializeField]
  private VideoPlayer videoPlayer;

  [Header("Animation")]

  [SerializeField]
  private float interfaceExitOffset = 100f;

  [SerializeField]
  private float interfaceDuration = 0.5f;

  [SerializeField]
  private float interfaceDelay = 0.15f;

  [SerializeField]
  private float videoDelay = 0.15f;

  [SerializeField]
  private float exitDuration = 0.4f;


  public bool IsTutorialActive {get; private set;}

  private Tween _currentTween;
  
  private Vector2 _leftFinalPosition;
  private Vector2 _rightFinalPosition;



  public event Action<bool> OnTutorialStateChanged;

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;

    // Guarda as posições originais
    if(interfaceLeft != null)
       _leftFinalPosition = interfaceLeft.anchoredPosition;
    
    if(interfaceRight != null)
       _rightFinalPosition = interfaceRight.anchoredPosition;

    PrepareTutorial();
       
  }

  private void Start()
  {
    if (tutorialHUD != null)
        tutorialHUD.SetActive(false);

    if(videoPanel != null)
       videoPanel.SetActive(false);
    
    StopVideo();
  }

  private void PrepareTutorial()
  {
    if(interfaceLeft != null)
    {
      interfaceLeft.anchoredPosition =
           new Vector2(
            CalculateHiddenPosition(interfaceLeft),
            _leftFinalPosition.y
           );
    }

    if(interfaceRight != null)
    {
      interfaceRight.anchoredPosition =
           new Vector2(
            CalculateHiddenPosition(interfaceRight),
            _rightFinalPosition.y
           );
    }

    if (videoPanel != null)
        videoPanel.SetActive(false);
  }

  private float CalculateHiddenPosition(RectTransform target)
  {
    if (target == null)
        return -2000f;

    float width =
      target.rect.width *
      target.localScale.x;

    if (target == interfaceLeft)
    {
      return _leftFinalPosition.x - width - interfaceExitOffset;
    }

    if (target == interfaceRight)
    {
      return _rightFinalPosition.x + width + interfaceExitOffset;
    }

    return target.anchoredPosition.x;
  }

  public void OpenTutorial(VideoClip video)
  {
    if(IsTutorialActive)
       return;

    if(video == null)
    {
      Debug.LogWarning(
        "[TutorialGlobal] nenhum VideoClip foi fornecido"
      );

      return;
    }

    IsTutorialActive = true;

    GameContext.IsTutorialActive = true;

    OnTutorialStateChanged?.Invoke(true);

    PrepareTutorial();

    if(tutorialHUD != null)
    {
      tutorialHUD.SetActive(true);
    }
       

    if(videoPanel != null)
    {
       videoPanel.SetActive(false);
    }

    PlayEntranceAnimation(video);
  }

  private void PlayEntranceAnimation(VideoClip video)
  {
    _currentTween?.Kill();

    Sequence sequence = DOTween.Sequence();

    if(interfaceLeft != null)
    {
      sequence.Append(
        interfaceLeft
            .DOAnchorPos(
              _leftFinalPosition,
              interfaceDuration
            )

          .SetEase(Ease.OutCubic)
      );
    }

    if(interfaceRight != null)
    {
      sequence.Join(
        interfaceRight.DOAnchorPos(_rightFinalPosition, interfaceDuration)
                       .SetEase(Ease.OutCubic)
     );
    }

    sequence.AppendInterval(videoDelay);

    sequence.AppendCallback(() =>
    {
      PlayVideo(video);
    });

    sequence.SetUpdate(true);

    _currentTween = sequence;
  }

  private void PlayVideo(VideoClip video)
  {
    if(videoPlayer == null)
    {
      Debug.LogError(
        "[TutorialGlobal] VideoPlayer não configurado"
      );

      CloseTutorial();

      return;
    }

    videoPlayer.Stop();

    videoPlayer.clip = video;

    videoPlayer.isLooping = false;

    videoPlayer.loopPointReached -= OnVideoFinished;
    videoPlayer.loopPointReached += OnVideoFinished;

    if(videoPanel != null)
       videoPanel.SetActive(true);

    videoPlayer.Play();
  }

  private void OnVideoFinished(VideoPlayer source)
  {
    CloseTutorial();
  }

  public void CloseTutorial()
  {
    if(!IsTutorialActive)
       return;

    _currentTween?.Kill();

    StopVideo();

    Sequence sequence = DOTween.Sequence();

    if (interfaceRight != null)
    {
      sequence.Append(
        interfaceRight
             .DOAnchorPosX(
              CalculateHiddenPosition(interfaceRight),
              exitDuration
             )
             .SetEase(Ease.InCubic)
      );
    }

    if(interfaceLeft != null)
    {
      sequence.Append(
        interfaceLeft
            .DOAnchorPosX(
                CalculateHiddenPosition(interfaceLeft),
                exitDuration
            )
            .SetEase(Ease.InCubic)
    );
    }

    sequence.AppendCallback(FinishTutorial);

    sequence.SetUpdate(true);

    _currentTween = sequence;
  }

  private void FinishTutorial()
  {
    _currentTween?.Kill();

    if(videoPanel != null)
       videoPanel.SetActive(false);

    if(tutorialHUD != null)
       tutorialHUD.SetActive(false);
    
    PrepareTutorial();

    IsTutorialActive = false;

    GameContext.IsTutorialActive = false;

    Time.timeScale = 1f;

    OnTutorialStateChanged?.Invoke(false);
  }

  private void StopVideo()
  {
    if(videoPlayer == null)
      return;

    videoPlayer.loopPointReached -= OnVideoFinished;

    videoPlayer.Stop();
  }

  public void SkipTutorial()
  {
    if(!IsTutorialActive)
       return;

    CloseTutorial();
  }

  private void OnDestroy()
  {
    _currentTween?.Kill();

    if(videoPlayer != null)
       videoPlayer.loopPointReached -= OnVideoFinished;

    if(Instance == this)
       Instance = null;
  }
}
