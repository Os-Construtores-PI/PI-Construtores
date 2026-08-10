using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MenuSlideIn : MonoBehaviour
{
  [Header("Animação")]
  [SerializeField]
  private float startOffset = 700f;

  [SerializeField]
  private float duration = 0.8f;

  [SerializeField]
  private Ease ease = Ease.OutExpo;

  [SerializeField]
  private float startDelay = 0f;

  [SerializeField]
  private SlideDirection direction = SlideDirection.Right;

  [Header("Bounce")]
  [SerializeField]
  private bool bounce = true;

  [SerializeField]
  private float bounceScale = 1.05f;

  [SerializeField]
  private float bounceTime = 0.15f;

  private RectTransform rect;
  private Vector2 finalPosition;

  private void Awake()
  {
    rect = GetComponent<RectTransform>();
    finalPosition = rect.anchoredPosition;
  }

  private void OnEnable()
  {
    PlayAnimation();
  }

  public void PlayAnimation()
  {
    if (rect == null)
    {
      rect = GetComponent<RectTransform>();
      finalPosition = rect.anchoredPosition;
    }
    rect.DOKill();

    RectTransform canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();

    Vector2 offset = GetDirectionOffset(canvasRect);

    rect.anchoredPosition = finalPosition + offset;
    rect.localScale = Vector3.one;

    Sequence seq = DOTween.Sequence();

    seq.Append(rect.DOAnchorPos(finalPosition, duration).SetEase(ease).SetDelay(startDelay));

    if (bounce)
    {
      seq.Join(rect.DOScale(bounceScale, bounceTime).SetLoops(2, LoopType.Yoyo));
    }

    seq.SetLink(gameObject);
  }

  public Tween PlayExitAnimation()
  {
    if (rect == null)
    {
      rect = GetComponent<RectTransform>();
      finalPosition = rect.anchoredPosition;
    }

    rect.DOKill();
    RectTransform canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();

    Vector2 offset = GetDirectionOffset(canvasRect);

    rect.localScale = Vector3.one;

    Sequence seq = DOTween.Sequence();

    if (bounce)
    {
      seq.Append(rect.DOScale(bounceScale, bounceTime).SetLoops(2, LoopType.Yoyo));
    }

    seq.Append(rect.DOAnchorPos(finalPosition + offset, duration).SetEase(Ease.InExpo));

    seq.SetLink(gameObject);

    return seq;
  }

  private Vector2 GetDirectionOffset(RectTransform canvasRect)
  {
    switch (direction)
    {
      case SlideDirection.Right:
      {
        float screenOffset = canvasRect.rect.width + rect.rect.width;
        return Vector2.right * screenOffset;
      }
      case SlideDirection.Left:
      {
        float screenOffset = canvasRect.rect.width + rect.rect.width;
        return Vector2.left * screenOffset;
      }
      case SlideDirection.Up:
      {
        float screenOffset = canvasRect.rect.height + rect.rect.height;
        return Vector2.up * screenOffset;
      }
      case SlideDirection.Down:
      {
        float screenOffset = canvasRect.rect.height + rect.rect.height;
        return Vector2.down * screenOffset;
      }
      default:
        return Vector2.zero;
    }
  }

  public enum SlideDirection
  {
    Left,
    Right,
    Up,
    Down,
  }
}
