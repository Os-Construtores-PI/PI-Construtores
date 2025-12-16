using System.Collections.Generic;
using UnityEngine;

public class PlayerFallingState : IState<PlayerContext>
{
    public ActionType Type => ActionType.Fall;
    public HashSet<ActionType> IncompatibleActions => new() { };

    // ajustes finos
    private const float GravityUpMultiplier   = 2.2f; // sobe rápido, perde força cedo
    private const float GravityDownMultiplier = 0.6f; // cai mais lento
    private const float MaxFallSpeed          = -26f; // limite da queda

    public void Enter(PlayerContext context) { }

    public void Exit(PlayerContext context) { }

    public void FixedUpdate(PlayerContext context)
    {
        if (context.OverrideGlobal || context.OverrideVertical) return;

        Vector3 move = context.PlayerMovementVector;

        // atrito no ar
        move.x = QualityOfLife.PlayerFriction(move.x, context.PlayerAirFriction, context.PlayerMoveInput);
        move.z = QualityOfLife.PlayerFriction(move.z, context.PlayerAirFriction, context.PlayerMoveInput);

        float gravityMultiplier = move.y > 0f
            ? GravityUpMultiplier     // SUBIDA
            : GravityDownMultiplier;  // DESCIDA

        move.y += context.PlayerGravity * gravityMultiplier * Time.deltaTime;

        // limite da queda
        if (move.y < MaxFallSpeed)
            move.y = MaxFallSpeed;

        context.PlayerMovementVector = move;

        if (context.PlayerIsGrounded && move.y < 0f)
        {
            context.PlayerVerticalLayer.ChangeState(new PlayerGroundedState(), context);
        }
    }

    public void Update(PlayerContext context) { }
}
