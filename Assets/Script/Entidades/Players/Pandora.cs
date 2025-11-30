using UnityEngine;

public class Pandora : Player
{
    #region --- OBJETOS ---
    bool HasGrapling = true;
    protected override (bool, RaycastHit) ScanObjects()
    {
        // executa o scan base
        var (hit, info) = base.ScanObjects();

        if (!hit)
            return (false, default);

        // FILTRO FINAL (somente na classe filha)
        if (!Constants.PlayerCommonObjects.types.Contains(interactionObjectType)
            && (!Constants.PandoraObjects.types.Contains(interactionObjectType) || !HasGrapling))
        {
            ClearInteractable();
            return (false, default);
        }

        // Sucesso
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
