using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BoostHUD : BarHUD
{
  [Header("Fill Options")]
  [SerializeField]
  private Image _fill;
  private Tween _shakeTween;
  private Vector2 _sliderInitialAnchoredPos;
  private bool _isShaking = false;
  private bool _isGlowing = false;

  protected override void Awake()
  {
    base.Awake();
    if (_slider != null)
      _sliderInitialAnchoredPos = _slider.GetComponent<RectTransform>().anchoredPosition;
  }

  public override void BindToPlayer(Player player)
  {
    if (player == null)
      return;

    if (_boundPlayer != null)
    {
      _boundPlayer.BoostChanged.RemoveListener(UpdateSlider);
    }

    _boundPlayer = player;
    _boundPlayer.BoostChanged.AddListener(UpdateSlider);
    _slider.DOValue(player.BoostValue, 0.35f).SetEase(Ease.OutQuad);
  }

  protected override void UpdateSlider(float normalizedValue)
  {
    if (_slider == null)
      return;

    normalizedValue = Mathf.Clamp01(normalizedValue);

    if (!gameObject.activeInHierarchy)
      gameObject.SetActive(true);

    if (!_isGlowing && normalizedValue >= _slider.value)
    {
      _isGlowing = true;

      _fill.DOKill();
      _fill
        .DOColor(Color.lightBlue, 0.25f)
        .SetLoops(2, LoopType.Yoyo)
        .OnComplete(() => _isGlowing = false);
    }
    _slider.DOValue(normalizedValue, 0.35f).SetEase(Ease.OutQuad);
  }

  private void OnDestroy()
  {
    _shakeTween?.Kill();

    if (_boundPlayer != null)
    {
      _boundPlayer.BoostChanged.RemoveListener(UpdateSlider);
    }
  }
}
