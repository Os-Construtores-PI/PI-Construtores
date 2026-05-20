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

  private readonly float _rotationSpeed = 50f;
  private readonly float _boostUsage = 20f;
  private readonly float _slopeLimit = 30f;
  private readonly float _maxVelocity = 100;
  private float _velocity;

  #endregion

  //TODO: Fazer não cancelar quando cair
  //TODO: Testar fazer funcionar só com o botão pressionado

  #region IState Callbacks

  public void Enter(Player player)
  {
    _velocity = player.DashSlashBoostButton.Value;
    float velocityFraction = _velocity / _maxVelocity;

    player.LocomotionLayer.ChangeState(player.HLocked, player);
    player.SpeedLines.Invoke(true);
    player.CustomShake.Invoke(
      player.ID,
      EnterShakeAmplitude * velocityFraction,
      EnterShakeFrequency * velocityFraction,
      EnterShakeDuration
    );

    //Systems
    player.TrailsSystem.PlayEffect(TrailType.MovementTrail);
    player.TrailsSystem.PlayEffect(TrailType.MovementSupport1Trail);
    player.TrailsSystem.PlayEffect(TrailType.MovementSupport2Trail);
    //player.EffectsSystem.PlayEffect(EffectType.BoostEffect, 0.15f);
    SetBoostCamera(player, active: true);
  }

  public void Exit(Player player)
  {
    _velocity = 0f;

    player.LocomotionLayer.ChangeState(player.Moving, player);

    player.SpeedLines.Invoke(false);
    player.TrailsSystem.StopEffect(TrailType.MovementTrail);
    player.TrailsSystem.StopEffect(TrailType.MovementSupport1Trail);
    player.TrailsSystem.StopEffect(TrailType.MovementSupport2Trail);
    //player.EffectsSystem.StopEffect(EffectType.BoostEffect);

    Vector3 mv = player.MovementVector;
    player.MovementVector = new Vector3(mv.x * 2f, mv.y, mv.z * 2f);

    SetBoostCamera(player, active: false);
  }

  public void Update(Player player)
  {
    player.DashSlashBoostButton.Value -= _boostUsage * Time.deltaTime;

    bool boostDepleted = player.DashSlashBoostButton.Value <= 0f;

    if (boostDepleted)
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
