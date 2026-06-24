using UnityEngine;

public class Ruska : Player
{
  protected override (bool, RaycastHit) ScanWithCamera()
  {
    var (hit, info) = base.ScanWithCamera();

    if (!hit)
      return (false, default);

    if (
      !Constants.PlayerCommonObjects.types.Contains(_interactionObjectType)
      && !Constants.PandoraObjects.types.Contains(_interactionObjectType)
    )
    {
      ClearInteractable();
      return (false, default);
    }

    GlobalEventBus.Instance.ObjectWasSeen.Invoke(ID, true, InteractionObject);

    return (true, info);
  }
}
