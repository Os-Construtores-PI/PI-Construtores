using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotionStateMoving : ILocomotionState<Player>
{
  public ActionType Type => ActionType.Move;
  public HashSet<ActionType> IncompatibleActions => new();

  [HideInInspector]
  public readonly Timer RailExitMomentumTimer = new();

  [HideInInspector]
  public readonly Timer RailGraceTimer = new();

  // ─── Coyote Time ──────────────────────────────────────────────────────────
  private readonly Timer _coyoteTimer = new();
  private bool _coyoteStarted = false;
  private const float CoyoteInterval = 0.3f;
  private const float RailGraceDuration = 0.45f;

  // ─── Enter / Exit ─────────────────────────────────────────────────────────

  public void Enter(Player player)
  {
    _wasGrounded = player.IsGrounded;

    if (RailExitMomentumTimer.TimeLeft > 0f)
    {
      RailGraceTimer.Start(RailGraceDuration);
    }

    player.Stats.AddStat(StatType.RunSpeedMultiplier, player.RunSpeedMultiplier);
    player.Stats.AddStat(StatType.RunAccelMultiplier, player.RunAccelMultiplier);

    if (player.IsGrounded)
      OnLanded(player);
  }

  public void Exit(Player player)
  {
    _coyoteTimer.Stop();
    _coyoteStarted = false;
    RailGraceTimer.Stop();

    player.Stats.RemoveStat<float>(StatType.RunSpeedMultiplier);
    player.Stats.RemoveStat<float>(StatType.RunAccelMultiplier);
  }

  public void Update(Player player) { }

  // ─── FixedUpdate ──────────────────────────────────────────────────────────

  public void FixedUpdate(Player player)
  {
    RailGraceTimer.Tick(Time.deltaTime);

    RailExitMomentumTimer.Tick(Time.deltaTime);

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
    RailGraceTimer.Stop();
  }

  private void OnLanded(Player player)
  {
    player.CurrentJumpCount = 0;
    player.CurrentDashCount = 0;
    player.IsImpulsioned = false;
    player.CanDash = true;
    player.JumpInteractionPressed = false;

    player.RailSlide.RailExitMomentum = Vector3.zero;
    RailExitMomentumTimer.Stop();
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

    bool inRailGrace = !player.IsGrounded && RailGraceTimer.TimeLeft > 0f;
    float friction = inRailGrace ? 0f : (player.IsGrounded ? player.Friction : player.AirFriction);

    if (player.MoveInput == Vector2.zero)
    {
      if (inRailGrace)
        return;

      var move = player.MovementVector;
      move.x = QualityOfLife.PlayerFriction(move.x, friction, player.MoveInput);
      move.z = QualityOfLife.PlayerFriction(move.z, friction, player.MoveInput);
      player.MovementVector = move;
      return;
    }

    Vector3 direction = ILocomotionState<Player>.CalculateCameraDirection(player);

    if (inRailGrace)
    {
      float graceAccel = accel * 0.35f;
      var m = player.MovementVector;
      player.MovementVector = new Vector3(
        QualityOfLife.SmoothStepLerp(m.x, direction.x * speed, graceAccel),
        m.y,
        QualityOfLife.SmoothStepLerp(m.z, direction.z * speed, graceAccel)
      );

      // Rotação também mais lenta no ar pós-rail
      player.transform.rotation = Quaternion.Slerp(
        player.transform.rotation,
        Quaternion.LookRotation(direction),
        4f * Time.deltaTime
      );
      return;
    }

    player.transform.rotation = Quaternion.Slerp(
      player.transform.rotation,
      Quaternion.LookRotation(direction),
      10f * Time.deltaTime
    );

    var mv = player.MovementVector;
    player.MovementVector = new Vector3(
      QualityOfLife.SmoothStepLerp(mv.x, direction.x * speed, accel),
      mv.y,
      QualityOfLife.SmoothStepLerp(mv.z, direction.z * speed, accel)
    );
  }

  // ─── Helpers ──────────────────────────────────────────────────────────────

  private static float GetStatValue(Player player, StatType statType, float fallback)
  {
    player.Stats.TryGetNum(statType, out float value);
    return value > 0f ? value : fallback;
  }
}
