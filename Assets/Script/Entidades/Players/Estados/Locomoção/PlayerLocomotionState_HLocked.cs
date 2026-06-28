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
    ILocomotionState<Player>.ApplyGravity(player);
    if (player.IsGrounded)
    {
      player.CurrentJumpCount = 0;
      player.CurrentDashCount = 0;
    }
  }

  public void Update(Player player) { }
}
