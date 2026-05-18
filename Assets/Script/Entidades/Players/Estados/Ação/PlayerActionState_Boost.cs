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

  private const int MainCameraPriority = 20;
  private const int BoostCameraPriority = 20;
  private const int InactivePriority = 0;

  // Shake

  private const float EnterShakeAmplitude = 1.5f;
  private const float EnterShakeFrequency = .4f;
  private const float EnterShakeDuration = .25f;

  #endregion

  #region Fields

  private readonly HashSet<ActionType> _incompatibleActions = new();

  private readonly float _rotationSpeed = 30f;
  private readonly float _boostUsage = 20f;
  private readonly float _slopeLimit = 30f;
  private float _velocity;

  public bool WasLaunched;

  #endregion

  #region IState Callbacks

  public void Enter(Player player)
  {
    WasLaunched = false;
    _velocity = player.DashSlashBoostButton.Value;

    player.LocomotionLayer.ChangeState(player.HLockedS, player);
    player.SpeedLines.Invoke(true);
    player.TrailsSystem.PlayEffect(Constants.TrailsNames.Movement);
    player.CustomShake.Invoke(
      player.ID,
      EnterShakeAmplitude,
      EnterShakeFrequency,
      EnterShakeDuration
    );
    player.EffectsSystem.PlayEffect(Constants.EffectsNames.Player.Boost, 0.15f);
    SetBoostCamera(player, active: true);
  }

  public void Exit(Player player)
  {
    _velocity = 0f;

    player.LocomotionLayer.ChangeState(player.GroundedS, player);

    player.SpeedLines.Invoke(false);
    player.TrailsSystem.StopEffect(Constants.TrailsNames.Movement);
    player.EffectsSystem.StopEffect(Constants.EffectsNames.Player.Boost);

    Vector3 mv = player.MovementVector;
    player.MovementVector = new Vector3(mv.x * 2f, mv.y, mv.z * 2f);

    SetBoostCamera(player, active: false);
  }

  public void Update(Player player)
  {
    player.DashSlashBoostButton.Value -= _boostUsage * Time.deltaTime;

    if (!player.IsGrounded)
      WasLaunched = true;

    bool landedAfterLaunch = player.IsGrounded && WasLaunched;
    bool boostDepleted = player.DashSlashBoostButton.Value <= 0f;

    if (landedAfterLaunch || boostDepleted)
      player.ActionLayer.ExitState(this, player);
  }

  public void FixedUpdate(Player player)
  {
    RotatePlayer(player);
    ApplyVelocity(player);
  }

  #endregion

  #region Private Methods

  private void RotatePlayer(Player player)
  {
    player.transform.Rotate(
      Vector3.up,
      player.MoveInput.x * _rotationSpeed * Time.deltaTime,
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
    float reach = player.CharacterController.height / 2f + 1f;

    if (Physics.Raycast(player.transform.position, Vector3.down, out hit, reach))
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
