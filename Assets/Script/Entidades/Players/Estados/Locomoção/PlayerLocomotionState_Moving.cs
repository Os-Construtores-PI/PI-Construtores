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

  // ─── Estado Interno ───────────────────────────────────────────────────────
  private bool _wasGrounded;
  private bool _justLanded = false;
  private bool _requestBounce = false;

  // ─── Enter / Exit ─────────────────────────────────────────────────────────

  public void Enter(Player player)
  {
    _wasGrounded = player.Motor.IsGrounded;
    _justLanded = false;
    _requestBounce = false;

    player.Stats.AddStat(StatType.RunSpeedMultiplier, player.RunSpeedMultiplier);
    player.Stats.AddStat(StatType.RunAccelMultiplier, player.RunAccelMultiplier);

    if (player.Motor.IsGrounded)
      ApplyLandingLogic(player);
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
    if (!player.Motor.IsGrounded && _wasGrounded)
    {
      _wasGrounded = false;
    }

    if (player.Motor.IsGrounded && !_wasGrounded)
    {
      _wasGrounded = true;
      _justLanded = true;

      ApplyLandingLogic(player);
    }

    if (!player.Motor.IsGrounded && player.ActionLayer.GetActive<PlayerActionStateJump>() == null)
    {
      HandleCoyoteTime(player);
    }

    HandleRotation(player);
  }

  // ─── Landing Logic ────────────────────────────────────────────────────────

  private void ApplyLandingLogic(Player player)
  {
    player.CurrentJumpCount = 0;
    player.CurrentDashCount = 0;
    player.IsImpulsioned = false;
    player.CanDash = true;
    player.JumpInteractionPressed = false;

    if (player.GroundSlamImpactSpeed > 0f)
    {
      _requestBounce = true;
    }
  }

  // ─── Rotação (Visual) ─────────────────────────────────────────────────────

  private void HandleRotation(Player player)
  {
    if (player.MoveInput == Vector2.zero)
      return;

    Vector3 direction = ILocomotionState<Player>.CalculateCameraDirection(player);
    player.transform.rotation = Quaternion.Slerp(
      player.transform.rotation,
      Quaternion.LookRotation(direction),
      10f * Time.fixedDeltaTime
    );
  }

  // ─── Coyote Time ──────────────────────────────────────────────────────────

  private void HandleCoyoteTime(Player player)
  {
    if (!_coyoteStarted)
    {
      _coyoteTimer.Start(CoyoteInterval);
      _coyoteStarted = true;
    }

    if (_coyoteTimer.Tick(Time.fixedDeltaTime))
    {
      _coyoteStarted = false;
    }
  }

  // ─── KCC: CalculateKCCVelocity ────────────────────────────────────────────

  public void CalculateKCCVelocity(Player player, ref Vector3 currentVelocity, float deltaTime)
  {
    if (_justLanded)
    {
      _justLanded = false;
      currentVelocity.y = -1f;
    }

    float speedMult = GetStatValue(player, StatType.RunSpeedMultiplier, player.RunSpeedMultiplier);
    float accelMult = GetStatValue(player, StatType.RunAccelMultiplier, player.RunAccelMultiplier);

    float speed = player.IsRunning ? player.Speed * speedMult : player.Speed;
    float accel = player.IsRunning
      ? (player.Motor.IsGrounded ? player.Acceleration * accelMult : player.Acceleration)
      : (player.Motor.IsGrounded ? player.Acceleration : player.Acceleration);
    float friction = player.Motor.IsGrounded ? player.Friction : player.AirFriction;

    Vector3 horizontalVel = new(currentVelocity.x, 0f, currentVelocity.z);

    bool hasInput = player.MoveInput.sqrMagnitude > 0.1f;

    if (!hasInput)
    {
      float holspeed = horizontalVel.magnitude;
      float newSpeed = Mathf.MoveTowards(holspeed, 0f, friction * deltaTime);

      if (newSpeed <= 0.5f)
      {
        horizontalVel = Vector3.zero;
      }
      else
      {
        horizontalVel = horizontalVel.normalized * newSpeed;
      }
    }
    else
    {
      Vector3 direction = ILocomotionState<Player>.CalculateCameraDirection(player);

      if (direction.sqrMagnitude > 0.01f)
        player.Direction = direction;

      horizontalVel.x = QualityOfLife.SmoothStepLerp(horizontalVel.x, direction.x * speed, accel);
      horizontalVel.z = QualityOfLife.SmoothStepLerp(horizontalVel.z, direction.z * speed, accel);
    }

    if (!player.Motor.IsGrounded && player.IsImpulsioned && player.MoveInput == Vector2.zero) { }

    if (player.Motor.IsGrounded)
    {
      currentVelocity.y = -0.1f;
    }

    ILocomotionState<Player>.ApplyGravity(ref currentVelocity, player, deltaTime);

    currentVelocity = new Vector3(horizontalVel.x, currentVelocity.y, horizontalVel.z);
  }

  // ─── Helpers ──────────────────────────────────────────────────────────────

  private static float GetStatValue(Player player, StatType statType, float fallback)
  {
    player.Stats.TryGetNum(statType, out float value);
    return value > 0f ? value : fallback;
  }
}
