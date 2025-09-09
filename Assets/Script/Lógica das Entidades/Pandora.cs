using UnityEngine;

public class Pandora : Player
{
    bool HasGrapling = false;
    protected override void ObjectScan()
    {
        base.ObjectScan();
        if (!Constants.PandoraObjects.types.Contains(interactionObjectType) || !HasGrapling) return;
        interactableRef = interactionObject;
        GlobalEventBus.Instance.ObjectWasSeen.Invoke(true, interactionObject, ID);
        return;
    }
    protected override void Attack()
    {
        print("Pandora");
    }
}
