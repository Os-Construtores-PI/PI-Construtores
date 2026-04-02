using UnityEngine;
using static Constants;

public class Pandora : Player
{
  #region --- OBJETOS ---

  bool HasGrapling = true;

  protected override (bool, RaycastHit) ScanWithCamera()
  {
    var (success, hit) = base.ScanWithCamera();

    if (!success || _interactionObject == null)
    {
      if (_lastInteractionObject != null)
      {
        ClearInteractable();
        _lastInteractionObject = null;
      }
      return (false, default);
    }

    _interactionObjectType = _interactionObject.GetType();

    bool valid =
      PlayerCommonObjects.types.Contains(_interactionObjectType)
      || PandoraObjects.types.Contains(_interactionObjectType);

    if (!valid)
    {
      ClearInteractable();
      return (false, default);
    }

    if (_interactionObject != _lastInteractionObject)
    {
      _lastInteractionObject = _interactionObject;
      GlobalEventBus.Instance.OBJECTWASSEEN.Invoke(true, _interactionObject, ID);
    }

    return (true, hit);
  }

  #endregion
  #region --- ATAQUE ---
  protected override void Attack()
  {
    if (canAttack && willAttack)
    {
      ActionLayer.PushState(new PlayerActionPandoraAttackState(), Context);
    }
  }
  #endregion
}
