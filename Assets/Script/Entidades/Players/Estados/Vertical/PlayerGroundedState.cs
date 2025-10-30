using UnityEngine;

public class PlayerGroundedState : IState<Player>
{
    public void Enter(Player entity)
    {
        Vector3 move = entity.MovementVector;
        move.y = 0;
        entity.MovementVector = move;
    }

    public void Exit(Player entity)
    {     
    }

    public void FixedUpdate(Player entity)
    {
        // Reseta o eixo Y do movimento
        Vector3 move = entity.MovementVector;

        // Reseta jumps e dash
        entity.currentJumpCount = 0;
        entity.dashCurrent = 0;

        // Aplica atrito separadamente em X e Z
        move.x = entity.ApplyFriction(move.x, entity.friction);
        move.z = entity.ApplyFriction(move.z, entity.friction);
        entity.MovementVector = move;  
    }

    public void Update(Player entity)
    {
        
    }
}
