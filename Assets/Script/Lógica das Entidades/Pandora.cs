using UnityEngine;

public class Pandora : Player
{
    #region --- OBJETOS ---
    bool HasGrapling = false;
    protected override void ObjectScan()
    {
        base.ObjectScan();
        if (!Constants.PandoraObjects.types.Contains(interactionObjectType) || !HasGrapling) return;
        interactableRef = interactionObject;
        GlobalEventBus.Instance.ObjectWasSeen.Invoke(true, interactionObject, ID);
        return;
    }
    #endregion
    #region --- ATAQUE ---
    protected override void Attack()
    {
        base.Attack();
        print("PANDORA ATAQUE");
    }
    #endregion
}
