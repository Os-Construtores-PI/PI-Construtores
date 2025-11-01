using UnityEngine;

public class PlayerJumpingState : IState<PlayerContext>
{
    public void Enter(PlayerContext context)
    {
        Vector3 move = context.MovementVector;
        float multiplier = Mathf.Max(1f - 0.3f * context.CurrentJumpCount, 0.2f);
        if (context.TouchingWall) // se estiver na parede → usa vetor mais horizontal
        {
            float horizontalBias = 6.5f; // quanto maior, mais horizontal
            Vector3 jumpDir = (Vector3.up + context.LastWallNormal * horizontalBias).normalized;
            move = context.JumpForce * context.WallJumpMultiplier * jumpDir;
            context.TouchingWall = false; // evita repetir
        }
        else // pulo normal
        {
            move = new(move.x, context.JumpForce * multiplier, move.z);
        }
        context.CurrentJumpCount++;
        context.MovementVector = move;
    }

    public void Exit(PlayerContext entity) { }

    public void FixedUpdate(PlayerContext entity) { }

    public void Update(PlayerContext entity) { }
}
