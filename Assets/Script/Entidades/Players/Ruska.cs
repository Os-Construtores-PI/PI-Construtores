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

    GlobalEventBus.Instance.OBJECTWASSEEN.Invoke(true, InteractionObject, ID);

    return (true, info);
  }
}
