using System.Collections.Generic;
using UnityEngine;

public class PlayerActionStateGroundSlam : IState<Player>
{
  public ActionType Type { get; }
  public HashSet<ActionType> IncompatibleActions => new() { };
  public int Priority => 0;

  private bool _deactivated = false;
  private Vector2 _momentum;

  public void Enter(Player player)
  {
    _momentum = new(player.MovementVector.x, player.MovementVector.z);
    player.LocomotionLayer.ChangeState(player.LockedS, player);
    _deactivated = false;
  }

  public void Update(Player player) { }

  public void FixedUpdate(Player player)
  {
    if (!player.IsGrounded)
    {
      player.MovementVector = new(_momentum.x, -75, _momentum.y);
      player.GroundSlamHitboxCollider.enabled = true;
    }
    else if (!_deactivated)
    {
      _deactivated = true;
      player.LocomotionLayer.ChangeState(player.GroundedS, player);
      player.JumpInputPressed = true;
      player.ActionLayer.PopStateDeferred(player);
    }
  }

  public void Exit(Player player)
  {
    player.GroundSlamHitboxCollider.enabled = false;
  }
}
