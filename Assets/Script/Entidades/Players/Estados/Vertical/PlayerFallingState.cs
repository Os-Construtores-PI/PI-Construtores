using UnityEngine;

public class PlayerFallingState : IState<Player>
{
    public void Enter(Player entity)
    {
    }

    public void Exit(Player entity)
    {
    }

    public void FixedUpdate(Player entity)
    {
        Vector3 move = entity.MovementVector;
        move.x = entity.ApplyFriction(move.x, entity.airFriction);
        move.z = entity.ApplyFriction(move.z, entity.airFriction);
        move = new(move.x, move.y + entity.gravity * Time.deltaTime, move.z);
        entity.MovementVector = move;
    }

    public void Update(Player entity)
    {
    }
}
