using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerActionStateJump : IState<Player>
{
  public ActionType Type => ActionType.Jump;

  public HashSet<ActionType> IncompatibleActions => _incompatibleActions;
  private readonly HashSet<ActionType> _incompatibleActions = new();

  [SerializeField]
  private float _jumpHeightMultiplierPerExtraJump = 0.35f;

  [SerializeField]
  private float _wallJumpHorizontalBias = 6.5f;

  public void Enter(Player player)
  {
    Vector3 targetVelocity = player.MovementVector;
    float jumpY;

    PlayerActionStateBounce bounceState = player.ActionLayer.GetActive<PlayerActionStateBounce>();
    bool isBouncing = bounceState != null && player.GroundSlamImpactSpeed > 0f;

    if (isBouncing)
    {
      jumpY = bounceState.CalculateBounceImpulse(player, ref targetVelocity);
      player.ActionLayer.ExitState(bounceState, player);
    }
    else
    {
      float jumpMultiplier = 1f + player.CurrentJumpCount * _jumpHeightMultiplierPerExtraJump;
      jumpY = player.JumpForce * jumpMultiplier;
    }

    if (player.TouchingWall)
    {
      Vector3 jumpDir = (Vector3.up + player.LastWallNormal * _wallJumpHorizontalBias).normalized;
      targetVelocity = jumpDir * player.JumpForce * player.WallJumpMultiplier;
      player.TouchingWall = false;
    }
    else
    {
      targetVelocity.y = jumpY;
    }

    if (player.CurrentJumpCount > 0)
      player.AnimatorComponent?.SetTrigger(Constants.AnimatorTriggerNames.DoubleJump);

    player.CurrentJumpCount++;

    if (player.ActionLayer.GetActive<PlayerActionStateRailSlide>() != null)
      player.WantsToCancelRailSlide = true;

    player.EffectsSystem?.PlayEffect(EffectType.JumpEffect, 1);

    player.MovementVector = new Vector3(
      player.MovementVector.x,
      targetVelocity.y,
      player.MovementVector.z
    );
    player.ActionLayer.ExitState(this, player);
  }

  public void Exit(Player player) { }

  public void Update(Player player) { }

  public void FixedUpdate(Player player) { }
}
