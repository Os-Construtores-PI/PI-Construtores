using System.Collections.Generic;

[System.Serializable]
public class PlayerActionPandoraAttackState : IPlayerState<Player>
{
  public PlayerActionType Type => PlayerActionType.Attack;

  public HashSet<PlayerActionType> IncompatibleActions => new() { PlayerActionType.Slide };

  public void Enter(Player player)
  {
    player.AnimatorComponent.SetTrigger("Attack");
    player.ActionLayer.PopStateDeferred(player);
  }

  public void Exit(Player player) { }

  public void FixedUpdate(Player player) { }

  public void Update(Player player) { }
}

public class PlayerActionRuskaAttackState : IPlayerState<Player>
{
  public PlayerActionType Type => PlayerActionType.Attack;

  public HashSet<PlayerActionType> IncompatibleActions => new() { };

  public void Enter(Player player)
  {
    player.ActionLayer.PopStateDeferred(player);
  }

  public void Exit(Player player) { }

  public void FixedUpdate(Player player) { }

  public void Update(Player player) { }
}
