using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageIntroDirector : MonoBehaviour
{
  [Header("Canvas")]
  [SerializeField]
  private GameObject _root;

  [Header("Text")]
  [SerializeField]
  Image _stageNumber;

  [Header("backGroundBlack")]
  [SerializeField] GameObject _backGroundBlack;

  [SerializeField] private float _backGroundHoldTime = 0.35f;

  [SerializeField]
  Image _stageTitle;

  [SerializeField]
  private MenuSlideIn[] slideObjects;

  public bool IsPlaying { get; private set; }

  private void Start()
  {
    StartCoroutine(PlayIntroOnStart());
  }

  private IEnumerator PlayIntroOnStart()
  {
    yield return null;

    StageIntroData data = new StageIntroData();

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

    _stageNumber.sprite = data.StageTitleSprite;
    _stageTitle.sprite = data.StageNumberSprite;

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

    Tween lastTween = null;

    if (slideObjects != null)
    {
      foreach (var slide in slideObjects)
      {
        if (slide != null)
          lastTween = slide.PlayExitAnimation();
      }
    }

    if (lastTween != null)
      yield return lastTween.WaitForCompletion();

    yield return new WaitForSecondsRealtime(0.15f);

    _root.SetActive(false);

    Time.timeScale = 1;
    IsPlaying = false;
  }
}

