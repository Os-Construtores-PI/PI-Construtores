using System.Collections.Generic;

public class PlayerActionPandoraAttackState : IState<PlayerContext>
{
    public ActionType Type => ActionType.Attack;

    public HashSet<ActionType> IncompatibleActions => new() {ActionType.Slide};

    public void Enter(PlayerContext context)
    {
        context.PlayerAnimator.SetTrigger("Attack");
        context.ActionLayer.PopState(context);
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


public class PlayerActionRuskaAttackState : IState<PlayerContext>
{
    public ActionType Type => ActionType.Attack;

    public HashSet<ActionType> IncompatibleActions => new() {};

    public void Enter(PlayerContext context)
    {
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