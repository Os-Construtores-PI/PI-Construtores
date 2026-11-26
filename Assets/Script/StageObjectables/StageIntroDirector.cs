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
  [SerializeField, Min(0f)]
  private float _gameRunTime = 0.3f;

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

    if (_root == null || _stageNumber == null || _stageTitle == null)
    {
      Debug.LogError("StageIntroDirector: referências do Inspector não atribuídas.", this);
      yield break;
    }

    IsPlaying = true;
    Time.timeScale = 0f;

    _stageNumber.sprite = data.StageNumberSprite;
    _stageTitle.sprite = data.StageTitleSprite;

    yield return new WaitForSecondsRealtime(data.WaitTime);

    float exitDuration = StartExitAnimations();
    float runTime = Mathf.Min(_gameRunTime, exitDuration);

    yield return new WaitForSecondsRealtime(exitDuration - runTime);

    Time.timeScale = 1f;

    yield return new WaitForSecondsRealtime(runTime);

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
