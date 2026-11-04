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
    }

    private void OnEnable()
    {
        StartSpin();
    }

    private void OnDisable()
    {
        StopSpin();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void StartSpin()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (rectTransform == null)
            return;

        StopSpin();

        rotationTween = rectTransform
            .DORotate(
                new Vector3(0f, -360f, 0f),
                rotationDuration,
                RotateMode.FastBeyond360
            )
            .SetEase(Ease.Linear)
            .SetLoops(-1);
    }

    private void StopSpin()
    {
        rotationTween?.Kill();
        rotationTween = null;
    }
}
