
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public IEnumerator Play(StageIntroData data)
    {
        IsPlaying = true;

    _root.SetActive(true);

    _stageNumber.sprite = data.StageTitleSprite;
    _stageTitle.sprite = data.StageNumberSprite;

    foreach (var slide in slideObjects)
        slide.PlayAnimation();

    yield return new WaitForSeconds(data.WaitTime);

    Tween lastTween = null;

    foreach (var slide in slideObjects)
        lastTween = slide.PlayExitAnimation();

    if (lastTween != null)
        yield return lastTween.WaitForCompletion();

    yield return new WaitForSeconds(0.15f);

    _root.SetActive(false);

    IsPlaying = false;
    }

}
