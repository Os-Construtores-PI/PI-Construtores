using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class NavBasedEnemy : Enemies
{
  protected NavMeshAgent agent;

  public override void Awake()
  {
    base.Awake();
    TryGetComponent(out agent);
  }

  public void FixedUpdate()
  {
    if (target != null && agent)
    {
      agent.SetDestination(target.position);
    }
  }
}
