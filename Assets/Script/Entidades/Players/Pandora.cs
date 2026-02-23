using UnityEngine;

public class Pandora : Player
{
  #region --- OBJETOS ---

  bool HasGrapling = true;

  protected override (bool, RaycastHit) ScanObjects()
  {
    var (success, hit) = base.ScanObjects();

    bool valid =
      success
      && (
        Constants.PlayerCommonObjects.types.Contains(_interactionObjectType)
        || Constants.PandoraObjects.types.Contains(_interactionObjectType)
      );
    if (!valid)
    {
      if (_lastInteractionObject != null)
      {
        ClearInteractable(); // Dispara evento false
        _lastInteractionObject = null;
      }
      return (false, default);
    }

    // SÓ dispara o evento se mudou a instância do objeto
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
