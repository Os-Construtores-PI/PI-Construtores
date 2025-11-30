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
        Vector3 move = context.PlayerMovementVector;
        move.x = QualityOfLife.PlayerFriction(move.x, context.PlayerAirFriction, context.PlayerMoveInput);
        move.z = QualityOfLife.PlayerFriction(move.z, context.PlayerAirFriction, context.PlayerMoveInput);
        move = new(move.x, move.y + context.PlayerGravity * Time.deltaTime, move.z);
        if(move.y < -2f)
        {
            context.PlayerAnimator.ResetTrigger(Constants.AnimatorTriggerNames.Jump);
        }
        context.PlayerMovementVector = move;
        if(context.PlayerIsGrounded && context.PlayerMovementVector.y < 0f)
        {
            context.PlayerVerticalLayer.ChangeState(new PlayerGroundedState(), context);
        }
    }

    public void Update(PlayerContext context)
    {
    }
}
