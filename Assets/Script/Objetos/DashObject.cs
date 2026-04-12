using UnityEngine;

public class LockableInteractableObject : InteractableObject, ILockable
{
  [SerializeField]
  private float _lockRange = 10;
  public float LockRange => _lockRange;
}
