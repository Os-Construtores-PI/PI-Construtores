using UnityEngine;

public class RailSegmentMarker : MonoBehaviour, ILockable
{
  public RailObject Owner;

  public float LockRange { get; set; }

  public float BoostGrace { get; set; }
}
