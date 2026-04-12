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
    player.MovementVector = Vector3.zero;
    player.MovementVector = transform.up * _impulseForce;
    player.IsImpulsioned = true;
    player.CurrentDashCount = 0;
    player.DisableLockIn();
    _interactionTimer.Start(_interactionCooldown);
  }

  public void Update()
  {
    if (_interactionTimer.Tick(Time.deltaTime))
    {
      _canJump = true;
    }
  }

  public override void OnDrawGizmos()
  {
    base.OnDrawGizmos();
    Gizmos.color = _gizmoColor;
    Gizmos.DrawRay(transform.position, transform.up * 10);
  }

  public void OnTriggerEnter(Collider collision)
  {
    if (collision.TryGetComponent(out Player player) && _canJump)
    {
      Interaction(player);
    }
  }
}
