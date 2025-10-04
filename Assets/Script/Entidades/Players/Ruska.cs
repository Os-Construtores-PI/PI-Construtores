using UnityEngine;

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
    
    protected override bool Attack()
    {
        if (base.Attack())
        {
            print("RUSKA ATAQUE");
            return true;
        }
        return false;
    }
}
