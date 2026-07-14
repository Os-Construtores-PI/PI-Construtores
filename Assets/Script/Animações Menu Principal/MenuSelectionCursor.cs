using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MenuSelectionCursor : MonoBehaviour
{
    public static MenuSelectionCursor Instance;

    [Header("Cursor")]
    [SerializeField] private RectTransform cursor;

    [SerializeField] private Vector2 cursorSize = new Vector2(1022.5f, 183f);

    [SerializeField] private Vector2 offset =
      new Vector2 (-90f, 0f);

    
    [Header("Animation")]
    [SerializeField] private float moveDuration = .22f;

    [SerializeField] private Ease moveEase =
       Ease.OutExpo;

    
    [Header("Idle")]
    [SerializeField] private float idleDistance = 8f;
    [SerializeField] private float idleSpeed = .45f;

    private Tween idleTween;

  [Header("Sprites")]
  [SerializeField] private Image cursorImage;

  [SerializeField] private Sprite _normalImage;
  [SerializeField] private Sprite _pressedSprite;

  public bool CanMove { get; set; } = false;
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  private void Awake()
  {
    Instance = this;

    cursorImage.sprite = _normalImage;
  }

  public void SetNormal()
  {
    cursorImage.sprite = _normalImage;
  }

  public void SetPressed(float delay)
  {
    cursorImage.sprite = _pressedSprite;

    CancelInvoke(nameof(SetNormal));

    Invoke(nameof(SetNormal), delay);
  }

  private void OnDisable()
    {
        cursor?.DOKill();
        idleTween?.Kill();
    }

    private void OnDestroy()
    {
        cursor?.DOKill();
        idleTween?.Kill();
    }
  public void MoveTo(Button button, bool instant = false)
    {

        InternalMove(button, instant);

    }

    public void Hide()
    {
        Debug.Log("HIDE CURSOR");
        cursor.DOKill();
        idleTween?.Kill();
        
        cursor.gameObject.SetActive(false);
    }

    public void Shoow(Button button)
    {
        cursor.gameObject.SetActive(true);
        MoveTo(button, true);
    }


    void StartIdle()
    {

        if (cursor == null)
        return;

    idleTween?.Kill();


        idleTween =
            cursor.DOAnchorPosX(
                cursor.anchoredPosition.x + idleDistance,
                idleSpeed)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }


    public void ShowAfterAnimation(Button button)
    {
       // CanMove = true;

       Debug.Log("SHOW CURSOR");


        cursor.gameObject.SetActive(true);
        InternalMove(button, true);
        
    }

    private void InternalMove(Button button, bool instant)
    {

        if (button == null)
        return;

       SetNormal();
    RectTransform target = button.GetComponent<RectTransform>();

    cursor.DOKill();
    idleTween?.Kill();
    idleTween = null;

    Vector2 screenPoint =
        RectTransformUtility.WorldToScreenPoint(null, target.position);

    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        cursor.parent as RectTransform,
        screenPoint,
        null,
        out Vector2 localPoint);

    cursor.sizeDelta = cursorSize;

    Vector2 targetPos = localPoint + offset;

        if (instant)
        {
            cursor.anchoredPosition = targetPos;
            StartIdle();
        }
        else
        {
            cursor.DOAnchorPos(targetPos, moveDuration)
                  .SetEase(moveEase)
                  .OnComplete(StartIdle);
        }

    cursor.DOScale(1.05f, 0.08f)
          .SetLoops(2, LoopType.Yoyo)
          .SetEase(Ease.OutQuad);
    }
}
