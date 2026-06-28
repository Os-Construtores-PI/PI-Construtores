using System.Collections.Generic;
using UnityEngine;

public class WolfStateChase : IWolfState<WolfBasicEnemy>
{
  public WolfActionType Type => WolfActionType.Chase;
  public HashSet<WolfActionType> IncompatibleActions => new();

  public void Enter(WolfBasicEnemy wolf)
  {
    wolf.ResetMemoryTimer();
    wolf.SetAnimationState(true, false);
  }

  public void Exit(WolfBasicEnemy wolf)
  {
    wolf.StopAttackCoroutine();
  }

  public void Update(WolfBasicEnemy wolf)
  {
    bool seesPlayer = wolf.Vision != null && wolf.Vision.DetectedPlayer != null;

    if (seesPlayer)
    {
      wolf.ResetMemoryTimer();
      wolf.SetCurrentTarget(wolf.Vision.DetectedPlayer);

      if (wolf.DashTimer <= 0f)
      {
        float distSqr = Vector3.SqrMagnitude(
          wolf.transform.position - wolf.Vision.DetectedPlayer.position
        );
        if (distSqr <= wolf.AttackDistanceSqr)
        {
          wolf.ChangeState(wolf.Attack);
          return;
        }
      }
    }
    else
    {
      if (wolf.MemoryTimer > 0f)
        wolf.DecrementMemoryTimer();
      else
        wolf.ChangeState(wolf.Patrol);
    }
  }

  public void FixedUpdate(WolfBasicEnemy wolf)
  {
    wolf.MoveToTarget();
  }
}
