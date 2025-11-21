public class Ruska : Player
{
    protected override bool ObjectScan()
    {
        if (!base.ObjectScan()) return false;

        // Agora faz o filtro final
        if (!Constants.PlayerCommonObjects.types.Contains(interactionObjectType)
            && (!Constants.PandoraObjects.types.Contains(interactionObjectType)))
        {
            ClearInteractable();
            return false;
        }

        interactableRef = interactionObject;
        GlobalEventBus.Instance.OBJECTWASSEEN.Invoke(true, interactionObject, ID);
        return true;
    }
    
    protected override void Attack()
    {
        ActionLayer.PushState(new PlayerActionRuskaAttackState(), Context);
    }
}
