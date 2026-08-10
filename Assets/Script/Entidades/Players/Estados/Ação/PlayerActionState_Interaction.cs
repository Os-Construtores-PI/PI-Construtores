using System.Collections.Generic;

[System.Serializable]
public class PlayerActionStateInteraction : IPlayerState<Player>
{
  public PlayerActionType Type => PlayerActionType.Interact;

  public HashSet<PlayerActionType> IncompatibleActions => new() { };

  public void Enter(Player player)
  {
    player.InteractionObject.Interaction(player);
    player.ActionLayer.PopStateDeferred(player);
  }

  public void Exit(Player player) { }

  public void FixedUpdate(Player player) { }

  public void Update(Player player) { }
}
