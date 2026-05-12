using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotionStateHLocked : ILocomotionState<Player>
{
  public ActionType Type => ActionType.None;

  public HashSet<ActionType> IncompatibleActions => new();

  public void Enter(Player player) { }

  public void Exit(Player player) { }

  public void FixedUpdate(Player player)
  {
    ILocomotionState<Player>.ApplyGravity(player);
  }

  public void Update(Player player) { }
}
