using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerActionStateBoost : IState<Player>
{
  #region IState
  public ActionType Type => ActionType.Boost;
  public HashSet<ActionType> IncompatibleActions => _incompatibleActions;
  private readonly HashSet<ActionType> _incompatibleActions = new();
  #endregion

  #region Fields
  private const int MainCameraPriority = 10;
  private const int BoostCameraPriority = 20;
  private const int InactivePriority = 0;

  [Header("Camera")]
  [SerializeField]
  private float _defaultFOV = 80f;

  [SerializeField]
  private float _boostFOV = 120f;

  [Header("Enter Effects")]
  [SerializeField]
  private float _enterShakeAmplitude = 1.5f;

  [SerializeField]
  private float _enterShakeFrequency = 0.4f;

  [SerializeField]
  private float _enterShakeDuration = 0.25f;

  [Header("Boost Settings")]
  [SerializeField]
  private float _initialBoostUsage = 20f;

  [SerializeField]
  private float _continuousBoostUsage = 10f;

  [SerializeField]
  private float _maxVelocity = 100f;

  [Tooltip("Valor mínimo do input de movimento para permitir o cancelamento antecipado do boost.")]
  [SerializeField]
  private float _cancelInputThreshold = 0.4f;

  [SerializeField]
  private float _timeToAllowCancel = 5f;

  [SerializeField]
  private SphereCollider _boostCollider;

  private float _playerOriginalSpeed;
  private float _velocity;
  private bool _canCancel = false;
  private Timer _cancelTimer = new();
  #endregion

  #region IState Callbacks
  public void Enter(Player player)
  {
    _velocity = Mathf.Clamp(player.DashSlashBoostButton.Value, 0f, _maxVelocity);
    _playerOriginalSpeed = player.Speed;
    _cancelTimer.Start(_timeToAllowCancel);

    float velocityFraction = _velocity / _maxVelocity;

    player.LocomotionLayer.ChangeState(player.Moving, player);
    player.MovementVector += player.transform.forward * _velocity;
    player.DashSlashBoostButton.Value -= _initialBoostUsage;
    player.Stats.ModifyStatToTarget(StatType.Speed, _velocity);

    player.SpeedLines.Invoke(true);

    if (_boostCollider != null)
      _boostCollider.enabled = true;

    player.CustomShake.Invoke(
      player.ID,
      _enterShakeAmplitude * velocityFraction,
      _enterShakeFrequency * velocityFraction,
      _enterShakeDuration
    );

    player.TrailsSystem.PlayEffect(TrailType.MovementTrail);
    player.TrailsSystem.PlayEffect(TrailType.MovementSupport1Trail);
    player.TrailsSystem.PlayEffect(TrailType.MovementSupport2Trail);
    player.MainCamera.Lens.FieldOfView = _boostFOV;
  }

  public void Exit(Player player)
  {
    _velocity = 0f;
    _cancelTimer.Stop();

    player.Stats.ModifyStatToTarget(StatType.Speed, _playerOriginalSpeed);
    player.LocomotionLayer.ChangeState(player.Moving, player);

    player.SpeedLines.Invoke(false);
    if (_boostCollider != null)
      _boostCollider.enabled = false;

    player.TrailsSystem.StopEffect(TrailType.MovementTrail);
    player.TrailsSystem.StopEffect(TrailType.MovementSupport1Trail);
    player.TrailsSystem.StopEffect(TrailType.MovementSupport2Trail);

    player.MainCamera.Lens.FieldOfView = _defaultFOV;
  }

  public void Update(Player player)
  {
    player.DashSlashBoostButton.Value -= _continuousBoostUsage * Time.deltaTime;
    player.DashSlashBoostButton.Value = Mathf.Max(0f, player.DashSlashBoostButton.Value);

    if (player.DashSlashBoostButton.Value <= 0f)
    {
      player.ActionLayer.ExitState(this, player);
      return;
    }

    if (player.MoveInput.y <= _cancelInputThreshold && _cancelTimer.Tick(Time.deltaTime))
    {
      player.ActionLayer.ExitState(this, player);
      return;
    }
  }

  public void FixedUpdate(Player player) { }

  #endregion
}
