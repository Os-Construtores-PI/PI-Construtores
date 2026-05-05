using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotionStateAirborne : ILocomotionState<Player>
{
  public ActionType Type => ActionType.Fall;
  public HashSet<ActionType> IncompatibleActions => new() { };

  // ─── Boost aéreo ──────────────────────────────────────────────────────────
  // Metade da taxa do Grounded: permite manter o boost por mais tempo no ar,
  // sem anular o custo — correr no ar gasta recurso, mas de forma mais suave.
  private const float AirBoostDrainRate = 7.5f;
  private const float AirBoostSpeedFactor = 0.65f;
  private bool _isUsingBoost;

  // ─── Enter / Exit ─────────────────────────────────────────────────────────
  public void Enter(Player player) { }

  public void Exit(Player player)
  {
    _isUsingBoost = false;
  }

  // ─── Update ───────────────────────────────────────────────────────────────
  public void Update(Player player)
  {
    if (player.JumpInputPressed && player.CurrentJumpCount < player.MaxJumpCount)
      ApplyAirJump(player);

    _isUsingBoost = player.DashSlashBoostButton.Value > 0f && player.IsRunning;

    if (_isUsingBoost)
      DrainAirBoost(player);
  }

  // ─── FixedUpdate ──────────────────────────────────────────────────────────
  public void FixedUpdate(Player player)
  {
    ApplyGravity(player);
    HandleAirMovement(player);

    if (player.IsGrounded && player.MovementVector.y <= 0f)
      player.LocomotionLayer.ChangeState(player.GroundedS, player);
  }

  // ─── Gravidade ────────────────────────────────────────────────────────────
  private static void ApplyGravity(Player player)
  {
    Vector3 move = player.MovementVector;
    float gravityMult = move.y > 0f ? player.GravityUpMultiplier : player.GravityDownMultiplier;

    move.y += player.GravityValue * gravityMult * Time.deltaTime;
    if (move.y < player.MaxFallSpeed)
      move.y = player.MaxFallSpeed;

    player.MovementVector = move;
  }

  // ─── Movimento horizontal no ar ───────────────────────────────────────────
  private void HandleAirMovement(Player player)
  {
    if (player.MoveInput == Vector2.zero && !player.IsImpulsioned)
    {
      Vector3 move = player.MovementVector;
      move.x = QualityOfLife.PlayerFriction(move.x, player.AirFriction, player.MoveInput);
      move.z = QualityOfLife.PlayerFriction(move.z, player.AirFriction, player.MoveInput);
      player.MovementVector = move;
      return;
    }

    if (player.MoveInput == Vector2.zero)
      return;

    float targetSpeed = player.IsRunning
      ? player.RunningSpeed
        * player.DashSlashBoostButton.SpeedMultiplier
        * (_isUsingBoost ? AirBoostSpeedFactor : 1f)
      : player.Speed;

    // Reutiliza helper estático definido em ILocomotionState
    ILocomotionState<Player>.ApplyHorizontalMovement(player, targetSpeed, player.Acceleration);
  }

  // ─── Air Jump ─────────────────────────────────────────────────────────────
  private static void ApplyAirJump(Player player)
  {
    player.JumpInputPressed = false;
    player.AnimatorComponent.SetTrigger(Constants.AnimatorTriggerNames.DoubleJump);

    Vector3 move = player.MovementVector;
    move.y = player.JumpForce * (1f + player.CurrentJumpCount * 0.35f);
    player.CurrentJumpCount++;
    player.EffectsWorker.PlayEffect(Constants.EffectsNames.Player.Jump, 1);
    player.MovementVector = move;
  }

  // ─── Boost aéreo ─────────────────────────────────────────────────────────
  private void DrainAirBoost(Player player)
  {
    if (player.DashSlashBoostButton.Value <= 0f)
    {
      _isUsingBoost = false;
      return;
    }

    player.DashSlashBoostButton.Value -= AirBoostDrainRate * Time.deltaTime;
  }
}
