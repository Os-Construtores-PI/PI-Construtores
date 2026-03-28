using DG.Tweening;
using UnityEngine;

public class ImageTriggerEvent : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationDuration = 4f;

    private RectTransform rectTransform;
    private Tween rotationTween;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ShowImmediate();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        StartSpin();
    }

    public void Hide()
    {
        StopSpin();
        gameObject.SetActive(false);
    }

    private void StartSpin()
    {
        StopSpin();

        rotationTween = rectTransform
            .DORotate(new Vector3(0f, -360f, 0), rotationDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear)
            .SetLoops(-1);
    }

    private void StopSpin()
    {
        rotationTween?.Kill();
        rotationTween = null;
    }

    private void ShowImmediate()
    {
        StartSpin();
        gameObject.SetActive(true);
    }
}
