using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialGlobal : MonoBehaviour
{
  public static TutorialGlobal Instance { get; private set; }

  [Header("Audio")]
  [SerializeField]
  private TutorialAudioConfig _tutorialAudioConfig;

  [Header("UI")]
  [SerializeField]
  private GameObject tutorialHUD;

  [Header("Tutorials")]
  [SerializeField]
  private GameObject movementTutorial;

  [SerializeField]
  private GameObject dashTutorial;

  public event System.Action<bool> OnTutorialStateChanged;

  public bool IsTutorialActive { get; private set; }

  private Tween _currentTween;

  private void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DeactivateAll();
  }

  private void Start()
  {
    tutorialHUD.SetActive(false);
  }

  public void OpenTutorial(TutorialTrigger.TutorialType type)
  {
    if (IsTutorialActive)
      return;

    IsTutorialActive = true;
    GameContext.IsTutorialActive = true;

    if (AudioManager.Instance != null && _tutorialAudioConfig != null)
    {
      AudioManager.Instance.PlaySFX(_tutorialAudioConfig.TutorialOpen);
    }

    DeviceInputManager.Instance?.ForceRefresh();

    DeactivateAll();
    tutorialHUD.SetActive(true);

    GameObject panel = GetPanel(type);
    if (panel != null)
    {
      AnimateEntrance(panel);
    }

    OnTutorialStateChanged?.Invoke(true);
  }

  public void CloseTutorial()
  {
    if (!IsTutorialActive)
      return;

    IsTutorialActive = false;
    GameContext.IsTutorialActive = false;

    if (AudioManager.Instance != null && _tutorialAudioConfig != null)
    {
      AudioManager.Instance.PlaySFX(_tutorialAudioConfig.TutorialClose);
    }

    GameObject activePanel = GetActivePanel();
    if (activePanel != null)
    {
      AnimateExit(activePanel);
    }
    else
    {
      tutorialHUD.SetActive(false);
      Time.timeScale = 1f;
    }

    DeviceInputManager.Instance?.ForceRefresh();
    OnTutorialStateChanged?.Invoke(false);
  }

  private void AnimateEntrance(GameObject panel)
  {
    _currentTween?.Kill();

    Time.timeScale = 0f;
    panel.SetActive(true);

    CanvasGroup cg = panel.GetComponent<CanvasGroup>();
    RectTransform rt = panel.GetComponent<RectTransform>();

    cg.alpha = 0f;
    rt.localScale = Vector3.one * 0.9f;

    _currentTween = DOTween
      .Sequence()
      .Append(cg.DOFade(1f, 0.25f))
      .Join(rt.DOScale(1f, 0.25f))
      .SetEase(Ease.OutBack)
      .SetUpdate(UpdateType.Normal, true);
  }

  private void AnimateExit(GameObject panel)
  {
    _currentTween?.Kill();

    CanvasGroup cg = panel.GetComponent<CanvasGroup>();
    RectTransform rt = panel.GetComponent<RectTransform>();

    _currentTween = DOTween
      .Sequence()
      .Append(cg.DOFade(0f, 0.2f))
      .Join(rt.DOScale(0.9f, 0.2f))
      .SetEase(Ease.InBack)
      .SetUpdate(UpdateType.Normal, true)
      .OnComplete(() =>
      {
        panel.SetActive(false);
        tutorialHUD.SetActive(false);
        Time.timeScale = 1f;
      });
  }

  private void DeactivateAll()
  {
    if (movementTutorial != null)
      movementTutorial.SetActive(false);
    if (dashTutorial != null)
      dashTutorial.SetActive(false);
  }

  private GameObject GetPanel(TutorialTrigger.TutorialType type)
  {
    return type switch
    {
      TutorialTrigger.TutorialType.Movement => movementTutorial,
      TutorialTrigger.TutorialType.Dash => dashTutorial,
      _ => null,
    };
  }

  private GameObject GetActivePanel()
  {
    if (movementTutorial != null && movementTutorial.activeSelf)
      return movementTutorial;
    if (dashTutorial != null && dashTutorial.activeSelf)
      return dashTutorial;

    return null;
  }
}
