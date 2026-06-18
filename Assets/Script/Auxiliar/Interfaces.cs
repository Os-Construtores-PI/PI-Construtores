using System.Collections.Generic;
using UnityEngine;

public interface IState<T>
{
  virtual int Priority => 0;
  void Enter(T entity);
  void Update(T entity);
  void FixedUpdate(T entity);
  void Exit(T entity);
}

public interface IPlayerState<T> : IState<T>
{
  PlayerActionType Type { get; }
  HashSet<PlayerActionType> IncompatibleActions { get; }
}

public interface IWolfState<T> : IState<T>
{
  WolfActionType Type { get; }
  HashSet<WolfActionType> IncompatibleActions { get; }
}

public interface ILocomotionState<T> : IPlayerState<T>
{
  protected static void ApplyGravity(Player player)
  {
    Vector3 move = player.MovementVector;
    float gravMult = move.y > 0f ? player.GravityUpMultiplier : player.GravityDownMultiplier;
    move.y += player.GravityValue * gravMult * Time.deltaTime;
    if (move.y < player.MaxFallSpeed)
      move.y = player.MaxFallSpeed;
    player.MovementVector = move;
  }

  protected static Vector3 CalculateCameraDirection(Player player)
  {
    Vector3 camForward = player.MainCamera.transform.forward;
    Vector3 camRight = player.MainCamera.transform.right;
    camForward.y = camRight.y = 0f;

    return (
      camForward.normalized * player.MoveInput.y + camRight.normalized * player.MoveInput.x
    ).normalized;
  }
}

public interface ILockable
{
  Transform transform { get; }
  public float LockRange { get; }
  public float BoostGrace { get; }
  public bool IsActive { get; }
}
