using Unity.Cinemachine;
using UnityEngine;

public class PlayerMovimentState : IState<Player>
{
    public void Enter(Player entity)
    {
    }

    public void Exit(Player entity)
    {
    }

    public void FixedUpdate(Player entity)
    {
        if (entity.Cinemachinecamera == null || entity.MoveInput == Vector2.zero) return;

        CinemachineCamera playerCamera = entity.Cinemachinecamera;
        Transform playerTransform = entity.transform;
        Vector3 playerMovementVector = entity.MovementVector;
        float playerSpeed = entity.Speed;
        float playerAcceleration = entity.Acceleration;
        Vector3 moveInput = entity.MoveInput;

        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;
        forward.y = right.y = 0f;

        Vector3 playerDirection = forward.normalized * moveInput.y + right.normalized * moveInput.x;
        entity.transform.rotation = Quaternion.Slerp(playerTransform.rotation, Quaternion.LookRotation(playerDirection), 10f * Time.deltaTime);
        entity.Direction = playerDirection;

        playerMovementVector = new(entity.SmoothLerp(playerMovementVector.x, playerDirection.x * playerSpeed, playerAcceleration), playerMovementVector.y, playerMovementVector.z);
        playerMovementVector = new(playerMovementVector.x, playerMovementVector.y, entity.SmoothLerp(playerMovementVector.z, playerDirection.z * playerSpeed, playerAcceleration));
        entity.MovementVector = playerMovementVector;

    }

    public void Update(Player entity)
    {
    }
}
