using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StageIntroDirector : MonoBehaviour
{
  [Header("Canvas")]
  [SerializeField]
  private GameObject _root;

  [Header("Text")]
  [SerializeField]
  private Image _stageNumber;

  [SerializeField]
  private Image _stageTitle;

  [Header("backGroundBlack")]
  [SerializeField]
  private GameObject _backGroundBlack;

  [SerializeField]
  private float _backGroundHoldTime = 0.35f;

  [Header("Saída")]
  [SerializeField]
  private float _resumeLeadTime = 0.3f;

  [SerializeField]
  private MenuSlideIn[] slideObjects;

  public bool IsPlaying { get; private set; }

  private void Start()
  {
    StartCoroutine(PlayIntroOnStart());
  }

  private IEnumerator PlayIntroOnStart()
  {
    StageIntroData data = new();
    yield return Play(data);
  }

  public IEnumerator Play(StageIntroData data)
  {
    if (data == null)
    {
      Debug.LogError("StageIntroDirector: StageIntroData está nulo.", this);
      yield break;
    }

    if (_root == null)
    {
      Debug.LogError("StageIntroDirector: _root não foi atribuído no Inspector.", this);
      yield break;
    }

    if (_stageNumber == null)
    {
      Debug.LogError("StageIntroDirector: _stageNumber não foi atribuído no Inspector.", this);
      yield break;
    }

    if (_stageTitle == null)
    {
      Debug.LogError("StageIntroDirector: _stageTitle não foi atribuído no Inspector.", this);
      yield break;
    }

    IsPlaying = true;
    Time.timeScale = 0;

    if (_backGroundBlack != null)
      _backGroundBlack.SetActive(true);

    _stageNumber.sprite = data.StageNumberSprite;
    _stageTitle.sprite = data.StageTitleSprite;

    _root.SetActive(true);

    if (slideObjects != null)
    {
      foreach (var slide in slideObjects)
      {
        if (slide != null)
          slide.PlayEnterAnimation();
      }
    }

    yield return new WaitForSecondsRealtime(_backGroundHoldTime);

    if (_backGroundBlack != null)
      _backGroundBlack.SetActive(false);

    yield return new WaitForSecondsRealtime(data.WaitTime);

    float exitDuration = StartExitAnimations();
    float lead = Mathf.Clamp(_resumeLeadTime, 0f, exitDuration);

    yield return new WaitForSecondsRealtime(exitDuration - lead);

    Time.timeScale = 1;

    yield return new WaitForSecondsRealtime(lead);

    _root.SetActive(false);
    IsPlaying = false;
  }

  private float StartExitAnimations()
  {
    float longest = 0f;

    if (slideObjects == null)
      return longest;

    foreach (var slide in slideObjects)
    {
      if (slide == null)
        continue;

      Tween tween = slide.PlayExitAnimation();

      if (tween == null)
        continue;

      longest = Mathf.Max(longest, tween.Delay() + tween.Duration());
    }

    return longest;
  }
}
