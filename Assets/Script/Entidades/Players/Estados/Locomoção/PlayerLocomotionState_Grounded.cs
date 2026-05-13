using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotionStateGrounded : ILocomotionState<Player>
{
  // ─── IState ───────────────────────────────────────────────────────────────
  public ActionType Type => ActionType.GroundSlam;
  public HashSet<ActionType> IncompatibleActions => new() { ActionType.Dash };

  // ─── Coyote Time ──────────────────────────────────────────────────────────
  private readonly Timer _coyoteTimer = new();
  private bool _coyoteStarted = false;
  private const float CoyoteInterval = 0.3f;

  // ─── Movement ─────────────────────────────────────────────────────────────
  private Dictionary<bool, float> _speeds;
  private Dictionary<bool, float> _accelerations;

  // ─── Enter / Exit ─────────────────────────────────────────────────────────

  public void Enter(Player player)
  {
    _speeds = new Dictionary<bool, float> { [false] = player.Speed, [true] = player.RunningSpeed };
    _accelerations = new Dictionary<bool, float>
    {
      [false] = player.Acceleration,
      [true] = player.AccelerationRunning,
    };

    player.CurrentJumpCount = 0;
    player.CurrentDashCount = 0;
    player.IsImpulsioned = false;
    player.CanDash = true;
    player.JumpInteractionPressed = false;

    var move = player.MovementVector;
    move.y = -1f;
    player.MovementVector = move;

    if (player.GroundSlamImpactSpeed > 0f)
    {
      player.ActionLayer.PushState(player.Bounce, player);
      player.ActionLayer.PushStateDeferred(player.Jump, player);
    }
  }

  public void Exit(Player player)
  {
    _coyoteTimer.Stop();
    _coyoteStarted = false;
  }

  // ─── Update / FixedUpdate ─────────────────────────────────────────────────

  public void Update(Player player) { }

  public void FixedUpdate(Player player)
  {
    if (player.ActionLayer.GetActive<PlayerActionStateJump>() == null)
      HandleCoyoteTime(player);

    HandleHorizontalMovement(player);
  }

  // ─── Coyote Time ──────────────────────────────────────────────────────────

  private void HandleCoyoteTime(Player player)
  {
    if (!player.IsGrounded && !_coyoteStarted)
    {
      _coyoteTimer.Start(CoyoteInterval);
      _coyoteStarted = true;
    }

    if (!_coyoteStarted)
      return;

    bool timerExpired = _coyoteTimer.Tick(Time.deltaTime);

    if (player.IsGrounded)
    {
      _coyoteStarted = false;
    }
    else if (timerExpired)
    {
      _coyoteStarted = false;
      player.ActionLayer.ExitStateDeferred(player.Bounce, player);
      player.LocomotionLayer.ChangeState(player.AirborneS, player);
    }
  }

  // ─── Horizontal Movement ──────────────────────────────────────────────────

  private void HandleHorizontalMovement(Player player)
  {
    if (player.MoveInput == Vector2.zero)
    {
      var move = player.MovementVector;
      move.x = QualityOfLife.PlayerFriction(move.x, player.Friction, player.MoveInput);
      move.z = QualityOfLife.PlayerFriction(move.z, player.Friction, player.MoveInput);
      player.MovementVector = move;
      return;
    }

    float speed = _speeds[player.IsRunning];
    Vector3 direction = ILocomotionState<Player>.CalculateCameraDirection(player);

    player.transform.rotation = Quaternion.Slerp(
      player.transform.rotation,
      Quaternion.LookRotation(direction),
      10f * Time.deltaTime
    );

    var m = player.MovementVector;
    player.MovementVector = new Vector3(
      QualityOfLife.SmoothStepLerp(m.x, direction.x * speed, _accelerations[player.IsRunning]),
      m.y,
      QualityOfLife.SmoothStepLerp(m.z, direction.z * speed, _accelerations[player.IsRunning])
    );
  }
}
