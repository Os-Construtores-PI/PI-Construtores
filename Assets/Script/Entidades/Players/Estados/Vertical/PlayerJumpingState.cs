using UnityEngine;

public class PlayerJumpingState : IState<Player>
{
    public void Enter(Player entity)
    {
        Vector3 move = entity.MovementVector;
        float multiplier = Mathf.Max(1f - 0.3f * entity.currentJumpCount, 0.2f);
        if (entity.touchingWall) // se estiver na parede → usa vetor mais horizontal
        {
            float horizontalBias = 6.5f; // quanto maior, mais horizontal
            Vector3 jumpDir = (Vector3.up + entity.LastWallNormal * horizontalBias).normalized;
            move = entity.JumpForce * entity.wallJumpMultiplier * jumpDir;
            entity.touchingWall = false; // evita repetir
        }
        else // pulo normal
        {
            move = new(move.x, entity.JumpForce * multiplier, move.z);
        }
        entity.currentJumpCount++;
        entity.MovementVector = move;
    }

    public void Exit(Player entity)
    {
        
    }

    public void FixedUpdate(Player entity)
    {
        
    }

    public void Update(Player entity)
    {
    }
}
