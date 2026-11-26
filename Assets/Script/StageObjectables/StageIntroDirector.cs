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

  [Header("Saída")]
  [SerializeField]
  private float _resumeLeadTime = 0.3f;

  [SerializeField]
  private MenuSlideIn[] slideObjects;

  public bool IsPlaying { get; private set; }

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

    _stageNumber.sprite = data.StageNumberSprite;
    _stageTitle.sprite = data.StageTitleSprite;

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
