using System.Collections.Generic;

[System.Serializable]
public class PlayerActionStateInteraction : IState<Player>
{
  public ActionType Type => ActionType.Interact;

  public HashSet<ActionType> IncompatibleActions => new() { };

  public void Enter(Player player)
  {
    player.InteractionObject.Interaction(player);
    player.ActionLayer.PopStateDeferred(player);
  }

  public void Exit(Player player) { }

  public void FixedUpdate(Player player) { }

  public void Update(Player player) { }
}
