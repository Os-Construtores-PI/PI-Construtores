using System.Collections.Generic;
using UnityEngine;

public class PlayerActionStateJump : IState<Player>
{
  public ActionType Type => ActionType.Jump;
  public HashSet<ActionType> IncompatibleActions => new();

  public void Enter(Player player)
  {
    Vector3 move = player.MovementVector;
    PlayerActionStateBounce bounceState = player.ActionLayer.GetActive<PlayerActionStateBounce>();
    bool isBounce = bounceState != null && player.GroundSlamImpactSpeed > 0f;
    float jumpY;

    if (isBounce)
    {
      jumpY = bounceState.CalculateBounceImpulse(player, ref move);
      player.ActionLayer.ExitState(bounceState, player);
    }
    else
    {
      float jumpMultiplier = 1f + player.CurrentJumpCount * 0.35f;
      jumpY = player.JumpForce * jumpMultiplier;
    }

    if (player.TouchingWall)
    {
      const float horizontalBias = 6.5f;
      var jumpDir = (Vector3.up + player.LastWallNormal * horizontalBias).normalized;
      move = player.JumpForce * player.WallJumpMultiplier * jumpDir;
      player.TouchingWall = false;
    }
    else
    {
      move.y = jumpY;
    }

    if (player.CurrentJumpCount > 0)
      player.AnimatorComponent.SetTrigger(Constants.AnimatorTriggerNames.DoubleJump);

    player.CurrentJumpCount++;
    player.EffectsSystem.PlayEffect(Constants.EffectsNames.Player.Jump, 1);
    player.MovementVector = move;
    player.ActionLayer.ExitState(this, player);
  }

  public void Exit(Player player) { }

  public void Update(Player player) { }

  public void FixedUpdate(Player player) { }
}
