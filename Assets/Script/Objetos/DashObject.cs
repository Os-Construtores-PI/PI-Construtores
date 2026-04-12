using UnityEngine;

public class DashInteractableObject : InteractableObject, ILockable
{
  [SerializeField]
  private float _lockRange = 10;
  public float LockRange => _lockRange;

  public void OnTriggerEnter(Collider collision)
  {
    if (
      collision.TryGetComponent(out Player player)
      && player.IsDashing
      && player.LockedTarget as Object == this
    )
    {
      Interaction(player);
    }
  }
}
