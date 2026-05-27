using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerActionStateBoost : IState<Player>
{
  #region IState
  public ActionType Type => ActionType.Boost;
  public HashSet<ActionType> IncompatibleActions => _incompatibleActions;
  #endregion

  #region Constants
  private const int MainCameraPriority = 10;
  private const int BoostCameraPriority = 20;
  private const int InactivePriority = 0;

  private const float EnterShakeAmplitude = 1.5f;
  private const float EnterShakeFrequency = .4f;
  private const float EnterShakeDuration = .25f;
  #endregion

  #region Fields
  private readonly HashSet<ActionType> _incompatibleActions = new();
  private readonly float _rotationSpeed = 50f;
  private readonly float _boostUsage = 20f;
  private readonly float _slopeLimit = 30f;
  private readonly float _maxVelocity = 100f;
  private readonly float _forcedDuration = 1.5f;
  private float _playerOriginalSpeed;

  private float _velocity;
  private float _forcedTimer;
  private bool _isFree;
  #endregion

  #region IState Callbacks
  public void Enter(Player player)
  {
    _velocity = Mathf.Clamp(player.DashSlashBoostButton.Value, 0f, _maxVelocity);
    _playerOriginalSpeed = player.Speed;
    _forcedTimer = _forcedDuration;
    _isFree = false;

    float velocityFraction = _velocity / _maxVelocity;

    player.LocomotionLayer.ChangeState(player.LockedInHorizontal, player);
    player.SpeedLines.Invoke(true);
    player.CustomShake.Invoke(
      player.ID,
      EnterShakeAmplitude * velocityFraction,
      EnterShakeFrequency * velocityFraction,
      EnterShakeDuration
    );

    player.TrailsSystem.PlayEffect(TrailType.MovementTrail);
    player.TrailsSystem.PlayEffect(TrailType.MovementSupport1Trail);
    player.TrailsSystem.PlayEffect(TrailType.MovementSupport2Trail);

    SetBoostCamera(player, active: true);
  }

  public void Exit(Player player)
  {
    _velocity = 0f;
    _forcedTimer = 0f;
    _isFree = false;

    TransitionToFreeMovement(player);

    player.Stats.ModifyStatToTarget(StatType.Speed, _playerOriginalSpeed);
    player.SpeedLines.Invoke(false);

    player.TrailsSystem.StopEffect(TrailType.MovementTrail);
    player.TrailsSystem.StopEffect(TrailType.MovementSupport1Trail);
    player.TrailsSystem.StopEffect(TrailType.MovementSupport2Trail);
    player.MainCamera.Lens.FieldOfView = 80;
  }

  public void Update(Player player)
  {
    player.DashSlashBoostButton.Value -= _boostUsage * Time.deltaTime;
    player.DashSlashBoostButton.Value = Mathf.Max(0f, player.DashSlashBoostButton.Value);

    if (player.DashSlashBoostButton.Value <= 0f)
    {
      player.ActionLayer.ExitState(this, player);
      return;
    }

    if (!_isFree)
    {
      _forcedTimer -= Time.deltaTime;
      if (_forcedTimer <= 0f)
      {
        TransitionToFreeMovement(player);
      }
    }
  }

  public void FixedUpdate(Player player)
  {
    if (_isFree)
      return;

    RotatePlayer(player);
    ApplyVelocity(player);
  }
  #endregion

  #region Private Methods
  private void TransitionToFreeMovement(Player player)
  {
    _isFree = true;

    Vector3 safeMovement = player.MovementVector;
    safeMovement.y = 0f;
    player.MovementVector = safeMovement;

    player.Stats.ModifyStatToTarget(StatType.Speed, _velocity);
    player.LocomotionLayer.ChangeState(player.Moving, player);
    player.MainCamera.Lens.FieldOfView = 120;
    SetBoostCamera(player, false);
  }

  private void RotatePlayer(Player player)
  {
    player.transform.Rotate(
      Vector3.up,
      player.MoveInput.x * _rotationSpeed * Time.fixedDeltaTime,
      Space.World
    );
  }

  private void ApplyVelocity(Player player)
  {
    if (OnSlope(player, out RaycastHit hit))
    {
      Vector3 slopeDir = Vector3.ProjectOnPlane(player.transform.forward, hit.normal).normalized;
      player.MovementVector = slopeDir * _velocity;
    }
    else
    {
      Vector3 horizontal = player.transform.forward * _velocity;
      player.MovementVector = new Vector3(horizontal.x, player.MovementVector.y, horizontal.z);
    }
  }

  private bool OnSlope(Player player, out RaycastHit hit)
  {
    Vector3 rayOrigin =
      player.transform.position
      - Vector3.up
        * (player.CharacterController.height * 0.5f - player.CharacterController.skinWidth);
    float reach = player.CharacterController.height * 0.5f + 1f;

    if (Physics.Raycast(rayOrigin, Vector3.down, out hit, reach))
    {
      float angle = Vector3.Angle(hit.normal, Vector3.up);
      return angle > 0f && angle <= _slopeLimit;
    }
    return false;
  }

  private static void SetBoostCamera(Player player, bool active)
  {
    player.MainCamera.Priority = active ? InactivePriority : MainCameraPriority;
    player.BoostCamera.Priority = active ? BoostCameraPriority : InactivePriority;
  }
  #endregion
}
