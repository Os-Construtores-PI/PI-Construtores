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

    if (player.LockedTarget != null)
    {
      // Modo strafe: orientação baseada no alvo, câmera não interfere
      Vector3 toTarget = player.LockedTarget.transform.position - player.transform.position;
      toTarget.y = 0f;
      Vector3 forward = toTarget.normalized;
      Vector3 right = Vector3.Cross(Vector3.up, forward);

      Vector3 moveInput = player.MoveInput;
      playerDirection = forward * moveInput.y + right * moveInput.x;

      // Player sempre enfrenta o alvo, independente do movimento
      player.transform.rotation = Quaternion.Slerp(
        player.transform.rotation,
        Quaternion.LookRotation(forward),
        10f * Time.deltaTime
      );
    }
    else
    {
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
    }

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
