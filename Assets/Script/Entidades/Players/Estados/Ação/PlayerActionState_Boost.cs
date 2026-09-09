using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class PlayerActionStateBoost : IPlayerState<Player>
{
  #region IState
  public PlayerActionType Type => PlayerActionType.Boost;
  public HashSet<PlayerActionType> IncompatibleActions => _incompatibleActions;
  private readonly HashSet<PlayerActionType> _incompatibleActions = new();
  #endregion

  #region Fields

  [Header("Camera")]
  [SerializeField]
  private float _defaultFOV = 80f;

  [SerializeField]
  private float _boostFOV = 120f;

  [SerializeField]
  private float _fovTransitionDuration = 0.3f;

  private Tween _fovTween;

  [Header("Enter Effects")]
  [SerializeField]
  private float _enterShakeAmplitude = 1.5f;

  [SerializeField]
  private float _enterShakeFrequency = 0.4f;

  [SerializeField]
  private float _enterShakeDuration = 0.25f;

  [Header("Boost Settings")]
  [SerializeField]
  private float _continuousBoostUsage = 10f;

  [SerializeField]
  private float _maxVelocity = 100f;

  [Header("Boost Ramp (início e fim gradual)")]
  [SerializeField]
  private float _rampInDuration = 1.5f;

  [SerializeField]
  private float _rampOutDuration = 3f;

  [SerializeField]
  private Ease _rampInEase = Ease.InQuad;

  [SerializeField]
  private Ease _rampOutEase = Ease.OutQuad;

  private Tween _speedRampTween;
  private Tween _gravityRampTween;
  private float _currentBoostSpeedRatio = 1f;

  [Header("Vibração do Gamepad na Corrida")]
  [SerializeField]
  private float _runRumbleLowFrequency = 0.1f;

  [SerializeField]
  private float _runRumbleHighFrequency = 0.2f;

  [Header("Vibração do Gamepad no Acerto")]
  [SerializeField]
  private float _hitRumbleLowFrequency = 0.5f;

  [SerializeField]
  private float _hitRumbleHighFrequency = 0.8f;

  [SerializeField]
  private float _hitRumbleDuration = 0.2f;

  [Header("Boost Componentes")]
  [SerializeField]
  private Collider _boostHitboxCollider;

  [SerializeField]
  private HitboxComponent _boostHitboxComponent;

  [SerializeField]
  private SphereCollider _boostCollectionCollider;

  private CancellationTokenSource _hitRumbleCts;

  private float _boostSpeedRatio;
  private string _boostSourceId;

  #endregion

  #region IState Callbacks
  public void Enter(Player player)
  {
    if (player.Stats.TryGetBaseNum(StatType.Speed, out float baseSpeed))
    {
      _boostSpeedRatio = _maxVelocity / baseSpeed;
    }
    else
    {
      _boostSpeedRatio = _maxVelocity / player.Speed;
    }

    player.SpeedLines.Invoke(true);

    if (_boostCollectionCollider != null)
    {
      _boostCollectionCollider.enabled = true;
    }

    if (player.HurtboxCollider != null)
    {
      player.HurtboxCollider.OverrideDamageLayers(LayerMask.GetMask("WorldHit"));
    }

    if (_boostHitboxCollider != null)
    {
      _boostHitboxCollider.enabled = true;
    }

    var hitbox = _boostHitboxCollider.GetComponent<HitboxComponent>();
    hitbox?.Hit.AddListener(OnBoostHitDetected);

    player.CustomShake.Invoke(
      player.ID,
      _enterShakeAmplitude,
      _enterShakeFrequency,
      _enterShakeDuration
    );

    player.TrailsSystem.PlayEffect(TrailType.MovementTrail);
    player.TrailsSystem.PlayEffect(TrailType.MovementSupport1Trail);
    player.TrailsSystem.PlayEffect(TrailType.MovementSupport2Trail);
    player.TrailsSystem.PlayEffect(TrailType.MovementSupport2Trail);

    Gamepad.current?.SetMotorSpeeds(_runRumbleLowFrequency, _runRumbleHighFrequency);

    _speedRampTween?.Kill();

    if (!string.IsNullOrEmpty(_boostSourceId))
    {
      player.Stats.RemoveMultiplier(StatType.Speed, _boostSourceId);
      _boostSourceId = null;
    }

    _currentBoostSpeedRatio = 1f;
    _boostSourceId = player.Stats.ApplyMultiplier(StatType.Speed, _currentBoostSpeedRatio);

    _speedRampTween = DOTween
      .To(
        () => _currentBoostSpeedRatio,
        ratio =>
        {
          _currentBoostSpeedRatio = ratio;

          if (!string.IsNullOrEmpty(_boostSourceId))
          {
            player.Stats.RemoveMultiplier(StatType.Speed, _boostSourceId);
          }

          _boostSourceId = player.Stats.ApplyMultiplier(StatType.Speed, ratio);
        },
        _boostSpeedRatio,
        _rampInDuration
      )
      .SetEase(_rampInEase);

    _gravityRampTween?.Kill();
    _gravityRampTween = DOTween
      .To(() => player.GravityValue, g => player.GravityValue = g, -100f, _rampInDuration)
      .SetEase(_rampInEase);

    _fovTween?.Kill();
    _fovTween = DOTween.To(
      () => player.MainCamera.Lens.FieldOfView,
      fov =>
      {
        var lens = player.MainCamera.Lens;
        lens.FieldOfView = fov;
        player.MainCamera.Lens = lens;
      },
      _boostFOV,
      _fovTransitionDuration
    );
  }

  public void Exit(Player player)
  {
    _hitRumbleCts?.Cancel();
    _hitRumbleCts?.Dispose();
    _hitRumbleCts = null;

    Gamepad.current?.SetMotorSpeeds(0, 0);

    player.SpeedLines.Invoke(false);

    if (_boostCollectionCollider != null)
    {
      _boostCollectionCollider.enabled = false;
    }

    if (player.HurtboxCollider != null)
    {
      player.HurtboxCollider.ResetDamageLayers();
    }

    if (_boostHitboxCollider != null)
    {
      _boostHitboxCollider.enabled = false;
    }

    player.TrailsSystem.StopEffect(TrailType.MovementTrail);
    player.TrailsSystem.StopEffect(TrailType.MovementSupport1Trail);
    player.TrailsSystem.StopEffect(TrailType.MovementSupport2Trail);

    _speedRampTween?.Kill();
    _speedRampTween = DOTween
      .To(
        () => _currentBoostSpeedRatio,
        ratio =>
        {
          _currentBoostSpeedRatio = ratio;

          if (!string.IsNullOrEmpty(_boostSourceId))
          {
            player.Stats.RemoveMultiplier(StatType.Speed, _boostSourceId);
            _boostSourceId = null;
          }

          if (ratio > 1f + 0.001f)
          {
            _boostSourceId = player.Stats.ApplyMultiplier(StatType.Speed, ratio);
          }
        },
        1f,
        _rampOutDuration
      )
      .SetEase(_rampOutEase);

    _gravityRampTween?.Kill();
    _gravityRampTween = DOTween
      .To(
        () => player.GravityValue,
        g => player.GravityValue = g,
        player.InitialGravityValue,
        _rampOutDuration
      )
      .SetEase(_rampOutEase);

    // --- FOV de volta ao padrão ---
    _fovTween?.Kill();
    _fovTween = DOTween.To(
      () => player.MainCamera.Lens.FieldOfView,
      fov =>
      {
        var lens = player.MainCamera.Lens;
        lens.FieldOfView = fov;
        player.MainCamera.Lens = lens;
      },
      _defaultFOV,
      _fovTransitionDuration
    );
  }

  public void Update(Player player)
  {
    player.BoostValue -= _continuousBoostUsage * Time.deltaTime;
    player.BoostValue = Mathf.Max(0f, player.BoostValue);

    if (player.BoostValue <= 0f)
    {
      player.ActionLayer.ExitState(this, player);
      return;
    }

    if (!player.PlayerInput.actions.FindAction("Dash / Boost").IsPressed())
    {
      player.ActionLayer.ExitState(this, player);
      return;
    }
  }

  public void FixedUpdate(Player player) { }

  private void OnBoostHitDetected()
  {
    TriggerHitRumbleAsync();
  }

  private async void TriggerHitRumbleAsync()
  {
    _hitRumbleCts?.Cancel();
    _hitRumbleCts?.Dispose();
    _hitRumbleCts = new CancellationTokenSource();

    var token = _hitRumbleCts.Token;

    try
    {
      Gamepad.current?.SetMotorSpeeds(_hitRumbleLowFrequency, _hitRumbleHighFrequency);

      await System.Threading.Tasks.Task.Delay(
        System.TimeSpan.FromSeconds(_hitRumbleDuration),
        token
      );

      Gamepad.current?.SetMotorSpeeds(_runRumbleLowFrequency, _runRumbleHighFrequency);
    }
    catch (System.OperationCanceledException) { }
  }

  #endregion
}
