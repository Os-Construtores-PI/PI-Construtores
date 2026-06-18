using System.Collections.Generic;
using UnityEngine;

public class PlayerActionStateWallSliding : IPlayerState<Player>
{
  private readonly Timer wallExitTimer = new();

  public PlayerActionType Type => PlayerActionType.Slide;

  public HashSet<PlayerActionType> IncompatibleActions => new();

  private void WallRunningTimer(Player player)
  {
    if (!player.TouchingWall && player.WallSpeedApplied && wallExitTimer.IsDone)
      wallExitTimer.Start(player.WallExitDuration);

    if (wallExitTimer.Tick(Time.deltaTime))
    {
      player.Stats.RemoveActiveModifications(StatType.Speed);
      player.WallSpeedApplied = false;
      player.TouchingWall = false;
      UnBlockPlayerDash(player);
      player.GravityValue = player.InitialGravityValue;
      player.ActionLayer.ExitStateDeferred(this, player);
    }
  }

  private void ResetWallExitTimer() => wallExitTimer.Stop();

  private void BlockPlayerDash(Player player)
  {
    if (player.IsDashBlocked)
    {
      return;
    }
    player.IsDashBlocked = true;
    player.Stats.ModifyStatImmediate<bool>(
      StatType.CanDash,
      ModifyTYPE.NEGATIVE,
      QualityTier.COMMON
    );
  }

  private void UnBlockPlayerDash(Player player)
  {
    if (!player.IsDashBlocked)
    {
      return;
    }
    player.IsDashBlocked = false;
    player.Stats.ModifyStatImmediate<bool>(
      StatType.CanDash,
      ModifyTYPE.POSITIVE,
      QualityTier.COMMON
    );
    player.Stats.RemoveActiveModifications(StatType.CanDash);
  }

  public void Enter(Player player)
  {
    player.CurrentJumpCount = 1;
    player.TouchingWall = true;
    if (wallExitTimer.IsActive)
    {
      ResetWallExitTimer();
    }

    if (!player.WallSpeedApplied)
    {
      player.Stats.RemoveActiveModifications(StatType.Speed);
      player.Stats.ModifyStatImmediate<float>(
        StatType.Speed,
        ModifyTYPE.POSITIVE,
        player.WallSpeedMultiplier
      );
      player.WallSpeedApplied = true;
      BlockPlayerDash(player);
    }
    player.GravityValue = -1.5f;
  }

  public void Exit(Player player)
  {
    // player.OverrideHorizontal = false;
  }

  public void FixedUpdate(Player player) { }

  public void Update(Player player)
  {
    WallRunningTimer(player);
  }
}
