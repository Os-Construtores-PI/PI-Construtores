using TMPro;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class StageIntroDirector : MonoBehaviour
{
    [Header("Canvas")]

    [SerializeField] private GameObject _root;

    [Header("Text")]

    [SerializeField] TMP_Text _stageNumber;

    [SerializeField] TMP_Text _stageTitle;

    [SerializeField] private MenuSlideIn[] slideObjects;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Play(StageIntroData data)
    {
        StartCoroutine(PlayRoutine(data));
    }

    private IEnumerator PlayRoutine(StageIntroData data)
    {
        _root.SetActive(true);
        

        _stageNumber.text = data.StageNumber;
        _stageTitle.text = data.StageTitle;

        foreach (var slide in slideObjects)
        slide.PlayAnimation();

    yield return new WaitForSeconds(data.WaitTime);

    Tween lastTween = null;

    foreach (var slide in slideObjects)
        {
            lastTween = slide.PlayExitAnimation();
        }
    
    if(lastTween != null)
       yield return lastTween.WaitForCompletion();

    yield return new WaitForSeconds(0.15f); // duração da animação

    SceneManager.LoadScene(data.SceneName);
    }
}
