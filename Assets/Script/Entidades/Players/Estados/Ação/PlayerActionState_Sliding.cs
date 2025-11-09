using UnityEngine;

public class PlayerActionStateSliding : IState<PlayerContext>
{
    private readonly Timer wallExitTimer = new();

    private void WallRunningTimer(PlayerContext context)
    {
        if (!context.TouchingWall && context.WallSpeedApplied && wallExitTimer.IsDone)
            wallExitTimer.Start(context.WallExitDuration);

        if (wallExitTimer.Tick(Time.deltaTime))
        {
            context.PlayerStats.RemoveActiveModifications(Constants.StatsNames.Speed.ToString());
            context.WallSpeedApplied = false;
            context.TouchingWall = false;
            UnBlockPlayerDash(context);
            context.Gravity = context.InitialGravityValue;
            context.ActionLayer.ChangeState(new PlayerActionStateIdle(), context);
        }
    }
    private void ResetWallExitTimer() => wallExitTimer.Stop();
    private void BlockPlayerDash(PlayerContext context)
    {
        if (context.IsDashBlocked) { return; }
        context.IsDashBlocked = true;
        context.PlayerStats.ModifyStatImmediate<bool>(
            Constants.StatsNames.CanDash.ToString(),
            ModifyTYPE.NEGATIVE,
            QualityTier.COMMON
        );
    }

    private void UnBlockPlayerDash(PlayerContext context)
    {
        if (!context.IsDashBlocked)
            return;
        context.IsDashBlocked = false;
        context.PlayerStats.ModifyStatImmediate<bool>(
            Constants.StatsNames.CanDash.ToString(),
            ModifyTYPE.POSITIVE,
            QualityTier.COMMON
        );
        context.PlayerStats.RemoveActiveModifications(Constants.StatsNames.CanDash.ToString());
    }
    public void Enter(PlayerContext context)
    {
        context.OverrideHorizontal = true;
        context.TouchingWall = true;
        context.CurrentJumpCount = 1;
        // só reseta se já estava fora da parede
        if (wallExitTimer.IsActive)
        {
            ResetWallExitTimer();
        }

        if (!context.WallSpeedApplied)
        {
            context.PlayerStats.RemoveActiveModifications(Constants.StatsNames.Speed.ToString()); // garante que não acumule
            context.PlayerStats.ModifyStatImmediate<float>(
                Constants.StatsNames.Speed.ToString(),
                ModifyTYPE.POSITIVE,
                context.WallSpeedMultiplier
            );
            context.WallSpeedApplied = true;
            BlockPlayerDash(context);
        }
        context.Gravity = -2f;
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
