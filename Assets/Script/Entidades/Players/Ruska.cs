using UnityEngine;

public class Ruska : Player
{
  protected override (bool, RaycastHit) ScanWithCamera()
  {
    // 1 — Executa o scan base
    var (hit, info) = base.ScanWithCamera();

    if (!hit)
      return (false, default);

    // 2 — Filtro final da classe filha
    if (
      !Constants.PlayerCommonObjects.types.Contains(_interactionObjectType)
      && !Constants.PandoraObjects.types.Contains(_interactionObjectType)
    )
    {
      ClearInteractable();
      return (false, default);
    }

    // 3 — Sucesso
    interactableRef = _interactionObject;
    GlobalEventBus.Instance.OBJECTWASSEEN.Invoke(true, _interactionObject, ID);

    return (true, info);
  }
}
