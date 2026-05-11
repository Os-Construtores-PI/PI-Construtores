using System.Collections.Generic;
using UnityEngine;

public class PlayerActionStateJump : IState<Player>
{
  public ActionType Type => ActionType.Jump;
  public HashSet<ActionType> IncompatibleActions => new();

  public void Enter(Player player)
  {
    player.JumpInputPressed = false;

    var move = player.MovementVector;
    var bounceState = player.ActionLayer.GetActive<PlayerActionStateBounce>();
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
    player.EffectsWorker.PlayEffect(Constants.EffectsNames.Player.Jump, 1);
    player.MovementVector = move;
    player.LocomotionLayer.ChangeState(player.AirborneS, player);
    player.ActionLayer.ExitState(this, player); // ← libera para o próximo pulo
  }

  public void Exit(Player player) { }

  public void Update(Player player) { }

  public void FixedUpdate(Player player) { }
}
