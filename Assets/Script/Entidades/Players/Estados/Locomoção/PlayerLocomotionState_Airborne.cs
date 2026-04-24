using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotionStateAirborne : IState<Player>
{
  public ActionType Type => ActionType.Fall;
  public HashSet<ActionType> IncompatibleActions => new() { };

  public void Enter(Player player) { }

  public void Exit(Player player) { }

  public void Update(Player player)
  {
    if (player.JumpInputPressed && player.CurrentJumpCount < player.MaxJumpCount)
    {
      ApplyAirJump(player);
    }
  }

  public void FixedUpdate(Player player)
  {
    Vector3 move = player.MovementVector;

    // 1. Gravidade
    float gravityMult = move.y > 0f ? player.GravityUpMultiplier : player.GravityDownMultiplier;
    move.y += player.GravityValue * gravityMult * Time.deltaTime;
    if (move.y < player.MaxFallSpeed)
      move.y = player.MaxFallSpeed;
    player.MovementVector = move;

    // 2. Movimento Horizontal e Atrito
    if (player.MoveInput == Vector2.zero && !player.IsImpulsioned)
    {
      move.x = QualityOfLife.PlayerFriction(move.x, player.AirFriction, player.MoveInput);
      move.z = QualityOfLife.PlayerFriction(move.z, player.AirFriction, player.MoveInput);
      player.MovementVector = move;
    }
    else if (player.MoveInput != Vector2.zero)
    {
      ApplyAirMovement(player);
    }

    // 3. Landing
    if (player.IsGrounded && player.MovementVector.y <= 0f)
      player.LocomotionLayer.ChangeState(new PlayerLocomotionStateGrounded(), player);
  }

  private void ApplyAirJump(Player player)
  {
    player.JumpInputPressed = false;
    player.AnimatorComponent.SetTrigger(Constants.AnimatorTriggerNames.DoubleJump);
    Vector3 move = player.MovementVector;
    move.y = player.JumpForce * (1 + (player.CurrentJumpCount * 0.35f));
    player.CurrentJumpCount++;
    player.EffectsWorker.PlayEffect(Constants.EffectsNames.Player.Jump, 1);
    player.MovementVector = move;
  }

  private void ApplyAirMovement(Player player)
  {
    Vector3 camForward = player.CinemachineCamera.transform.forward;
    Vector3 camRight = player.CinemachineCamera.transform.right;
    camForward.y = camRight.y = 0f;
    player.Direction = CalculateCameraDirection(player);
    player.transform.rotation = Quaternion.Slerp(
      player.transform.rotation,
      Quaternion.LookRotation(player.Direction),
      10f * Time.deltaTime
    );

    float targetSpeed = player.IsRunning ? player.RunningSpeed : player.Speed;
    player.MovementVector = new Vector3(
      QualityOfLife.SmoothStepLerp(
        player.MovementVector.x,
        player.Direction.x * targetSpeed,
        player.Acceleration
      ),
      player.MovementVector.y,
      QualityOfLife.SmoothStepLerp(
        player.MovementVector.z,
        player.Direction.z * targetSpeed,
        player.Acceleration
      )
    );
  }

  private Vector3 CalculateCameraDirection(Player player)
  {
    Vector3 camForward = player.CinemachineCamera.transform.forward;
    Vector3 camRight = player.CinemachineCamera.transform.right;
    camForward.y = camRight.y = 0f;
    return (
      camForward.normalized * player.MoveInput.y + camRight.normalized * player.MoveInput.x
    ).normalized;
  }
}
