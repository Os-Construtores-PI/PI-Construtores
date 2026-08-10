using UnityEngine;

public class DirectionalTrampoline : LockableInteractableObject
{
  [SerializeField]
  private float _impulseForce = 10f;

  [SerializeField]
  private Color _gizmoColor = Color.white;

  private bool _canJump = true;

  public override void Interaction(Player player)
  {
    _canJump = false;
    player.Motor.Velocity = Vector3.zero;
    player.Motor.Velocity = transform.up * _impulseForce;
    player.IsImpulsioned = true;
    player.CurrentDashCount = 0;
    player.BoostValue += _boostGrace;
    player.DisableLockIn();
    player.AnimatorComponent.SetTrigger(Constants.AnimatorTriggerNames.WasVerticalBoosted);
    _interactionTimer.Start(_interactionCooldown);
  }

  public void Update()
  {
    if (_interactionTimer.Tick(Time.deltaTime))
    {
      _canJump = true;
    }
  }

#if UNITY_EDITOR
  public override void OnDrawGizmos()
  {
    base.OnDrawGizmos();
    Gizmos.color = _gizmoColor;
    Gizmos.DrawRay(transform.position, transform.up * 10);
  }
#endif

  public void OnTriggerEnter(Collider collision)
  {
    if (collision.TryGetComponent(out Player player) && _canJump)
    {
      Interaction(player);
    }
  }
}
