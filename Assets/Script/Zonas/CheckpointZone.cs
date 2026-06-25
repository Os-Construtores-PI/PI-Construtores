using UnityEngine;

public class CheckpointZone : MonoBehaviour
{
  private bool saved = false;

  private void OnTriggerEnter(Collider other)
  {
    if (saved || DataDirector.Instance == null)
      return;
    if (other.TryGetComponent(out Player _))
    {
      DataDirector.Instance.SaveCheckpoint(DataDirector.Instance.GetCurrentSlot());
      saved = true;
    }
  }
}
