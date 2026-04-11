using System.Collections.Generic;
using UnityEngine;

public class PlayerActionStateWallSliding : IState<Player>
{
  private readonly Timer wallExitTimer = new();

  public ActionType Type => ActionType.Slide;

  public HashSet<ActionType> IncompatibleActions => new();

  private void WallRunningTimer(Player player)
  {
    if (!player.TouchingWall && player.WallSpeedApplied && wallExitTimer.IsDone)
      wallExitTimer.Start(player.WallExitDuration);

    if (wallExitTimer.Tick(Time.deltaTime))
    {
      player.Stats.RemoveActiveModifications(Constants.StatsNames.Speed.ToString());
      player.WallSpeedApplied = false;
      player.TouchingWall = false;
      UnBlockPlayerDash(player);
      player.GravityValue = player.InitialGravityValue;
      player.ActionLayer.PopStateDeferred(player);
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
      Constants.StatsNames.CanDash.ToString(),
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
      Constants.StatsNames.CanDash.ToString(),
      ModifyTYPE.POSITIVE,
      QualityTier.COMMON
    );
    player.Stats.RemoveActiveModifications(Constants.StatsNames.CanDash.ToString());
  }

  public void Enter(Player player)
  {
    player.OverrideHorizontal = true;
    player.CurrentJumpCount = 1;
    player.TouchingWall = true;
    // só reseta se já estava fora da parede
    if (wallExitTimer.IsActive)
    {
      ResetWallExitTimer();
    }

    if (!player.WallSpeedApplied)
    {
      player.Stats.RemoveActiveModifications(Constants.StatsNames.Speed.ToString()); // garante que não acumule
      player.Stats.ModifyStatImmediate<float>(
        Constants.StatsNames.Speed.ToString(),
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
    player.OverrideHorizontal = false;
  }

  public void FixedUpdate(Player player) { }

  public void Update(Player player)
  {
    WallRunningTimer(player);
  }
}
