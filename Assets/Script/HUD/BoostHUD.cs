using DG.Tweening;
using UnityEngine;

public class BoostHUD : BarHUD
{
  private bool _isShaking = false;
  private Tween _shakeTween;
  private Vector2 _sliderInitialAnchoredPos;

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
      _boundPlayer.DashSlashBoostButton.ChargingEv.RemoveListener(UpdateSlider);
      _boundPlayer.DashSlashBoostButton.StartedChargingEv.RemoveListener(StartShaking);
      _boundPlayer.DashSlashBoostButton.StoppedChargingEv.RemoveListener(StopShaking);
    }

    _boundPlayer = player;
    player.DashSlashBoostButton.ChargingEv.AddListener(UpdateSlider);
    player.DashSlashBoostButton.StartedChargingEv.AddListener(StartShaking);
    player.DashSlashBoostButton.StoppedChargingEv.AddListener(StopShaking);
  }

  protected override void UpdateSlider(float normalizedValue)
  {
    if (_slider == null)
      return;

    normalizedValue = Mathf.Clamp01(normalizedValue);

    if (!gameObject.activeInHierarchy)
      gameObject.SetActive(true);

    _slider.DOValue(normalizedValue, 0.35f).SetEase(Ease.OutQuad);
  }

  private void StartShaking()
  {
    if (_isShaking || _slider == null)
      return;

    _isShaking = true;

    ShakeLoop();
  }

  private void ShakeLoop()
  {
    if (!_isShaking || _slider == null)
      return;

    RectTransform sliderRect = _slider.GetComponent<RectTransform>();

    _shakeTween = sliderRect
      .DOShakeAnchorPos(
        duration: 0.4f,
        strength: new Vector2(2f, 2f),
        vibrato: 20,
        randomness: 45f,
        snapping: false,
        fadeOut: true
      )
      .OnComplete(ShakeLoop);
  }

  private void StopShaking()
  {
    if (!_isShaking || _slider == null)
      return;

    _isShaking = false;

    _shakeTween?.Kill();
    _shakeTween = null;

    // Reseta a posição do RectTransform após o shake
    _slider
      .GetComponent<RectTransform>()
      .DOAnchorPos(_sliderInitialAnchoredPos, 0.15f)
      .SetEase(Ease.OutQuad);
  }

  private void OnDestroy()
  {
    _shakeTween?.Kill();

    if (_boundPlayer != null)
    {
      _boundPlayer.DashSlashBoostButton.ChargingEv.RemoveListener(UpdateSlider);
      _boundPlayer.DashSlashBoostButton.StartedChargingEv.RemoveListener(StartShaking);
      _boundPlayer.DashSlashBoostButton.StoppedChargingEv.RemoveListener(StopShaking);
    }
  }
}
