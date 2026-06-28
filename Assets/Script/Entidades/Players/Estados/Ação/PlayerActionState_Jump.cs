using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerActionStateJump : IPlayerState<Player>
{
  public PlayerActionType Type => PlayerActionType.Jump;
  public HashSet<PlayerActionType> IncompatibleActions => _incompatibleActions;
  private readonly HashSet<PlayerActionType> _incompatibleActions = new();

  [Header("Multiplicador a cada Pulo")]
  [SerializeField]
  private float _jumpHeightMultiplierPerExtraJump = 0.35f;

  [Header("Opções de WallJump")]
  [SerializeField]
  private float _wallJumpHorizontalBias = 6.5f;

  public void Enter(Player player)
  {
    Vector3 targetVelocity = player.MovementVector;
    float jumpY;

    // ─── Bounce ───────────────────────────────────────────────────────────
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

    // ─── Wall Jump ────────────────────────────────────────────────────────
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

    // ─── Rail Slide cancel ────────────────────────────────────────────────
    if (player.ActionLayer.GetActive<PlayerActionStateRailSlide>() != null)
      player.WantsToCancelRailSlide = true;

    if (player.CurrentJumpCount > 0)
      player.AnimatorComponent?.SetTrigger(Constants.AnimatorTriggerNames.DoubleJump);

    player.CurrentJumpCount++;
    player.EffectsSystem?.PlayEffect(EffectType.JumpEffect, 1);

    player.MovementVector = targetVelocity;
    player.ActionLayer.ExitState(this, player);
  }

  public void Exit(Player player) { }

  public void Update(Player player) { }

  public void FixedUpdate(Player player) { }
}
