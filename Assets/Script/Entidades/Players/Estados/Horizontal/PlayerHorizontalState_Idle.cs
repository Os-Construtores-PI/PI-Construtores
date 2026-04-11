using System.Collections.Generic;

public class PlayerHorizontalStateIdle : IState<Player>
{
  public ActionType Type => ActionType.Idle;

  public HashSet<ActionType> IncompatibleActions => new() { };

  public void Enter(Player player) { }

  public void Exit(Player player) { }

  public void FixedUpdate(Player player) { }

  public void Update(Player player) { }
}
