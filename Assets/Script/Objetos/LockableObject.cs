using UnityEngine;

public class LockableObject : Object, ILockable
{
  [SerializeField]
  private float _lockRange = 10;

  [SerializeField, Range(0, 100)]
  protected float _boostGrace = 0f;
  public float LockRange => _lockRange;
  public float BoostGrace => _boostGrace;
}
