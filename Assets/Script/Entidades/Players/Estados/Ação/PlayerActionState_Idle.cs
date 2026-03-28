using System.Collections.Generic;
using UnityEngine;

public class PlayerActionStateIdle : IState<PlayerContext>
{
    private float attackCooldownWalker = .0f;
    private float attackCooldown;

    public ActionType Type => ActionType.Idle;

    public HashSet<ActionType> IncompatibleActions => new() { };

    public void Enter(PlayerContext context)
    {
        attackCooldown = context.PlayerAttackCooldown;
    }

    public void Exit(PlayerContext context) { }

    public void FixedUpdate(PlayerContext context) { }

    public void Update(PlayerContext context)
    {
        AttackTimer(context);
    }

    private void AttackTimer(PlayerContext context)
    {
        if (!context.PlayerCanAttack && context.PlayerWillAttack)
        {
            attackCooldownWalker += Time.deltaTime;
            if (attackCooldownWalker >= attackCooldown)
            {
                context.PlayerCanAttack = true;
                attackCooldownWalker = 0f;
            }
        }
    }
}
