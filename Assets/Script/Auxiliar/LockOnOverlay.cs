using UnityEngine;

public class LockOnOverlay : MonoBehaviour
{
  [HideInInspector]
  public Vector3 TargetPosition;

  public void Update()
  {
    if (TargetPosition != null)
    {
      transform.position = TargetPosition;
    }
  }
}
