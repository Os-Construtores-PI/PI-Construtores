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
  bool UpdateKCCVelocity(T entity, ref Vector3 currentVelocity, float deltaTime) => false;
}

public interface IWolfState<T> : IState<T>
{
  WolfActionType Type { get; }
  HashSet<WolfActionType> IncompatibleActions { get; }
}

public interface ILocomotionState<T> : IPlayerState<T>
{
  protected static void ApplyGravity(ref Vector3 currentVelocity, Player player, float deltaTime)
  {
    float gravMult =
      currentVelocity.y > 0f ? player.GravityUpMultiplier : player.GravityDownMultiplier;

    currentVelocity.y += player.GravityValue * gravMult * deltaTime;

    if (currentVelocity.y < player.MaxFallSpeed)
      currentVelocity.y = player.MaxFallSpeed;
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

  public void CalculateKCCVelocity(Player player, ref Vector3 currentVelocity, float deltaTime);
}

public interface IGroundCollisionHandler
{
  void OnGroundHit(Player player, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint);
  void OnLanded(Player player);
  void OnLeftGround(Player player);
}

public interface IWallCollisionHandler
{
  void OnWallHit(Player player, Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint);
}

public interface ILockable
{
  public Transform transform { get; }
  public float LockRange { get; }
  public float BoostGrace { get; }
  public Vector3 GetLockOnPoint(Vector3 referencePoint) => transform.position;
}

public interface IRespawnable
{
  public bool IsAlive { get; }
  public void Respawn();
}
