using System.Collections.Generic;
using UnityEngine;

public interface IState<T>
{
  ActionType Type { get; }
  HashSet<ActionType> IncompatibleActions { get; }
  virtual int Priority => 0;
  void Enter(T entity);
  void Update(T entity);
  void FixedUpdate(T entity);
  void Exit(T entity);
}

public interface ILocomotionState<T> : IState<T>
{
  protected static Vector3 CalculateCameraDirection(Player player)
  {
    Vector3 camForward = player.CinemachineCamera.transform.forward;
    Vector3 camRight = player.CinemachineCamera.transform.right;
    camForward.y = camRight.y = 0f;

    return (
      camForward.normalized * player.MoveInput.y + camRight.normalized * player.MoveInput.x
    ).normalized;
  }

  protected static void ApplyHorizontalMovement(
    Player player,
    float targetSpeed,
    float acceleration
  )
  {
    Vector3 move = player.MovementVector;

    if (player.MoveInput == Vector2.zero)
    {
      move.x = QualityOfLife.PlayerFriction(move.x, player.AirFriction, player.MoveInput);
      move.z = QualityOfLife.PlayerFriction(move.z, player.AirFriction, player.MoveInput);
      player.MovementVector = move;
      return;
    }

    Vector3 direction = CalculateCameraDirection(player);

    player.transform.rotation = Quaternion.Slerp(
      player.transform.rotation,
      Quaternion.LookRotation(direction),
      10f * Time.deltaTime
    );

    player.MovementVector = new Vector3(
      QualityOfLife.SmoothStepLerp(move.x, direction.x * targetSpeed, acceleration),
      move.y,
      QualityOfLife.SmoothStepLerp(move.z, direction.z * targetSpeed, acceleration)
    );
  }
}

public interface ILockable
{
  Transform transform { get; }
  public float LockRange { get; }
  public float BoostGrace { get; }
  public bool IsActive { get; }
}
