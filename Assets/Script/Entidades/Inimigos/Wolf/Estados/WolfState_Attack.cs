using System.Collections.Generic;
using UnityEngine;

public class WolfStateAttack : IWolfState<WolfBasicEnemy>
{
  public WolfActionType Type => WolfActionType.Attack;
  public HashSet<WolfActionType> IncompatibleActions =>
    new() { WolfActionType.Patrol, WolfActionType.Chase };

  public void Enter(WolfBasicEnemy wolf)
  {
    wolf.SetAnimationState(isWalking: false, isIdle: false);
    wolf.BeginAttackSequence();
  }

  public void Exit(WolfBasicEnemy wolf)
  {
    wolf.StopAttackCoroutine();
  }

  public void Update(WolfBasicEnemy wolf) { }

  public void FixedUpdate(WolfBasicEnemy wolf) { }
}
