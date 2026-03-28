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
        }
        ;

        CinemachineCamera playerCamera = context.PlayerCamera;
        Transform playerTransform = context.EntityTransform;
        Vector3 playerMovementVector = context.PlayerMovementVector;
        float playerSpeed = _speeds[context.PlayerIsRunning];
        float playerAcceleration = _accelerations[context.PlayerIsRunning];
        Vector3 moveInput = context.PlayerMoveInput;

        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;
        forward.y = right.y = 0f;

        Vector3 playerDirection = forward.normalized * moveInput.y + right.normalized * moveInput.x;
        if (playerDirection == Vector3.zero)
            playerDirection = context.EntityTransform.forward;

        context.EntityTransform.rotation = Quaternion.Slerp(
            playerTransform.rotation,
            Quaternion.LookRotation(playerDirection),
            10f * Time.deltaTime
        );
        context.PlayerDirection = playerDirection;

        playerMovementVector = new(
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
        context.PlayerMovementVector = playerMovementVector;
    }

    public void Update(PlayerContext context) { }
}
