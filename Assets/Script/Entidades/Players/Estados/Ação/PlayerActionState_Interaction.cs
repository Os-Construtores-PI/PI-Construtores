using System.Collections.Generic;

public class PlayerActionStateInteraction : IState<PlayerContext>
{
    public ActionType Type => ActionType.Interact;

    public HashSet<ActionType> IncompatibleActions => new() {};

    public void Enter(PlayerContext context)
    {
        InfoPlayerInteraction info = new(context.PlayerGameObject, context);
        context.PlayerInteractionReference.Interaction(info);
        context.ActionLayer.PopStateDeferred(context);
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
