using UnityEngine;

public class Pandora : Player
{
    #region --- OBJETOS ---
    bool HasGrapling = true;
    protected override bool ObjectScan()
    {
        if (!base.ObjectScan()) return false;

        // Agora faz o filtro final
        if (!Constants.PlayerCommonObjects.types.Contains(interactionObjectType)
            && (!Constants.PandoraObjects.types.Contains(interactionObjectType) || !HasGrapling))
        {
            print("filtro final");
            ClearInteractable();
            return false;
        }

        interactableRef = interactionObject;
        GlobalEventBus.Instance.OBJECTWASSEEN.Invoke(true, interactionObject, ID);
        return true;
    }



    #endregion
    #region --- ATAQUE ---
    protected override bool Attack()
    {
        if (base.Attack())
        {
            print("ATAQUE");
            animatorComp.SetTrigger("Attack");
            return true;
        }
        return false;
    }
    #endregion
}
