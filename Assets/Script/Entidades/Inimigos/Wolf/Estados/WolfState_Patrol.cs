using System.Collections.Generic;
using UnityEngine;

public class WolfStatePatrol : IWolfState<WolfBasicEnemy>
{
  public WolfActionType Type => WolfActionType.Patrol;
  public HashSet<WolfActionType> IncompatibleActions => new();

  public void Enter(WolfBasicEnemy wolf)
  {
    wolf.SetAnimationState(true, false);
    wolf.PickNewPatrolPoint();
  }

  public void Exit(WolfBasicEnemy wolf)
  {
    wolf.StopIdleCoroutine();
  }

  public void Update(WolfBasicEnemy wolf)
  {
    if (wolf.Vision != null && wolf.Vision.DetectedPlayer != null)
    {
      wolf.StopIdleCoroutine();
      wolf.ChangeState(wolf.Chase);
    }
  }

  public void FixedUpdate(WolfBasicEnemy wolf)
  {
    if (wolf.IsWaiting)
      return;
    wolf.MoveToPatrolPoint();
  }
}
