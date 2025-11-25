using DG.Tweening;
using UnityEngine;

public class ImageTriggerEvent : MonoBehaviour
{
    public CanvasGroup icon;
    public float _fadeDuration = 0.3f;

    public float _rotationSpeed = 90f; 

    private Tween _rotationTween;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    private void Update()
    {
        icon.alpha = 1f;

        StartSpin();
    }

    public void StartSpin()
    {
        _rotationTween?.Kill();

        _rotationTween = transform
            .DORotate(new Vector3(0f, 180f, 0f),4f, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .SetLoops(-1);
    }

    public void StopSpin()
    {
        _rotationTween?.Kill();
    }

    public void HideIcon()
    {
        StopSpin();
        icon.DOFade(0f, _fadeDuration).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    public void ShowIcon()
    {
        gameObject.SetActive(true);
        icon.alpha = 0f;

        icon.DOFade(1f, _fadeDuration);
        StartSpin();
    }


}
