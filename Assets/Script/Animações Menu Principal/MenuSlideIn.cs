using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MenuSlideIn : MonoBehaviour
{
  [Header("Animação")]
  [SerializeField]
  private float startOffset = 0f;

  [SerializeField]
  private float duration = 0.8f;

  [SerializeField]
  private Ease enterEase = Ease.OutExpo;

  [SerializeField]
  private Ease exitEase = Ease.InExpo;

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

  [Header("Submenus")]
  [SerializeField]
  private bool animateChildMenus = true;

  [SerializeField]
  private float childStagger = 0.08f;

  [SerializeField]
  private bool controlledByParent = false;

  private RectTransform rect;
  private RectTransform canvasRect;
  private Vector2 finalPosition;
  private Vector3 originalScale;
  private bool initialized;
  private readonly List<MenuSlideIn> childMenus = new List<MenuSlideIn>();

  private void Awake()
  {
    Initialize();
  }

  private void OnEnable()
  {
    if (!controlledByParent)
    {
      PlayEnterAnimation();
    }
  }

  private void Initialize()
  {
    if (initialized)
    {
      return;
    }

    rect = GetComponent<RectTransform>();
    finalPosition = rect.anchoredPosition;
    originalScale = rect.localScale;

    Canvas canvas = GetComponentInParent<Canvas>();
    canvasRect =
      canvas != null
        ? canvas.rootCanvas.GetComponent<RectTransform>()
        : rect.parent as RectTransform;

    initialized = true;
  }

  public Tween PlayEnterAnimation()
  {
    Initialize();
    rect.DOKill();

    rect.anchoredPosition = finalPosition + GetDirectionOffset();
    rect.localScale = originalScale;

    Sequence seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

    seq.AppendInterval(startDelay);
    seq.Append(rect.DOAnchorPos(finalPosition, duration).SetEase(enterEase));

    if (bounce)
    {
      seq.Append(rect.DOScale(originalScale * bounceScale, bounceTime).SetLoops(2, LoopType.Yoyo));
    }

    CollectChildMenus();

    for (int i = 0; i < childMenus.Count; i++)
    {
      seq.Insert(startDelay + (i + 1) * childStagger, childMenus[i].PlayEnterAnimation());
    }

    return seq;
  }

  public Tween PlayExitAnimation()
  {
    Initialize();
    rect.DOKill();

    rect.localScale = originalScale;

    Sequence seq = DOTween.Sequence().SetUpdate(true).SetLink(gameObject);

    if (bounce)
    {
      seq.Append(rect.DOScale(originalScale * bounceScale, bounceTime).SetLoops(2, LoopType.Yoyo));
    }

    seq.Append(rect.DOAnchorPos(finalPosition + GetDirectionOffset(), duration).SetEase(exitEase));

    CollectChildMenus();

    for (int i = 0; i < childMenus.Count; i++)
    {
      seq.Insert(i * childStagger, childMenus[i].PlayExitAnimation());
    }

    return seq;
  }

  public bool TryFindParentMenu(out MenuSlideIn parentMenu)
  {
    return transform.TryFindInParents(out parentMenu);
  }

  private void CollectChildMenus()
  {
    childMenus.Clear();

    if (!animateChildMenus)
    {
      return;
    }

    transform.FindInChildren(childMenus, includeInactive: false, stopAtMatch: true);

    for (int i = childMenus.Count - 1; i >= 0; i--)
    {
      if (!childMenus[i].controlledByParent)
      {
        childMenus.RemoveAt(i);
      }
    }
  }

  private Vector2 GetDirectionOffset()
  {
    bool horizontal = direction == SlideDirection.Left || direction == SlideDirection.Right;
    float sign = direction == SlideDirection.Right || direction == SlideDirection.Up ? 1f : -1f;

    float distance = horizontal
      ? canvasRect.rect.width + rect.rect.width
      : canvasRect.rect.height + rect.rect.height;

    Vector2 axis = horizontal ? Vector2.right : Vector2.up;

    return axis * (sign * (distance + startOffset));
  }

  public enum SlideDirection
  {
    Left,
    Right,
    Up,
    Down,
  }
}
