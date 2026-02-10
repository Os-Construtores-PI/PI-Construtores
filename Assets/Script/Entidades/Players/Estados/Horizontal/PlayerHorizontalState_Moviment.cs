using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerHorizontalStateMoviment : IState<PlayerContext>
{
    public int Priority => 5;

    public ActionType Type => ActionType.Move;

    public HashSet<ActionType> IncompatibleActions => new() {};

    public void Enter(PlayerContext context)
    {
    }

    public void Exit(PlayerContext context)
    {
    }

    public void FixedUpdate(PlayerContext context)
    {
        if(context.PlayerMoveInput == Vector2.zero) { context.PlayerHorizontalLayer.ChangeState(new PlayerHorizontalStateIdle(), context);};

        CinemachineCamera playerCamera = context.PlayerCamera;
        Transform playerTransform = context.EntityTransform;
        Vector3 playerMovementVector = context.PlayerMovementVector;
        float playerSpeed = context.PlayerSpeed;
        float playerAcceleration = context.PlayerAcceleration;
        Vector3 moveInput = context.PlayerMoveInput;

        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;
        forward.y = right.y = 0f;

        Vector3 playerDirection = forward.normalized * moveInput.y + right.normalized * moveInput.x;
        if(playerDirection == Vector3.zero) playerDirection = context.EntityTransform.forward;
        
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
