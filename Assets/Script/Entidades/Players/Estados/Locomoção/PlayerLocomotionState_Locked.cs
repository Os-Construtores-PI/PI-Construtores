using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotionStateLocked : IState<Player>
{
  public ActionType Type => ActionType.Dash;
  public HashSet<ActionType> IncompatibleActions => new() { };

  public void Enter(Player player) { }

  public void Exit(Player player) { }

  public void Update(Player player) { }

  public void FixedUpdate(Player player) { }
}
