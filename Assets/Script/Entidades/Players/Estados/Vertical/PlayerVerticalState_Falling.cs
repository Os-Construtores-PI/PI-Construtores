using System.Collections.Generic;
using UnityEngine;

public class PlayerFallingState : IState<PlayerContext>
{
    public ActionType Type => ActionType.Fall;

    public HashSet<ActionType> IncompatibleActions => new() {};

    public void Enter(PlayerContext context)
    {
    }

    public void Exit(PlayerContext context)
    {
    }

    public void FixedUpdate(PlayerContext context)
    {
        if (context.OverrideGlobal || context.OverrideVertical) { return; }
        Vector3 move = context.MovementVector;
        move.x = QualityOfLife.PlayerFriction(move.x, context.AirFriction, context.MoveInput);
        move.z = QualityOfLife.PlayerFriction(move.z, context.AirFriction, context.MoveInput);
        move = new(move.x, move.y + context.Gravity * Time.deltaTime, move.z);
        context.MovementVector = move;
        if(context.IsGrounded && context.MovementVector.y < 0f)
        {
            context.VerticalLayer.ChangeState(new PlayerGroundedState(), context);
        }
    }

    public void Update(PlayerContext context)
    {
    }
}
