using System.Collections.Generic;
using UnityEngine;

public class PlayerActionStateGroundSlam : IState<Player>
{
  public ActionType Type { get; }
  public HashSet<ActionType> IncompatibleActions => new() { };
  public int Priority => 0;

  public void Enter(Player player)
  {
    player.LocomotionLayer.ChangeState(player.LockedS, player);
  }

  public void Update(Player player) { }

  public void FixedUpdate(Player player)
  {
    if (!player.IsGrounded)
    {
      player.MovementVector = Vector3.down * 75;
      player.GroundSlamHitboxCollider.enabled = true;
    }
    else
    {
      player.LocomotionLayer.ChangeState(player.GroundedS, player);
      player.ActionLayer.PopState(player);
    }
  }

  public void Exit(Player player)
  {
    player.GroundSlamHitboxCollider.enabled = false;
  }
}
