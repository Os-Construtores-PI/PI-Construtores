using UnityEngine;

public class Ruska : Player
{
protected override (bool, RaycastHit) ScanObjects()
{
    // 1 — Executa o scan base
    var (hit, info) = base.ScanObjects();

    if (!hit)
        return (false, default);

    // 2 — Filtro final da classe filha
    if (!Constants.PlayerCommonObjects.types.Contains(interactionObjectType) &&
        !Constants.PandoraObjects.types.Contains(interactionObjectType))
    {
        ClearInteractable();
        return (false, default);
    }

    // 3 — Sucesso
    interactableRef = interactionObject;
    GlobalEventBus.Instance.OBJECTWASSEEN.Invoke(true, interactionObject, ID);

    return (true, info);
}
}
