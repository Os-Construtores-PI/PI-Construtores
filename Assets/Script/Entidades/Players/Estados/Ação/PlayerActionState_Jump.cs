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
    player.Motor.Engine.ForceUnground(0.1f);

    Vector3 targetVelocity = player.Motor.Engine.BaseVelocity;
    float jumpY;

    float jumpMultiplier = 1f + player.CurrentJumpCount * _jumpHeightMultiplierPerExtraJump;
    jumpY = player.JumpForce * jumpMultiplier;

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

    if (player.ActionLayer.GetActive<PlayerActionStateRailSlide>() != null)
      player.RailSlide.RequestCancel();

    if (player.CurrentJumpCount > 0)
      player.AnimatorComponent?.SetTrigger(Constants.AnimatorTriggerNames.DoubleJump);

    player.CurrentJumpCount++;
    player.EffectsSystem?.PlayEffect(EntityEffectType.PlayerJumpEffect, 1);

    player.Motor.Engine.BaseVelocity = targetVelocity;

    player.ActionLayer.ExitState(this, player);
  }

  public void Exit(Player player) { }

  public void Update(Player player) { }

  public void FixedUpdate(Player player) { }

  public bool UpdateKCCVelocity(Player player, ref Vector3 currentVelocity, float deltaTime) =>
    false;
}
