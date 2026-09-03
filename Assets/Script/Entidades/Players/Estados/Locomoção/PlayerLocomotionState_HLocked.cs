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
    if (player.Motor.IsGrounded)
    {
      player.CurrentJumpCount = 0;
      player.CurrentDashCount = 0;

      if (player.Motor.Engine.Velocity.y < 0f)
      {
        Vector3 move = player.Motor.Engine.Velocity;
        move.y = -2f;
        player.Motor.Engine.BaseVelocity = move;
      }
    }
  }

  public void CalculateKCCVelocity(Player player, ref Vector3 currentVelocity, float deltaTime)
  {
    if (!player.Motor.IsGrounded)
    {
      ILocomotionState<Player>.ApplyGravity(ref currentVelocity, player, deltaTime);
    }
  }

  public void Update(Player player) { }
}
