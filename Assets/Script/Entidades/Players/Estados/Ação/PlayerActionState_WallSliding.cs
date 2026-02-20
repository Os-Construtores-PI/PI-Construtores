using System.Collections.Generic;
using UnityEngine;

public class PlayerActionStateWallSliding : IState<PlayerContext>
{
    private readonly Timer wallExitTimer = new();

    public ActionType Type => ActionType.Slide;

    public HashSet<ActionType> IncompatibleActions => new();

    private void WallRunningTimer(PlayerContext context)
    {
        if (!context.PlayerTouchingWall && context.PlayerWallSpeedApplied && wallExitTimer.IsDone)
            wallExitTimer.Start(context.PlayerWallExitDuration);

        if (wallExitTimer.Tick(Time.deltaTime))
        {
            context.LiveEntityStats.RemoveActiveModifications(Constants.StatsNames.Speed.ToString());
            context.PlayerWallSpeedApplied = false;
            context.PlayerTouchingWall = false;
            UnBlockPlayerDash(context);
            context.PlayerGravity = context.InitialGravityValue;
            context.PlayerActionLayer.PopStateDeferred(context);
        }
    }
    private void ResetWallExitTimer() => wallExitTimer.Stop();
    private void BlockPlayerDash(PlayerContext context)
    {
        if (context.IsDashBlocked) { return; }
        context.IsDashBlocked = true;
        context.LiveEntityStats.ModifyStatImmediate<bool>(
            Constants.StatsNames.CanDash.ToString(),
            ModifyTYPE.NEGATIVE,
            QualityTier.COMMON
        );
    }

    private void UnBlockPlayerDash(PlayerContext context)
    {
        if (!context.IsDashBlocked)
        {
            return;
        }
        context.IsDashBlocked = false;
        context.LiveEntityStats.ModifyStatImmediate<bool>(
            Constants.StatsNames.CanDash.ToString(),
            ModifyTYPE.POSITIVE,
            QualityTier.COMMON
        );
        context.LiveEntityStats.RemoveActiveModifications(Constants.StatsNames.CanDash.ToString());
    }
    public void Enter(PlayerContext context)
    {
        context.OverrideHorizontal = true;
        context.PlayerCurrentJumpCount = 1;
        context.PlayerTouchingWall = true;
        // só reseta se já estava fora da parede
        if (wallExitTimer.IsActive)
        {
            ResetWallExitTimer();
        }

        if (!context.PlayerWallSpeedApplied)
        {
            context.LiveEntityStats.RemoveActiveModifications(Constants.StatsNames.Speed.ToString()); // garante que não acumule
            context.LiveEntityStats.ModifyStatImmediate<float>(
                Constants.StatsNames.Speed.ToString(),
                ModifyTYPE.POSITIVE,
                context.PlayerWallSpeedMultiplier
            );
            context.PlayerWallSpeedApplied = true;
            BlockPlayerDash(context);
        }
        context.PlayerGravity = -1.5f;
    }
    public void Exit(PlayerContext context)
    {
        context.OverrideHorizontal = false;
    }

    public void FixedUpdate(PlayerContext context)
    {
    }

    public void Update(PlayerContext context)
    {
        WallRunningTimer(context);
    }
}
