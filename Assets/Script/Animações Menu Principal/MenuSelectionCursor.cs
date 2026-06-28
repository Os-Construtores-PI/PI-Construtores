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
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  private void Awake()
  {
    Instance = this;
  }

  public void MoveTo(Button button)
    {
        if (button == null)
        return;

    RectTransform target = button.GetComponent<RectTransform>();

    cursor.DOKill();
    idleTween?.Kill();

    Vector2 screenPoint =
        RectTransformUtility.WorldToScreenPoint(null, target.position);

    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        cursor.parent as RectTransform,
        screenPoint,
        null,
        out Vector2 localPoint);

    // mesma altura do botão
    cursor.sizeDelta = cursorSize;

    // posição
    cursor.DOAnchorPos(localPoint, moveDuration)
          .SetEase(moveEase)
          .OnComplete(StartIdle);

    cursor.DOScale(1.05f, 0.08f)
          .SetLoops(2, LoopType.Yoyo)
          .SetEase(Ease.OutQuad);
    }


    void StartIdle()
    {
        idleTween =
            cursor.DOAnchorPosX(
                cursor.anchoredPosition.x + idleDistance,
                idleSpeed)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
