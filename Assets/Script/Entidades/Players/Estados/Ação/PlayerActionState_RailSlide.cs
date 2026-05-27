using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[System.Serializable]
public class PlayerActionStateRailSlide : IState<Player>
{
  public ActionType Type => ActionType.RailSlide;

  private readonly HashSet<ActionType> _incompatibleAction = new() { { ActionType.Dash } };
  public HashSet<ActionType> IncompatibleActions => _incompatibleAction;

  private SplineContainer _currentRail;
  private float _currentRailLength;

  public void Enter(Player player)
  {
    if (player.CurrentRail == null)
    {
      _currentRail = null;
      _currentRailLength = default;
      player.ActionLayer.ExitState(this, player);
      return;
    }

    player.LocomotionLayer.ChangeState(player.LockedInHorizontal, player);
    _currentRail = player.CurrentRail;
    _currentRailLength = _currentRail.Spline.GetLength();
  }

  public void Exit(Player player)
  {
    player.LocomotionLayer.ChangeState(player.Moving, player);
  }

  public void FixedUpdate(Player player) { }

  public void Update(Player player) { }
}
