using System.Collections.Generic;
using DG.Tweening;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerActionStateBoost : IState<Player>
{
  public ActionType Type => ActionType.Boost;
  public HashSet<ActionType> IncompatibleActions => new();

  private float _playerRotation = 0;
  private float _rotationSpeed = 20f;
  private float _boostUsage = 20;

  public void Enter(Player player)
  {
    player.LocomotionLayer.ChangeState(player.LockedS, player);
    player.MainCamera.Priority = 0;
    player.BoostCamera.Priority = 20;
  }

  public void Exit(Player player)
  {
    player.LocomotionLayer.ChangeState(player.LocomotionLayer.PreviousState, player);
    player.MainCamera.Priority = 20;
    player.BoostCamera.Priority = 0;
  }

  public void FixedUpdate(Player player)
  {
    _playerRotation += player.MoveInput.x * _rotationSpeed;
    player.MovementVector = player.transform.forward * 50;
    player.MovementVector.y = -player.transform.up.y;
    player.transform.Rotate(new Vector3(0, _playerRotation, 0) * Time.deltaTime);
  }

  public void Update(Player player)
  {
    player.DashSlashBoostButton.Value -= _boostUsage * Time.deltaTime;
    if (player.DashSlashBoostButton.Value <= 0)
    {
      player.ActionLayer.ExitStateDeferred(this, player);
    }
  }
}
