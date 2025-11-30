using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpingState : IState<PlayerContext>
{
    public ActionType Type => ActionType.Jump;

    public HashSet<ActionType> IncompatibleActions => new() {};

    public void Enter(PlayerContext context)
    {
        if (context.PlayerCurrentJumpCount < context.PlayerMaxJumpCount)
        {
            Vector3 move = context.PlayerMovementVector;
            float multiplier = Mathf.Max(1f - 0.3f * context.PlayerCurrentJumpCount, 0.2f);
            if (context.PlayerTouchingWall) // se estiver na parede → usa vetor mais horizontal
            {
                float horizontalBias = 6.5f; // quanto maior, mais horizontal
                Vector3 jumpDir = (Vector3.up + context.PlayerLastWallNormal * horizontalBias).normalized;
                move = context.PlayerJumpForce * context.PlayerWallJumpMultiplier * jumpDir;
                context.PlayerTouchingWall = false; // evita repetir
            }
            else // pulo normal
            {
                move = new(move.x, context.PlayerJumpForce * multiplier, move.z);
            }
            context.PlayerCurrentJumpCount++;
            context.PlayerMovementVector = move;
            context.PlayerAnimator.SetTrigger(Constants.AnimatorTriggerNames.Jump);
        }
        context.PlayerVerticalLayer.ChangeState(new PlayerFallingState(), context);
    }

    public void Exit(PlayerContext context) { }

    public void FixedUpdate(PlayerContext context) { }

    public void Update(PlayerContext context) { }
}
