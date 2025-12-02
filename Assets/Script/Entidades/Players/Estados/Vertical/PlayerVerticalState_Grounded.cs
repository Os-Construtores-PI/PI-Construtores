using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : IState<PlayerContext>
{
    public ActionType Type => ActionType.Idle;
    public HashSet<ActionType> IncompatibleActions => new() {};

    private Timer exitTimer = new();
    private bool timerStarted = false;
    private float exitInterval = .3f;

    public void Enter(PlayerContext context)
    {
        Vector3 move = context.PlayerMovementVector;
        move.y = -1f;
        context.PlayerMovementVector = move;
    }

    public void Exit(PlayerContext context) { }

    public void FixedUpdate(PlayerContext context)
    {
        // Reseta o eixo Y do movimento
        Vector3 move = context.PlayerMovementVector;

        // Reseta jumps e dash
        context.PlayerCurrentJumpCount = 0;
        context.PlayerDashCurrent = 0;

        // Aplica atrito separadamente em X e Z
        move.x = QualityOfLife.PlayerFriction(move.x, context.PlayerFriction,context.PlayerMoveInput);
        move.z = QualityOfLife.PlayerFriction(move.z, context.PlayerFriction,context.PlayerMoveInput);
        context.PlayerMovementVector = move;
        if(!context.PlayerIsGrounded && !timerStarted)
        {
            exitTimer.Start(exitInterval);
            timerStarted = true;
        }
        if(timerStarted && context.PlayerIsGrounded)
        {
            exitTimer.Stop();
            timerStarted = false;
        }
        if(exitTimer.Tick(Time.deltaTime) &&  timerStarted)
        {
            context.PlayerVerticalLayer.ChangeState(new PlayerFallingState(), context);
        }
    }

    public void Update(PlayerContext context)
    {
        
    }
}
