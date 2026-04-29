using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotionStateGrounded : IState<Player>
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

  // ─── Bounce ───────────────────────────────────────────────────────────────
  private const float BounceWindowDuration = 0.4f;
  private const int MaxBounceCombo = 3;
  private int _bounceCombo = 0;
  private const float BounceFrontImpulse = 30f;

  // Quanto da velocidade de queda vira impulso vertical (0–1)
  // Ex: caiu a 75 u/s → bounce base = 75 * 0.85 = ~63.75
  private const float BounceConversionRate = 0.85f;

  // Amplificação por combo (sem GroundSlam = só JumpForce normal)
  private readonly float[] BounceComboBonus = { 0f, 0.25f, 0.55f, 0.90f };

  private float _bounceWindowLeft = 0f;
  private bool _justLanded = false;
  private bool _jumpedThisState = false;

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
    _jumpedThisState = false;

    _justLanded = true;
    _bounceWindowLeft = BounceWindowDuration;

    var move = player.MovementVector;
    move.y = -1f;
    player.MovementVector = move;
  }

  public void Exit(Player player)
  {
    _coyoteTimer.Stop();
    _coyoteStarted = false;
    _justLanded = false;
  }

  // ─── Update / FixedUpdate ─────────────────────────────────────────────────

  public void Update(Player player)
  {
    if (_bounceWindowLeft > 0f)
      _bounceWindowLeft -= Time.deltaTime;

    if (player.JumpInputPressed && player.CurrentJumpCount < player.MaxJumpCount)
      ExecuteJump(player);
  }

  public void FixedUpdate(Player player)
  {
    if (!_jumpedThisState)
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
      if (!_justLanded)
      {
        _justLanded = true;
        _bounceWindowLeft = BounceWindowDuration;
      }
      _coyoteStarted = false;
    }
    else if (timerExpired)
    {
      _bounceCombo = 0;
      _coyoteStarted = false;
      player.LocomotionLayer.ChangeState(player.AirborneS, player);
    }
  }

  // ─── Jump / Bounce ────────────────────────────────────────────────────────

  private void ExecuteJump(Player player)
  {
    player.JumpInputPressed = false;
    _jumpedThisState = true;

    var move = player.MovementVector;
    bool isBounce = _justLanded && _bounceWindowLeft > 0f && player.GroundSlamImpactSpeed > 0f;
    bool isComboWindow = isBounce;

    // ── Atualiza combo ────────────────────────────────────────────────────
    if (isComboWindow)
      _bounceCombo = Mathf.Min(_bounceCombo + 1, MaxBounceCombo);
    else
      _bounceCombo = 0;

    _justLanded = false;

    // ── Calcula impulso Y ─────────────────────────────────────────────────
    float jumpY;

    if (isBounce)
    {
      float comboBonus = BounceComboBonus[_bounceCombo];
      jumpY = player.GroundSlamImpactSpeed * BounceConversionRate * (1f + comboBonus);
      jumpY = Mathf.Max(jumpY, player.JumpForce);
      move += player.transform.forward * BounceFrontImpulse;
      player.GroundSlamImpactSpeed = 0f;
    }
    else
    {
      // Pulo normal — sem GroundSlam
      float jumpMultiplier = 1f + player.CurrentJumpCount * 0.35f;
      jumpY = player.JumpForce * jumpMultiplier;
    }

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
      move.y = jumpY;
    }

    if (player.CurrentJumpCount > 0)
      player.AnimatorComponent.SetTrigger(Constants.AnimatorTriggerNames.DoubleJump);

    player.CurrentJumpCount++;
    player.EffectsWorker.PlayEffect(Constants.EffectsNames.Player.Jump, 1);
    player.MovementVector = move;
    player.LocomotionLayer.ChangeState(player.AirborneS, player);
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
