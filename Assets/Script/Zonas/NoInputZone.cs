using UnityEngine;

public class NoInputZone : MonoBehaviour
{
  public void OnTriggerStay(Collider collider)
  {
    if (!collider.TryGetComponent(out Player player))
      return;

    player.PlayerInput.DeactivateInput();
  }

  public void OnTriggerExit(Collider collider)
  {
    if (!collider.TryGetComponent(out Player player))
      return;

    player.PlayerInput.ActivateInput();
  }
}
