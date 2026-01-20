using UnityEngine;

public class Pandora : Player
{
    #region --- OBJETOS ---
    bool HasGrapling = true;
    protected override (bool, RaycastHit) ScanObjects()
    {
        var (hit, info) = base.ScanObjects();

        if (!hit)
        {
            ClearInteractable();
            return (false, default);
        }

        // FILTRO FINAL
        bool valid =
            Constants.PlayerCommonObjects.types.Contains(interactionObjectType)
            || (Constants.PandoraObjects.types.Contains(interactionObjectType) && HasGrapling);

        if (!valid)
        {
            ClearInteractable();
            return (false, default);
        }

        // sucesso final
        interactableRef = interactionObject;
        GlobalEventBus.Instance.OBJECTWASSEEN.Invoke(true, interactionObject, ID);

        return (true, info);
    }




    #endregion
    #region --- ATAQUE ---
    protected override void Attack()
    {
        if(canAttack && willAttack)
        {
            ActionLayer.PushState(new PlayerActionPandoraAttackState(), Context);
        }
    }
    #endregion
}
