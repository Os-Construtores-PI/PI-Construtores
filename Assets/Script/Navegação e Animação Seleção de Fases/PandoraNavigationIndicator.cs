using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PandoraNavigationIndicator : MonoBehaviour
{
    public static PandoraNavigationIndicator Instance { get; private set; }

  [Header("Referencias")]
  [SerializeField] private RectTransform _pandora;

  [Header("Configuração")]
  [SerializeField] private Vector2 _offset = new Vector2(0f, 110f);
  [SerializeField] private float _moveDuration = 0.25f;
  [SerializeField] private Ease _moveEase = Ease.OutCubic;

  private Canvas _canvas;

  private void Awake()
  {
    Instance = this;

    if (_pandora == null)
        _pandora = GetComponent<RectTransform>();

    _canvas = GetComponent<Canvas>();
  }

  public void MoveTo(Button butto, bool instant = false)
  {
    if (butto == null || _pandora == null)
      return;

    RectTransform target = butto.GetComponent<RectTransform>();

    if (target == null)
      return;

    Camera canvasCamera = null;

    if (_canvas != null &&
      _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
    {
      canvasCamera = _canvas.worldCamera;
    }

    Vector2 screenPoint =
      RectTransformUtility.WorldToScreenPoint(
        canvasCamera,
        target.position);

    RectTransform parent =
      _pandora.parent as RectTransform;

    if (parent == null)
      return;

    RectTransformUtility.ScreenPointToLocalPointInRectangle(
      parent,
      screenPoint,
      canvasCamera,
      out Vector2 localPosition);

    Vector2 destination = localPosition + _offset;

    _pandora.DOKill();

    if (instant)
    {
      _pandora.anchoredPosition = destination;
      return;
    }

    _pandora.DOAnchorPos(destination, _moveDuration)
      .SetEase(_moveEase)
      .SetUpdate(true);
  }

  private void OnDestroy()
  {
    _pandora?.DOKill();

    if (Instance == this)
       Instance = null;
  }
}
