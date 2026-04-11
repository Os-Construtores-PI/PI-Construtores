using System.Collections.Generic;
using UnityEngine;

public class PlayerActionStateIdle : IState<Player>
{
  private float attackCooldownWalker = .0f;
  private float attackCooldown;

  public ActionType Type => ActionType.Idle;

  public HashSet<ActionType> IncompatibleActions => new() { };

  public void Enter(Player player)
  {
    attackCooldown = player.AttackCooldown;
  }

  public void Exit(Player player) { }

  public void FixedUpdate(Player player) { }

  public void Update(Player player)
  {
    AttackTimer(player);
  }

  private void AttackTimer(Player player)
  {
    if (!player.CanAttack && player.WillAttack)
    {
      attackCooldownWalker += Time.deltaTime;
      if (attackCooldownWalker >= attackCooldown)
      {
        player.CanAttack = true;
        attackCooldownWalker = 0f;
      }
    }
  }
}
