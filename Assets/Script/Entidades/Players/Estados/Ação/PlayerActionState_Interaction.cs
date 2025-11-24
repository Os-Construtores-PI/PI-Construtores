using System.Collections.Generic;

public class PlayerActionStateInteraction : IState<PlayerContext>
{
    public ActionType Type => ActionType.Interact;

    public HashSet<ActionType> IncompatibleActions => new() {};

    public void Enter(PlayerContext context)
    {
        InfoPlayerInteraction info = new(context.EntityGameObject, context);
        context.PlayerInteractionReference.Interaction(info);
        context.PlayerActionLayer.PopStateDeferred(context);
    }

    public void Exit(PlayerContext context)
    {
    }

    public void FixedUpdate(PlayerContext context)
    {
    }

    public void Update(PlayerContext context)
    {
    }
}
