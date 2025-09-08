using UnityEngine;

public class Pandora : Player
{
    protected override void ObjectScan()
    {
        base.ObjectScan();
        if (!Constants.HighRangeObjects.types.Contains(interactionObjectType)) return;
        interactableRef = interactionObject;
        GlobalEventBus.Instance.ObjectWasSeen.Invoke(true, interactionObject, ID);
        return;
    }
}
