using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotionStateLocked : ILocomotionState<Player>
{
  public ActionType Type => ActionType.Locked;
  public HashSet<ActionType> IncompatibleActions => new() { };

  public void Enter(Player player) { }

  public void Exit(Player player) { }

  public void Update(Player player) { }

  public void FixedUpdate(Player player) { }
}
