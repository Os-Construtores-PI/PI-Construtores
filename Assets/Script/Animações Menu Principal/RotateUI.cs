using DG.Tweening;
using UnityEngine;

public class RotateUI : MonoBehaviour
{
  [Header("Rotation")]
  [SerializeField] private float rotationSpeed = 30f;
  [SerializeField] private bool clockWise = true;

  [Header("Floating")]
  [SerializeField] private bool floating = false;
  [SerializeField] private float floatDistance = 8f;
  [SerializeField] private float floatDuration = 2f;

  private RectTransform rect;
  private Vector2 startPos;

  private Tween floatTween;


  private void Awake()
  {
    rect = GetComponent<RectTransform>();
    startPos = rect.anchoredPosition;
  }

  private void OnEnable()
  {
    if (floating)
    {
      floatTween = rect
        .DOAnchorPosY(startPos.y + floatDistance, floatDuration)
        .SetLoops(-1, LoopType.Yoyo)
        .SetEase(Ease.InOutSine)
        .SetLink(gameObject);
    }
  }

  private void Update()
  {
    float dir = clockWise ? -1f : 1f;

    rect.Rotate(0f, 0f, rotationSpeed * dir * Time.deltaTime);
  }

  private void OnDisable()
  {
    floatTween?.Kill();
  }
}
