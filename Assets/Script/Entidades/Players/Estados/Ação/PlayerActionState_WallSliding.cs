using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerActionStateWallSliding : IPlayerState<Player>, IDisposable
{
  public PlayerActionType Type => PlayerActionType.Slide;
  public HashSet<PlayerActionType> IncompatibleActions => new();

  private CancellationTokenSource _wallSlideCts;
  private CancellationTokenSource _linkedCts;

  private bool _isExiting;
  private string _speedSourceId;

  private float _wallSlideVerticalSpeed = 0f;
  private const float MAX_WALL_SLIDE_SPEED = -3f;

  public void Enter(Player player)
  {
    _isExiting = false;
    _wallSlideVerticalSpeed = 0f;
    player.CurrentJumpCount = 1;
    player.TouchingWall = true;

    CancelWallSlideEffects(player);

    _ = StartWallSlideAsync(player);
  }

  public void Exit(Player player)
  {
    if (_isExiting)
      return;
    _isExiting = true;

    CancelWallSlideEffects(player);
  }

  public void Update(Player player) { }

  public void FixedUpdate(Player player) { }

  public bool UpdateKCCVelocity(Player player, ref Vector3 currentVelocity, float deltaTime)
  {
    if (_isExiting)
    {
      currentVelocity = Vector3.zero;
      return true;
    }

    Vector3 horizontal = new Vector3(currentVelocity.x, 0f, currentVelocity.z);
    horizontal = Vector3.MoveTowards(horizontal, Vector3.zero, player.AirFriction * deltaTime);

    float wallGravity = player.GravityValue * player.GravityDownMultiplier * 0.3f;
    _wallSlideVerticalSpeed += wallGravity * deltaTime;

    if (_wallSlideVerticalSpeed < MAX_WALL_SLIDE_SPEED)
      _wallSlideVerticalSpeed = MAX_WALL_SLIDE_SPEED;

    Vector3 wallPush = Vector3.zero;
    if (player.LastWallNormal != Vector3.zero)
    {
      wallPush = player.LastWallNormal * 0.5f;
    }

    currentVelocity = horizontal + Vector3.up * _wallSlideVerticalSpeed + wallPush * deltaTime;

    return true;
  }

  private async Task StartWallSlideAsync(Player player)
  {
    _wallSlideCts = new CancellationTokenSource();

    var playerLifetime = player.GetCancellationToken();
    _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
      _wallSlideCts.Token,
      playerLifetime
    );

    try
    {
      ApplyWallEffects(player);

      await WaitForWallExitAsync(player, _linkedCts.Token);

      if (!_linkedCts.Token.IsCancellationRequested && !_isExiting)
      {
        await ExitWallSlideAsync(player);
      }
    }
    catch (OperationCanceledException)
    {
      Debug.Log("[WallSlide] Efeito cancelado (provavelmente trocou de state)");
    }
    catch (Exception ex)
    {
      Debug.LogError($"[WallSlide] Erro inesperado: {ex.Message}");
    }
    finally
    {
      CleanupTokens();
    }
  }

  private void ApplyWallEffects(Player player)
  {
    _speedSourceId = player.Stats.ApplyMultiplier(StatType.Speed, player.WallSpeedMultiplier);
    player.WallSpeedApplied = true;

    player.Stats.SetBool(StatType.CanDash, false);

    player.GravityValue = -1.5f;
  }

  private async Task WaitForWallExitAsync(Player player, CancellationToken ct)
  {
    while (player.TouchingWall)
    {
      ct.ThrowIfCancellationRequested();
      await Task.Yield();
    }

    if (player.WallExitDuration > 0f)
    {
      float elapsed = 0f;
      while (elapsed < player.WallExitDuration)
      {
        ct.ThrowIfCancellationRequested();

        if (player.TouchingWall)
          return;

        elapsed += Time.deltaTime;
        await Task.Yield();
      }
    }
  }

  private async Task ExitWallSlideAsync(Player player)
  {
    if (!string.IsNullOrEmpty(_speedSourceId))
    {
      player.Stats.RemoveMultiplier(StatType.Speed, _speedSourceId);
      _speedSourceId = null;
    }
    player.WallSpeedApplied = false;

    player.Stats.SetBool(StatType.CanDash, true);

    player.GravityValue = player.InitialGravityValue;
    player.TouchingWall = false;

    player.ActionLayer.ExitStateDeferred(this, player);
  }

  private void CancelWallSlideEffects(Player player)
  {
    _wallSlideCts?.Cancel();

    if (!string.IsNullOrEmpty(_speedSourceId))
    {
      player.Stats.RemoveMultiplier(StatType.Speed, _speedSourceId);
      _speedSourceId = null;
    }
    player.Stats.SetBool(StatType.CanDash, true);

    player.WallSpeedApplied = false;
    player.TouchingWall = false;
    player.GravityValue = player.InitialGravityValue;
  }

  public void Dispose()
  {
    CleanupTokens();
  }

  private void CleanupTokens()
  {
    _wallSlideCts?.Dispose();
    _wallSlideCts = null;

    _linkedCts?.Dispose();
    _linkedCts = null;
  }
}
