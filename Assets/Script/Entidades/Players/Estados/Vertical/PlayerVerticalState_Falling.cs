using System.Collections.Generic;
using UnityEngine;

public class PlayerFallingState : IState<PlayerContext>
{
    public ActionType Type => ActionType.Fall;
    public HashSet<ActionType> IncompatibleActions => new() { };

    // ajustes finos
    private float _gravityUpMultiplier = 2.2f; // sobe rápido, perde força cedo
    private float _gravityDownMultiplier = 0.6f; // cai mais lento
    private float _maxFallSpeed = -26f; // limite da queda

    public void Enter(PlayerContext context)
    {
        _gravityUpMultiplier = context.PlayerGravityUpMultiplier;
        _gravityDownMultiplier = context.PlayerGravityDownMultiplier;
        _maxFallSpeed = context.PlayerMaxFallSpeed;
    }

    public void Exit(PlayerContext context) { }

    public void FixedUpdate(PlayerContext context)
    {
        if (context.OverrideGlobal || context.OverrideVertical)
            return;

        Vector3 move = context.PlayerMovementVector;

        // atrito no ar
        move.x = QualityOfLife.PlayerFriction(
            move.x,
            context.PlayerAirFriction,
            context.PlayerMoveInput
        );
        move.z = QualityOfLife.PlayerFriction(
            move.z,
            context.PlayerAirFriction,
            context.PlayerMoveInput
        );

        float gravityMultiplier =
            move.y > 0f
                ? _gravityUpMultiplier // SUBIDA
                : _gravityDownMultiplier; // DESCIDA

        move.y += context.PlayerGravity * gravityMultiplier * Time.deltaTime;

        // limite da queda
        if (move.y < _maxFallSpeed)
        {
            move.y = _maxFallSpeed;
        }

        context.PlayerMovementVector = move;

        if (context.PlayerIsGrounded && move.y < 0f)
        {
            context.PlayerVerticalLayer.ChangeState(new PlayerGroundedState(), context);
        }
    }

    public void Update(PlayerContext context) { }
}
