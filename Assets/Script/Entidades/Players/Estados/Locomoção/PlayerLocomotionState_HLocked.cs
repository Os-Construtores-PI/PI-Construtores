using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotionStateHLocked : ILocomotionState<Player>
{
  public PlayerActionType Type => PlayerActionType.Locked;

  public HashSet<PlayerActionType> IncompatibleActions => new();

  public void Enter(Player player) { }

  public void Exit(Player player) { }

  public void FixedUpdate(Player player)
  {
    if (player.IsGrounded)
    {
      player.CurrentJumpCount = 0;
      player.CurrentDashCount = 0;

      if (player.MovementVector.y < 0f)
      {
        Vector3 move = player.MovementVector;
        move.y = -2f;
        player.MovementVector = move;
      }
    }
    else
    {
      ILocomotionState<Player>.ApplyGravity(player);
    }
  }

  public void Update(Player player) { }
}
