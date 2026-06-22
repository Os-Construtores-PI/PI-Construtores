using UnityEngine;
using static Constants;

public class Pandora : Player
{
  #region --- OBJETOS ---

  bool HasGrapling = true;

  protected override (bool, RaycastHit) ScanWithCamera()
  {
    var (success, hit) = base.ScanWithCamera();

    if (!success || InteractionObject == null)
    {
      if (_lastInteractionObject != null)
      {
        ClearInteractable();
        _lastInteractionObject = null;
      }
      return (false, default);
    }

    _interactionObjectType = InteractionObject.GetType();

    bool valid =
      PlayerCommonObjects.types.Contains(_interactionObjectType)
      || PandoraObjects.types.Contains(_interactionObjectType);

    if (!valid)
    {
      ClearInteractable();
      return (false, default);
    }

    if (InteractionObject != _lastInteractionObject)
    {
      _lastInteractionObject = InteractionObject;
      GlobalEventBus.Instance.ObjectWasSeen.Invoke(true, InteractionObject, ID);
    }

    return (true, hit);
  }

  #endregion
  #region --- ATAQUE ---
  protected override void OnExecuteAttack()
  {
    // if (CanAttack && WillAttack)
    // {
    //   ActionLayer.PushState(new PlayerActionPandoraAttackState(), this);
    // }
  }
  #endregion
}
