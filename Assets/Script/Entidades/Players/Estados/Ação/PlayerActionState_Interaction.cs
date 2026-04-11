using System.Collections.Generic;

public class PlayerActionStateInteraction : IState<Player>
{
  public ActionType Type => ActionType.Interact;

  public HashSet<ActionType> IncompatibleActions => new() { };

  public void Enter(Player player)
  {
    InfoPlayerInteraction info = new(player);
    player.InteractionObject.Interaction(info);
    player.ActionLayer.PopStateDeferred(player);
  }

  public void Exit(Player player) { }

  public void FixedUpdate(Player player) { }

  public void Update(Player player) { }
}
