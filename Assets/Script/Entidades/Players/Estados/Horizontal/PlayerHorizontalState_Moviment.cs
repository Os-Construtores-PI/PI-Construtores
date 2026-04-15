using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerHorizontalStateMoviment : IState<Player>
{
  public int Priority => 5;

  public ActionType Type => ActionType.Move;

  public HashSet<ActionType> IncompatibleActions => new() { };

  private readonly Dictionary<bool, float> _speeds = new();
  private readonly Dictionary<bool, float> _accelerations = new();

  public void Enter(Player player)
  {
    _speeds[false] = player.Speed;
    _speeds[true] = player.RunningSpeed;
    _accelerations[false] = player.Acceleration;
    _accelerations[true] = player.AccelerationRunning;
  }

  public void Exit(Player player) { }

  public void FixedUpdate(Player player)
  {
    if (player.MoveInput == Vector2.zero)
    {
      player.HorizontalLayer.ChangeState(new PlayerHorizontalStateIdle(), player);
      return;
    }

    Vector3 playerDirection;

    // Modo normal: baseado na câmera
    CinemachineCamera playerCamera = player.CinemachineCamera;
    Vector3 moveInput = player.MoveInput;
    Vector3 camForward = playerCamera.transform.forward;
    Vector3 camRight = playerCamera.transform.right;
    camForward.y = camRight.y = 0f;

    playerDirection = camForward.normalized * moveInput.y + camRight.normalized * moveInput.x;
    if (playerDirection == Vector3.zero)
      playerDirection = player.transform.forward;

    player.transform.rotation = Quaternion.Slerp(
      player.transform.rotation,
      Quaternion.LookRotation(playerDirection),
      10f * Time.deltaTime
    );

    player.Direction = playerDirection;

    float playerSpeed = _speeds[player.IsRunning];
    float playerAcceleration = _accelerations[player.IsRunning];
    Vector3 playerMovementVector = player.MovementVector;

    player.MovementVector = new Vector3(
      QualityOfLife.SmoothStepLerp(
        playerMovementVector.x,
        playerDirection.x * playerSpeed,
        playerAcceleration
      ),
      playerMovementVector.y,
      QualityOfLife.SmoothStepLerp(
        playerMovementVector.z,
        playerDirection.z * playerSpeed,
        playerAcceleration
      )
    );
  }

  public void Update(Player player) { }
}
