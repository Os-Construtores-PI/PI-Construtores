using UnityEngine;

public class Ruska : Player
{
    protected override void ObjectScan()
    {
        base.ObjectScan();
        if (!Constants.RuskaObjects.types.Contains(interactionObjectType)) return;
        interactableRef = interactionObject;
        GlobalEventBus.Instance.ObjectWasSeen.Invoke(true, interactionObject, ID);
        return;
    }
    protected override void Attack()
    {
        print("Ruska");
    }
}
