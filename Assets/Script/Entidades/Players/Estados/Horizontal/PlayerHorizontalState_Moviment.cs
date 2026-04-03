using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerHorizontalStateMoviment : IState<PlayerContext>
{
  public int Priority => 5;

  public ActionType Type => ActionType.Move;

  public HashSet<ActionType> IncompatibleActions => new() { };

  private Dictionary<bool, float> _speeds = new();
  private Dictionary<bool, float> _accelerations = new();

  public void Enter(PlayerContext context)
  {
    _speeds[false] = context.PlayerSpeed;
    _speeds[true] = context.PlayerRunningSpeed;
    _accelerations[false] = context.PlayerAcceleration;
    _accelerations[true] = context.PlayerRunningAcceleration;
  }

  public void Exit(PlayerContext context) { }

  public void FixedUpdate(PlayerContext context)
  {
    if (context.PlayerMoveInput == Vector2.zero)
    {
      context.PlayerHorizontalLayer.ChangeState(new PlayerHorizontalStateIdle(), context);
      return;
    }

    Vector3 playerDirection;

    if (context.PlayerLockedTarget != null)
    {
      // Modo strafe: orientação baseada no alvo, câmera não interfere
      Vector3 toTarget =
        context.PlayerLockedTarget.transform.position - context.EntityTransform.position;
      toTarget.y = 0f;
      Vector3 forward = toTarget.normalized;
      Vector3 right = Vector3.Cross(Vector3.up, forward);

      Vector3 moveInput = context.PlayerMoveInput;
      playerDirection = forward * moveInput.y + right * moveInput.x;

      // Player sempre enfrenta o alvo, independente do movimento
      context.EntityTransform.rotation = Quaternion.Slerp(
        context.EntityTransform.rotation,
        Quaternion.LookRotation(forward),
        10f * Time.deltaTime
      );
    }
    else
    {
      // Modo normal: baseado na câmera
      CinemachineCamera playerCamera = context.PlayerCamera;
      Vector3 moveInput = context.PlayerMoveInput;
      Vector3 camForward = playerCamera.transform.forward;
      Vector3 camRight = playerCamera.transform.right;
      camForward.y = camRight.y = 0f;

      playerDirection = camForward.normalized * moveInput.y + camRight.normalized * moveInput.x;
      if (playerDirection == Vector3.zero)
        playerDirection = context.EntityTransform.forward;

      context.EntityTransform.rotation = Quaternion.Slerp(
        context.EntityTransform.rotation,
        Quaternion.LookRotation(playerDirection),
        10f * Time.deltaTime
      );
    }

    context.PlayerDirection = playerDirection;

    float playerSpeed = _speeds[context.PlayerIsRunning];
    float playerAcceleration = _accelerations[context.PlayerIsRunning];
    Vector3 playerMovementVector = context.PlayerMovementVector;

    context.PlayerMovementVector = new Vector3(
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

  public void Update(PlayerContext context) { }
}
