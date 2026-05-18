using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class NavBasedEnemy : Enemies
{
  protected NavMeshAgent _agent;

  public override void Awake()
  {
    base.Awake();
    TryGetComponent(out _agent);
  }
}
