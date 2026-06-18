using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotionStateMoving : ILocomotionState<Player>
{
  public PlayerActionType Type => PlayerActionType.Move;
  public HashSet<PlayerActionType> IncompatibleActions => new();

  // ─── Coyote Time ──────────────────────────────────────────────────────────
  private readonly Timer _coyoteTimer = new();
  private bool _coyoteStarted = false;
  private const float CoyoteInterval = 0.3f;

  // ─── Enter / Exit ─────────────────────────────────────────────────────────

  public void Enter(Player player)
  {
    _wasGrounded = player.IsGrounded;

    player.Stats.AddStat(StatType.RunSpeedMultiplier, player.RunSpeedMultiplier);
    player.Stats.AddStat(StatType.RunAccelMultiplier, player.RunAccelMultiplier);

    if (player.IsGrounded)
      OnLanded(player);
  }

  public void Exit(Player player)
  {
    _coyoteTimer.Stop();
    _coyoteStarted = false;

    player.Stats.RemoveStat<float>(StatType.RunSpeedMultiplier);
    player.Stats.RemoveStat<float>(StatType.RunAccelMultiplier);
  }

  public void Update(Player player) { }

  // ─── FixedUpdate ──────────────────────────────────────────────────────────

  public void FixedUpdate(Player player)
  {
    if (player.IsGrounded)
      HandleGrounded(player);
    else
      HandleAirborne(player);

    HandleHorizontalMovement(player);
  }

  // ─── Grounded ─────────────────────────────────────────────────────────────

  private bool _wasGrounded;

  private void HandleGrounded(Player player)
  {
    if (!_wasGrounded)
      OnLanded(player);

    _wasGrounded = true;
    _coyoteStarted = false;
    _coyoteTimer.Stop();
  }

  private void OnLanded(Player player)
  {
    player.CurrentJumpCount = 0;
    player.CurrentDashCount = 0;
    player.IsImpulsioned = false;
    player.CanDash = true;
    player.JumpInteractionPressed = false;

    var move = player.MovementVector;
    move.y = -1f;
    player.MovementVector = move;

    if (player.GroundSlamImpactSpeed > 0f)
    {
      player.ActionLayer.PushState(player.Bounce, player);
      player.ActionLayer.PushStateDeferred(player.Jump, player);
    }
  }

  // ─── Airborne ─────────────────────────────────────────────────────────────

  private void HandleAirborne(Player player)
  {
    _wasGrounded = false;
    ILocomotionState<Player>.ApplyGravity(player);

    if (player.ActionLayer.GetActive<PlayerActionStateJump>() == null)
      HandleCoyoteTime(player);
  }

  private void HandleCoyoteTime(Player player)
  {
    if (!_coyoteStarted)
    {
      _coyoteTimer.Start(CoyoteInterval);
      _coyoteStarted = true;
    }

    if (_coyoteTimer.Tick(Time.deltaTime))
    {
      _coyoteStarted = false;
      player.ActionLayer.ExitStateDeferred(player.Bounce, player);
    }
  }

  // ─── Horizontal Movement ──────────────────────────────────────────────────

  private void HandleHorizontalMovement(Player player)
  {
    if (!player.IsGrounded && player.IsImpulsioned && player.MoveInput == Vector2.zero)
      return;

    float speedMult = GetStatValue(player, StatType.RunSpeedMultiplier, player.RunSpeedMultiplier);
    float accelMult = GetStatValue(player, StatType.RunAccelMultiplier, player.RunAccelMultiplier);

    float speed = player.IsRunning ? player.Speed * speedMult : player.Speed;
    float accel = player.IsRunning
      ? (player.IsGrounded ? player.Acceleration * accelMult : player.Acceleration)
      : (player.IsGrounded ? player.Acceleration : player.Acceleration);
    float friction = player.IsGrounded ? player.Friction : player.AirFriction;

    if (player.MoveInput == Vector2.zero)
    {
      var move = player.MovementVector;
      move.x = QualityOfLife.PlayerFriction(move.x, friction, player.MoveInput);
      move.z = QualityOfLife.PlayerFriction(move.z, friction, player.MoveInput);
      player.MovementVector = move;
      return;
    }

    Vector3 direction = ILocomotionState<Player>.CalculateCameraDirection(player);

    player.transform.rotation = Quaternion.Slerp(
      player.transform.rotation,
      Quaternion.LookRotation(direction),
      10f * Time.deltaTime
    );

    var m = player.MovementVector;
    player.MovementVector = new Vector3(
      QualityOfLife.SmoothStepLerp(m.x, direction.x * speed, accel),
      m.y,
      QualityOfLife.SmoothStepLerp(m.z, direction.z * speed, accel)
    );
  }

  // ─── Helpers ──────────────────────────────────────────────────────────────

  private static float GetStatValue(Player player, StatType statType, float fallback)
  {
    player.Stats.TryGetNum(statType, out float value);
    return value > 0f ? value : fallback;
  }
}
