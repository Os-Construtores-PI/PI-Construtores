using UnityEngine;

public class PlayerActionPandoraAttackState : IState<PlayerContext>
{
    public void Enter(PlayerContext context)
    {
        context.PlayerAnimator.SetTrigger("Attack");
        context.ActionLayer.ChangeState(new PlayerActionIdleState(), context);
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
    public void Enter(PlayerContext context)
    {
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