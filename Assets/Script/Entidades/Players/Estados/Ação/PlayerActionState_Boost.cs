using System.Collections.Generic;
using DG.Tweening;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerActionStateBoost : IState<Player>
{
  public ActionType Type => ActionType.Boost;
  public HashSet<ActionType> IncompatibleActions => new();

  private float _rotationSpeed = 60f;
  private float _boostUsage = 20;
  private float _velocity = 50f;

  public void Enter(Player player)
  {
    player.LocomotionLayer.ChangeState(player.HLockedS, player);
    _velocity = player.DashSlashBoostButton.Value;
    player.MainCamera.Priority = 0;
    player.BoostCamera.Priority = 20;
  }

  public void Exit(Player player)
  {
    player.LocomotionLayer.ChangeState(player.GroundedS, player);
    player.MovementVector += player.transform.forward * _velocity;
    _velocity = 0f;
    player.MainCamera.Priority = 20;
    player.BoostCamera.Priority = 0;
  }

  public void FixedUpdate(Player player)
  {
    player.transform.Rotate(
      Vector3.up,
      player.MoveInput.x * _rotationSpeed * Time.deltaTime,
      Space.World
    );
    Vector3 horizontal = player.transform.forward * _velocity;
    player.MovementVector = new(horizontal.x, player.MovementVector.y, horizontal.z);
  }

  public void Update(Player player)
  {
    player.DashSlashBoostButton.Value -= _boostUsage * Time.deltaTime;
    if (player.DashSlashBoostButton.Value <= 0)
    {
      player.ActionLayer.ExitState(this, player);
    }
  }
}
