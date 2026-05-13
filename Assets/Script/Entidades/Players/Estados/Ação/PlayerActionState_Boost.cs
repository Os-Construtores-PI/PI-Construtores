using System.Collections.Generic;
using DG.Tweening;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerActionStateBoost : IState<Player>
{
  public ActionType Type => ActionType.Boost;
  public HashSet<ActionType> IncompatibleActions => new();

  private float _rotationSpeed = 30f;
  private float _boostUsage = 20;
  private float _velocity = 50f;
  private float slopeLimit = 30f;

  public void Enter(Player player)
  {
    player.LocomotionLayer.ChangeState(player.HLockedS, player);
    _velocity = player.DashSlashBoostButton.Value;
    player.MainCamera.Priority = 0;
    player.BoostCamera.Priority = 20;
  }

  public void Exit(Player player)
  {
    player.LocomotionLayer.ChangeState(player.GroundedS, player);
    player.MovementVector += player.CharacterController.velocity;
    _velocity = 0f;
    player.MainCamera.Priority = 20;
    player.BoostCamera.Priority = 0;
  }

  public void FixedUpdate(Player player)
  {
    player.transform.Rotate(
      Vector3.up,
      player.MoveInput.x * _rotationSpeed * Time.deltaTime,
      Space.World
    );

    Vector3 moveDir = OnSlope(player, out RaycastHit hit)
      ? Vector3.ProjectOnPlane(player.transform.forward, hit.normal).normalized
      : player.transform.forward;

    Vector3 horizontal = moveDir * _velocity;
    player.MovementVector = new(horizontal.x, player.MovementVector.y, horizontal.z);
  }

  public void Update(Player player)
  {
    player.DashSlashBoostButton.Value -= _boostUsage * Time.deltaTime;

    if (!player.IsGrounded)
    {
      player.ActionLayer.ExitState(this, player);
      player.IsImpulsioned = true;
    }
    if (player.DashSlashBoostButton.Value <= 0)
    {
      player.ActionLayer.ExitState(this, player);
    }
  }

  private bool OnSlope(Player player, out RaycastHit hit)
  {
    hit = default;

    if (!player.IsGrounded)
      return false;

    if (
      Physics.Raycast(
        player.transform.position,
        Vector3.down,
        out hit,
        player.CharacterController.height / 2 + 0.5f
      )
    )
    {
      float angle = Vector3.Angle(hit.normal, Vector3.up);
      return angle > 0 && angle <= slopeLimit;
    }

    return false;
  }
}
