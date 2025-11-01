using Unity.Cinemachine;
using UnityEngine;

public class PlayerMovimentState : IState<PlayerContext>
{
    public void Enter(PlayerContext context) { }

    public void Exit(PlayerContext context) { }

    public void FixedUpdate(PlayerContext context)
    {
        CinemachineCamera playerCamera = context.PlayerCamera;
        Transform playerTransform = context.PlayerTransform;
        Vector3 playerMovementVector = context.MovementVector;
        float playerSpeed = context.Speed;
        float playerAcceleration = context.Acceleration;
        Vector3 moveInput = context.MoveInput;

        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;
        forward.y = right.y = 0f;

        Vector3 playerDirection = forward.normalized * moveInput.y + right.normalized * moveInput.x;
        context.PlayerTransform.rotation = Quaternion.Slerp(
            playerTransform.rotation,
            Quaternion.LookRotation(playerDirection),
            10f * Time.deltaTime
        );
        context.Direction = playerDirection;

        playerMovementVector = new(
            QualityOfLife.SmoothLerp(
                playerMovementVector.x,
                playerDirection.x * playerSpeed,
                playerAcceleration
            ),
            playerMovementVector.y,
            playerMovementVector.z
        );
        playerMovementVector = new(
            playerMovementVector.x,
            playerMovementVector.y,
            QualityOfLife.SmoothLerp(
                playerMovementVector.z,
                playerDirection.z * playerSpeed,
                playerAcceleration
            )
        );
        context.MovementVector = playerMovementVector;
    }

    public void Update(PlayerContext context) { }
}
