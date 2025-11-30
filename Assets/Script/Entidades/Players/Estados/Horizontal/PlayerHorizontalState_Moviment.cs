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
        context.PlayerAnimator.SetTrigger(Constants.AnimatorTriggerNames.Walk);
    }

    public void Exit(PlayerContext context)
    {
        context.PlayerAnimator.ResetTrigger(Constants.AnimatorTriggerNames.Walk);
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
        context.EntityTransform.rotation = Quaternion.Slerp(
            playerTransform.rotation,
            Quaternion.LookRotation(playerDirection),
            10f * Time.deltaTime
        );
        context.PlayerDirection = playerDirection;

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
        context.PlayerMovementVector = playerMovementVector;
    }

    public void Update(PlayerContext context) { }
}
