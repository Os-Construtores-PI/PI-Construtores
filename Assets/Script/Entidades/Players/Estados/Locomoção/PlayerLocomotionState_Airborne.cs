using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotionStateAirborne : ILocomotionState<Player>
{
  public ActionType Type => ActionType.Fall;
  public HashSet<ActionType> IncompatibleActions => new() { };

  // ─── Enter / Exit ─────────────────────────────────────────────────────────
  public void Enter(Player player) { }

  public void Exit(Player player) { }

  // ─── Update ───────────────────────────────────────────────────────────────
  public void Update(Player player)
  {
    if (player.JumpInputPressed && player.CurrentJumpCount < player.MaxJumpCount)
      player.ActionLayer.PushState(player.Jump, player);
  }

  // ─── FixedUpdate ──────────────────────────────────────────────────────────
  public void FixedUpdate(Player player)
  {
    ILocomotionState<Player>.ApplyGravity(player);
    HandleAirMovement(player);

    if (player.IsGrounded && player.MovementVector.y <= 0f)
      player.LocomotionLayer.ChangeState(player.GroundedS, player);
  }

  // ─── Gravidade ────────────────────────────────────────────────────────────

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

    float speed = player.IsRunning ? player.RunningSpeed : player.Speed;
    ILocomotionState<Player>.ApplyHorizontalMovement(player, speed, player.Acceleration);
  }
}
