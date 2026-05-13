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
  public bool WasLaunched;

  public void Enter(Player player)
  {
    WasLaunched = false;
    player.LocomotionLayer.ChangeState(player.HLockedS, player);
    _velocity = player.DashSlashBoostButton.Value;
    player.MainCamera.Priority = 0;
    player.BoostCamera.Priority = 20;
  }

  public void Exit(Player player)
  {
    player.LocomotionLayer.ChangeState(player.GroundedS, player);
    player.MovementVector *= 2;
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

    if (OnSlope(player, out RaycastHit hit))
    {
      Vector3 moveDir = Vector3.ProjectOnPlane(player.transform.forward, hit.normal).normalized;
      player.MovementVector = moveDir * _velocity;
    }
    else
    {
      Vector3 horizontal = player.transform.forward * _velocity;
      player.MovementVector = new(horizontal.x, player.MovementVector.y, horizontal.z);
    }
  }

  public void Update(Player player)
  {
    player.DashSlashBoostButton.Value -= _boostUsage * Time.deltaTime;

    if (!player.IsGrounded)
      WasLaunched = true;

    if (player.IsGrounded && WasLaunched)
      player.ActionLayer.ExitState(this, player);

    if (player.DashSlashBoostButton.Value <= 0)
      player.ActionLayer.ExitState(this, player);
  }

  private bool OnSlope(Player player, out RaycastHit hit)
  {
    hit = default;

    // Aumenta o reach para não perder contato em slopes
    float reach = player.CharacterController.height / 2 + 1f;

    if (Physics.Raycast(player.transform.position, Vector3.down, out hit, reach))
    {
      float angle = Vector3.Angle(hit.normal, Vector3.up);
      return angle > 0 && angle <= slopeLimit;
    }

    return false;
  }
}
