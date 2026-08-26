
using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageIntroDirector : MonoBehaviour
{
    [Header("Canvas")]

    [SerializeField] private GameObject _root;

    [Header("Text")]

    [SerializeField] Image _stageNumber;

    [SerializeField] Image _stageTitle;

    [SerializeField] private MenuSlideIn[] slideObjects;

    public bool IsPlaying { get; private set; }

  [Header("Fundo Preto")]
  [SerializeField] private CanvasGroup _fundoPreto;
  [SerializeField] private float _duracaoFadeFundo = 0.5f;
  // Start is called once before the first execution of Update after the MonoBehaviour is created


  private void Awake()
  {
    if(_root != null)
      _root.SetActive(false);

    if(_fundoPreto != null)
    {
      _fundoPreto.alpha = 0f;
      _fundoPreto.interactable = false;
      _fundoPreto.blocksRaycasts = false;
      _fundoPreto.gameObject.SetActive(false);
    }

  }

  public IEnumerator Play(StageIntroData data)
  {
    IsPlaying = true;

    if(_fundoPreto != null)
    {
      _fundoPreto.gameObject.SetActive(true);
      _fundoPreto.alpha = 1;
      _fundoPreto.interactable = false;
      _fundoPreto.blocksRaycasts = true;
    }

    _root.SetActive(true);

    _stageNumber.sprite = data.StageTitleSprite;
    _stageTitle.sprite = data.StageNumberSprite;

    foreach (var slide in slideObjects)
      slide.PlayAnimation();

    yield return new WaitForSeconds(data.WaitTime);


    Tween lastTween = null;

    foreach (var slide in slideObjects)
      lastTween = slide.PlayExitAnimation();

    if(_fundoPreto != null)
    {
      _fundoPreto
        .DOFade(0f, _duracaoFadeFundo)
        .SetEase(Ease.OutQuad)
        .SetUpdate(true)
        .OnComplete(() =>
        {
          if (_fundoPreto != null)
          {
            _fundoPreto.interactable = false;
            _fundoPreto.blocksRaycasts = false;
            _fundoPreto.gameObject.SetActive(false);
          }
        });
    }

    if(lastTween != null)
      yield return lastTween.WaitForCompletion();

    yield return new WaitForSeconds(0.15f);

    _root.SetActive(false);

    IsPlaying = false;
  }

}
