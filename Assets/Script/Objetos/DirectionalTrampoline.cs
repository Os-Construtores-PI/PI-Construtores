using UnityEngine;

public class DirectionalTrampoline : DashInteractableObject
{
  [SerializeField]
  private float _impulseForce = 10f;

  [SerializeField]
  private Color _gizmoColor = Color.white;

  public override void Interaction(Player player)
  {
    player.MovementVector = transform.up * _impulseForce;
    player.DisableLockIn();
  }

  public override void OnDrawGizmos()
  {
    base.OnDrawGizmos();
    Gizmos.color = _gizmoColor;
    Gizmos.DrawRay(transform.position, transform.up * 10);
  }
}
