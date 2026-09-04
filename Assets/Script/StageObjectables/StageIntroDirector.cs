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


  public IEnumerator Play(StageIntroData data)
  {
    IsPlaying = true;

    Time.timeScale = 0;

    // Garante que o fundo preto esteja ativo
    if (_backGroundBlack != null)
      _backGroundBlack.SetActive(true);


    // Configura os dados da fase
    _stageNumber.sprite = data.StageTitleSprite;
    _stageTitle.sprite = data.StageNumberSprite;

    // Ativa a interface da intro
    _root.SetActive(true);
    

    foreach (var slide in slideObjects)
      slide.PlayEnterAnimation();

    yield return new WaitForSecondsRealtime(_backGroundHoldTime);

    if(_backGroundBlack != null)
      _backGroundBlack.SetActive(false);



    yield return new WaitForSecondsRealtime(data.WaitTime);

    Tween lastTween = null;

    foreach (var slide in slideObjects)
      lastTween = slide.PlayExitAnimation();

    if (lastTween != null)
      yield return lastTween.WaitForCompletion();

    yield return new WaitForSecondsRealtime(0.15f);

    _root.SetActive(false);

    Time.timeScale = 1;

    IsPlaying = false;
  }
}
