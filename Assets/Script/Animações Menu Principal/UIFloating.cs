using DG.Tweening;
using UnityEngine;


[RequireComponent(typeof(RectTransform))]
public class UIFloating : MonoBehaviour
{
    public enum MoveDirection
    {
        Horizontal,
        Vertical,
        Diagonal
    }

    [Header("Movemente")]
    [SerializeField] private MoveDirection direction = MoveDirection.Horizontal;
    [SerializeField] private float _distance = 30f;
    [SerializeField] private float _duration = 3f;
    [SerializeField] private Ease ease = Ease.InOutSine;

    [SerializeField] private float _entranceDelay = 0.8f;


    [Header("Random Start")]
    [SerializeField] private bool _randomStart = true;

    private RectTransform rect;
    private Vector2 startPosition;

    private Tween tween;

  private void Awake()
  {
    rect = GetComponent<RectTransform>();
    startPosition = rect.anchoredPosition;
  }

  private void OnEnable()
  {
    Invoke(nameof(Play), _entranceDelay);
  }

  public void Play()
    {
        tween?.Kill();

        startPosition = rect.anchoredPosition;

        Vector2 offset = Vector2.zero;

        switch (direction)
        {
            case MoveDirection.Horizontal:
            offset = Vector2.right * _distance;
            break;

            case MoveDirection.Vertical:
            offset = Vector2.up * _distance;
            break;

            case MoveDirection.Diagonal:
            offset = new Vector2(_distance, _distance);
            break;
        }

        rect.anchoredPosition = startPosition;

        tween = rect
            .DOAnchorPos(startPosition + offset, _duration)
            .SetEase(ease)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);

        
    }

  private void OnDisable()
  {
    tween?.Kill();
  }
}
