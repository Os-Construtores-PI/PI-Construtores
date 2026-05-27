using System.Collections.Generic;

[System.Serializable]
public class PlayerActionPandoraAttackState : IState<Player>
{
  public ActionType Type => ActionType.Attack;

  public HashSet<ActionType> IncompatibleActions => new() { ActionType.Slide };

  public void Enter(Player player)
  {
    player.AnimatorComponent.SetTrigger("Attack");
    player.ActionLayer.PopStateDeferred(player);
  }

  public void Exit(Player player) { }

  public void FixedUpdate(Player player) { }

  public void Update(Player player) { }
}

public class PlayerActionRuskaAttackState : IState<Player>
{
  public ActionType Type => ActionType.Attack;

  public HashSet<ActionType> IncompatibleActions => new() { };

  public void Enter(Player player)
  {
    player.ActionLayer.PopStateDeferred(player);
  }

  public void Exit(Player player) { }

  public void FixedUpdate(Player player) { }

  public void Update(Player player) { }
}
