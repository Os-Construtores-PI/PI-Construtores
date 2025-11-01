using UnityEngine;

public class PlayerFallingState : IState<PlayerContext>
{
    public void Enter(PlayerContext context)
    {
    }

    public void Exit(PlayerContext context)
    {
    }

    public void FixedUpdate(PlayerContext context)
    {
        Vector3 move = context.MovementVector;
        move.x = QualityOfLife.PlayerFriction(move.x, context.AirFriction, context.MoveInput);
        move.z = QualityOfLife.PlayerFriction(move.z, context.AirFriction, context.MoveInput);
        move = new(move.x, move.y + context.Gravity * Time.deltaTime, move.z);
        context.MovementVector = move;
    }

    public void Update(PlayerContext context)
    {
    }
}
