using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerLocomotionStateGrounded : IState<Player>
{
  // ─── IState ───────────────────────────────────────────────────────────────
  public ActionType Type => ActionType.GroundSlam;
  public HashSet<ActionType> IncompatibleActions => new() { { ActionType.Dash } };

  // ─── Coyote Time ──────────────────────────────────────────────────────────
  private readonly Timer _coyoteTimer = new();
  private bool _coyoteStarted = false;
  private const float CoyoteInterval = 0.3f;

  // ─── Movement ─────────────────────────────────────────────────────────────
  private Dictionary<bool, float> _speeds;
  private Dictionary<bool, float> _accelerations;

  // ─── Bounce Combo ─────────────────────────────────────────────────────────
  [Header("Bounce Settings")]
  private const float BounceWindowDuration = 0.4f; // janela após pousar
  private const int MaxBounceCombo = 3; // máximo de stacks
  private const float BounceFrontImpulse = 20;
  private readonly float[] BounceMultipliers = { 1f, 1.4f, 1.8f, 2.4f };

  private int _bounceCombo = 0;
  private float _bounceWindowLeft = 0f;
  private bool _justLanded = false;

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

    // Marca que acabou de pousar para abrir a janela de bounce
    _justLanded = true;
    _bounceWindowLeft = BounceWindowDuration;

    var move = player.MovementVector;
    move.y = -1f;
    player.MovementVector = move;
  }

  public void Exit(Player player)
  {
    _coyoteTimer.Stop();
    _justLanded = false;
  }

  public void Update(Player player)
  {
    // Tick da janela de bounce
    if (_bounceWindowLeft > 0f)
      _bounceWindowLeft -= Time.deltaTime;

    if (player.JumpInputPressed && player.CurrentJumpCount < player.MaxJumpCount)
      ExecuteJump(player);
  }

  public void FixedUpdate(Player player)
  {
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
      // Pousou de volta — abre janela de bounce
      if (!_justLanded)
      {
        _justLanded = true;
        _bounceWindowLeft = BounceWindowDuration;
      }
      _coyoteStarted = false;
    }
    else if (timerExpired)
    {
      // Saiu da plataforma sem pular
      _bounceCombo = 0;
      _coyoteStarted = false;
      player.LocomotionLayer.ChangeState(player.AirborneS, player);
    }
  }

  // ─── Jump ─────────────────────────────────────────────────────────────────

  private void ExecuteJump(Player player)
  {
    var move = player.MovementVector;
    player.JumpInputPressed = false;

    // ── Bounce Combo ──────────────────────────────────────────────────────
    if (_justLanded && _bounceWindowLeft > 0f)
    {
      _bounceCombo = Mathf.Min(_bounceCombo + 1, MaxBounceCombo);
    }
    else
    {
      _bounceCombo = 0;
    }
    _justLanded = false;

    float bounceMultiplier = CalculateBounceMultiplier(_bounceCombo);

    // ── Double Jump ───────────────────────────────────────────────────────
    if (player.CurrentJumpCount > 0)
      player.AnimatorComponent.SetTrigger(Constants.AnimatorTriggerNames.DoubleJump);

    float jumpMultiplier = (1 + player.CurrentJumpCount * 0.35f) * bounceMultiplier;

    // ── Wall Jump ─────────────────────────────────────────────────────────
    if (player.TouchingWall)
    {
      const float horizontalBias = 6.5f;
      var jumpDir = (Vector3.up + player.LastWallNormal * horizontalBias).normalized;
      move = player.JumpForce * player.WallJumpMultiplier * jumpDir;
      player.TouchingWall = false;
    }
    else
    {
      if (bounceMultiplier > 1)
      {
        move += player.transform.forward * BounceFrontImpulse;
      }
      move.y += player.JumpForce * jumpMultiplier;
    }

    player.CurrentJumpCount++;
    player.EffectsWorker.PlayEffect(Constants.EffectsNames.Player.Jump, 1);
    player.MovementVector = move;
    player.LocomotionLayer.ChangeState(player.AirborneS, player);
  }

  private float CalculateBounceMultiplier(int combo)
  {
    combo = Mathf.Clamp(combo, 0, MaxBounceCombo);
    return BounceMultipliers[combo];
  }

  // ─── Horizontal Movement ──────────────────────────────────────────────────

  private void HandleHorizontalMovement(Player player)
  {
    var move = player.MovementVector;

    if (player.MoveInput == Vector2.zero)
    {
      move.x = QualityOfLife.PlayerFriction(move.x, player.Friction, player.MoveInput);
      move.z = QualityOfLife.PlayerFriction(move.z, player.Friction, player.MoveInput);
      player.MovementVector = move;
      return;
    }

    var direction = CalculateCameraDirection(player);

    player.transform.rotation = Quaternion.Slerp(
      player.transform.rotation,
      Quaternion.LookRotation(direction),
      10f * Time.deltaTime
    );

    float speed = _speeds[player.IsRunning];
    float accel = _accelerations[player.IsRunning];

    player.MovementVector = new Vector3(
      QualityOfLife.SmoothStepLerp(move.x, direction.x * speed, accel),
      move.y,
      QualityOfLife.SmoothStepLerp(move.z, direction.z * speed, accel)
    );
  }

  private static Vector3 CalculateCameraDirection(Player player)
  {
    var camForward = player.CinemachineCamera.transform.forward;
    var camRight = player.CinemachineCamera.transform.right;

    camForward.y = camRight.y = 0f;

    return (
      camForward.normalized * player.MoveInput.y + camRight.normalized * player.MoveInput.x
    ).normalized;
  }
}
