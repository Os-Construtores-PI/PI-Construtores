using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MenuSlideIn : MonoBehaviour
{
  [Header("Anima��o")]
  [SerializeField] private float startOffset = 700f;
  [SerializeField] private float duration = 0.8f;
  [SerializeField] private Ease ease = Ease.OutExpo;

  [SerializeField] private float startDelay = 0f;

  [SerializeField]
  private SlideDirection direction = SlideDirection.Right;

  [Header("Bounce")]
  [SerializeField] private bool bounce = true;
  [SerializeField] private float bounceScale = 1.05f;
  [SerializeField] private float bounceTime = 0.15f;

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
    if(rect == null)
    {
      rect = GetComponent<RectTransform>();
      finalPosition = rect.anchoredPosition;

    }
    rect.DOKill();

    RectTransform canvasRect =
      GetComponentInParent<Canvas>().GetComponent<RectTransform>();


    // Coloca completa fora da tela pela direira
    float screenOffset =
      canvasRect.rect.width + rect.rect.width;

    Vector2 offset = direction == SlideDirection.Right
      ? Vector2.right
      : Vector2.left;

    rect.anchoredPosition =
      finalPosition + offset * screenOffset;

    rect.localScale = Vector3.one;

    Sequence seq = DOTween.Sequence();

    seq.Append(
      rect.DOAnchorPos(finalPosition, duration)
      .SetEase(ease)
      .SetDelay(startDelay));
      

    if (bounce)
    {
      seq.Join(
        rect.DOScale(bounceScale, bounceTime)
        .SetLoops(2, LoopType.Yoyo));
    }

    seq.SetLink(gameObject);
  }

  public Tween PlayExitAnimation()
  {
    if(rect == null)
    {
      rect = GetComponent<RectTransform>();
      finalPosition = rect.anchoredPosition;
    }

    rect.DOKill();
    RectTransform canvasRect = 
       GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    
    float screenOffset =
      canvasRect.rect.width + rect.rect.width;
    
    Vector2 offset = direction == SlideDirection.Right
       ? Vector2.right
       : Vector2.left;
    
    rect.localScale = Vector3.one;

    Sequence seq = DOTween.Sequence();

    if (bounce)
    {
      seq.Append(
        rect.DOScale(bounceScale, bounceTime)
            .SetLoops(2,LoopType.Yoyo)
      );
    }

    seq.Append(
      rect.DOAnchorPos(
        finalPosition + offset * screenOffset,
        duration
      )
      .SetEase(Ease.InExpo)
    );

    seq.SetLink(gameObject);

    return seq;
  }

  public enum SlideDirection
  {
    Left,
    Right
  }
}
