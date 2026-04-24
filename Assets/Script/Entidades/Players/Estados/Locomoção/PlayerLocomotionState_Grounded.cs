using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerLocomotionStateGrounded : IState<Player>
{
  public ActionType Type => ActionType.Move;
  public HashSet<ActionType> IncompatibleActions => new() { };

  private readonly Timer exitTimer = new();
  private bool timerStarted = false;
  private readonly float exitInterval = .3f;

  private readonly Dictionary<bool, float> _speeds = new();
  private readonly Dictionary<bool, float> _accelerations = new();

  public void Enter(Player player)
  {
    _speeds[false] = player.Speed;
    _speeds[true] = player.RunningSpeed;
    _accelerations[false] = player.Acceleration;
    _accelerations[true] = player.AccelerationRunning;

    player.CurrentJumpCount = 0;
    player.CurrentDashCount = 0;
    player.IsImpulsioned = false;

    Vector3 move = player.MovementVector;
    move.y = -1f;
    player.MovementVector = move;
  }

  public void Exit(Player player) => exitTimer.Stop();

  public void Update(Player player)
  {
    if (player.JumpInputPressed && player.CurrentJumpCount < player.MaxJumpCount)
    {
      ExecuteJump(player);
    }
  }

  public void FixedUpdate(Player player)
  {
    if (!player.IsGrounded && !timerStarted)
    {
      exitTimer.Start(exitInterval);
      timerStarted = true;
    }

    if (timerStarted && (player.IsGrounded || exitTimer.Tick(Time.deltaTime)))
    {
      if (!player.IsGrounded)
        player.LocomotionLayer.ChangeState(player.AirborneS, player);
      timerStarted = false;
    }

    HandleHorizontalMovement(player);
  }

  private void ExecuteJump(Player player)
  {
    Vector3 move = player.MovementVector;
    player.JumpInputPressed = false;
    if (player.CurrentJumpCount != 0)
      player.AnimatorComponent.SetTrigger(Constants.AnimatorTriggerNames.DoubleJump);

    float multiplier = 1 + (player.CurrentJumpCount * 0.35f);

    if (player.TouchingWall)
    {
      float horizontalBias = 6.5f;
      Vector3 jumpDir = (Vector3.up + player.LastWallNormal * horizontalBias).normalized;
      move = player.JumpForce * player.WallJumpMultiplier * jumpDir;
      player.TouchingWall = false;
    }
    else
    {
      move.y += player.JumpForce * multiplier;
    }

    player.CurrentJumpCount++;
    player.EffectsWorker.PlayEffect(Constants.EffectsNames.Player.Jump, 1);
    player.MovementVector = move;

    player.LocomotionLayer.ChangeState(player.AirborneS, player);
  }

  private void HandleHorizontalMovement(Player player)
  {
    Vector3 move = player.MovementVector;

    if (player.MoveInput == Vector2.zero)
    {
      move.x = QualityOfLife.PlayerFriction(move.x, player.Friction, player.MoveInput);
      move.z = QualityOfLife.PlayerFriction(move.z, player.Friction, player.MoveInput);
      player.MovementVector = move;
      return;
    }

    Vector3 playerDirection = CalculateCameraDirection(player);
    player.transform.rotation = Quaternion.Slerp(
      player.transform.rotation,
      Quaternion.LookRotation(playerDirection),
      10f * Time.deltaTime
    );

    float speed = _speeds[player.IsRunning];
    float accel = _accelerations[player.IsRunning];

    player.MovementVector = new Vector3(
      QualityOfLife.SmoothStepLerp(move.x, playerDirection.x * speed, accel),
      move.y,
      QualityOfLife.SmoothStepLerp(move.z, playerDirection.z * speed, accel)
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
