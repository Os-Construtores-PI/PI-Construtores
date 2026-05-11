using System.Collections.Generic;
using UnityEngine;

public class PlayerActionStateGroundSlam : IState<Player>
{
  public ActionType Type => ActionType.GroundSlam;
  public HashSet<ActionType> IncompatibleActions => new() { ActionType.Dash, ActionType.Jump };

  private const float SlamForce = 75f;
  private const float MaxImpactCap = 30f;

  private bool _deactivated = false;
  private Vector2 _momentum;

  public void Enter(Player player)
  {
    _momentum = new(player.MovementVector.x, player.MovementVector.z);
    _deactivated = false;
    player.GroundSlamImpactSpeed = 0f;
    player.LocomotionLayer.ChangeState(player.LockedS, player);
  }

  public void Update(Player player) { }

  public void FixedUpdate(Player player)
  {
    if (!player.IsGrounded)
    {
      player.MovementVector = new(_momentum.x, -SlamForce, _momentum.y);
      player.GroundSlamHitboxCollider.enabled = true;
      float currentFallSpeed = Mathf.Abs(player.MovementVector.y);
      player.GroundSlamImpactSpeed = Mathf.Min(
        Mathf.Max(player.GroundSlamImpactSpeed, currentFallSpeed),
        MaxImpactCap
      );
    }
    else if (!_deactivated)
    {
      _deactivated = true;
      player.LocomotionLayer.ChangeState(player.GroundedS, player);
      player.ActionLayer.ExitState(this, player);
    }
  }

  public void Exit(Player player)
  {
    player.GroundSlamHitboxCollider.enabled = false;
  }
}
