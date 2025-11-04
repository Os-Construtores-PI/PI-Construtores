using UnityEngine;

public class PlayerActionStateInteraction : IState<PlayerContext>
{
    public void Enter(PlayerContext context)
    {
        InfoPlayerInteraction info = new(context.PlayerGameObject, context);
        context.PlayerInteractionReference.Interaction(info);
        context.ActionLayer.ChangeState(new PlayerActionStateIdle(), context);
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
