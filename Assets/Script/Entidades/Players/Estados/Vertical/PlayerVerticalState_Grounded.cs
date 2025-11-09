using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : IState<PlayerContext>
{
    public ActionType Type => ActionType.Idle;

    public HashSet<ActionType> IncompatibleActions => new() {};

    public void Enter(PlayerContext context)
    {
        Vector3 move = context.MovementVector;
        move.y = 0;
        context.MovementVector = move;
    }

    public void Exit(PlayerContext context) { }

    public void FixedUpdate(PlayerContext context)
    {
        // Reseta o eixo Y do movimento
        Vector3 move = context.MovementVector;

        // Reseta jumps e dash
        context.CurrentJumpCount = 0;
        context.DashCurrent = 0;

        // Aplica atrito separadamente em X e Z
        move.x = QualityOfLife.PlayerFriction(move.x, context.Friction,context.MoveInput);
        move.z = QualityOfLife.PlayerFriction(move.z, context.Friction,context.MoveInput);
        context.MovementVector = move;
        if(!context.IsGrounded)
        {
            context.VerticalLayer.ChangeState(new PlayerFallingState(), context);
        }
    }

    public void Update(PlayerContext context) { }
}
