using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[System.Serializable]
public class PlayerActionStateRailSlide : IPlayerState<Player>, IDisposable
{
  private static readonly int IsSlidingHash = Animator.StringToHash("IsSliding");

  public PlayerActionType Type => PlayerActionType.RailSlide;

  private readonly HashSet<PlayerActionType> _incompatibleActions = new()
  {
    PlayerActionType.Dash,
    PlayerActionType.GroundSlam,
  };
  public HashSet<PlayerActionType> IncompatibleActions => _incompatibleActions;

  public SplineContainer CurrentRail { get; set; }

  private float _currentRailLength;
  private float _railProgress;
  private float _direction = 1f;
  private float _currentSpeed;
  private bool _isActive;
  private bool _isSnapping;
  private bool _isExiting;

  private bool _cancelRequested;

  // Snap
  private Vector3 _snapStartPosition;
  private Vector3 _snapTargetPosition;
  private float _snapProgress;

  [SerializeField]
  private float _snapDuration = 0.12f;

  [SerializeField]
  private float _speedRampDuration = 0.25f;

  // Exit
  [SerializeField]
  private float _exitVelocityMultiplier = 1.35f;

  [SerializeField]
  private float _exitVerticalBias = 1.6f;

  [SerializeField]
  private float _exitMinHorizontalSpeed = 8f;

  [SerializeField]
  private Vector3 _modelOffset = new(0f, 2f, 0f);

  [SerializeField]
  private int _slideIncrementScore = 1;

  public event Action<int> OnScoreAwarded;

  private CancellationTokenSource _exitBuffCts;

  private Vector3 _targetPosition;
  private bool _hasTargetPosition = false;

  public void SetRail(SplineContainer rail) => CurrentRail = rail;

  public void RequestCancel() => _cancelRequested = true;

  public void Enter(Player player)
  {
    _cancelRequested = false;
    player.WantsToCancelRailSlide = false;
    _isExiting = false;
    _hasTargetPosition = false;

    if (CurrentRail == null || CurrentRail.Spline.Count == 0)
    {
      Debug.LogWarning("[RailSlide.Enter] Rail inválido! Saindo imediatamente.");
      player.ActionLayer.ExitState(this, player);
      return;
    }

    _currentRailLength = CurrentRail.Spline.GetLength();

    float3 localPlayerPos = CurrentRail.transform.InverseTransformPoint(player.transform.position);
    SplineUtility.GetNearestPoint(
      CurrentRail.Spline,
      localPlayerPos,
      out float3 nearestLocal,
      out _railProgress
    );

    _snapStartPosition = player.transform.position;
    _snapTargetPosition =
      CurrentRail.transform.TransformPoint(nearestLocal) + ComputeOffset(_railProgress);
    _snapProgress = 0f;
    _isSnapping = true;

    Vector3 currentVel = player.Motor.Engine.BaseVelocity;
    float entrySpeed = new Vector3(currentVel.x, 0f, currentVel.z).magnitude;

    float minRailSpeed = CurrentRail.TryGetComponent(out RailObject railObj)
      ? railObj.SlideSpeed * 0.4f
      : 4f;
    _currentSpeed = Mathf.Max(entrySpeed, minRailSpeed);

    float3 tangentLocal = CurrentRail.Spline.EvaluateTangent(_railProgress);
    Vector3 tangentWorld = CurrentRail.transform.TransformDirection(tangentLocal);
    float angle = Vector3.Angle(tangentWorld, player.transform.forward);
    _direction = angle > 90f ? -1f : 1f;

    SetupPlayerForSlide(player);
    _isActive = true;
  }

  public void Exit(Player player)
  {
    if (_isExiting)
      return;
    _isExiting = true;
    _isActive = false;
    _isSnapping = false;
    _cancelRequested = false;
    _hasTargetPosition = false;

    _exitBuffCts?.Cancel();

    CleanupPlayerState(player);
    CurrentRail = null;
  }

  public void Update(Player player)
  {
    if (!_isActive || CurrentRail == null)
      return;

    if (_cancelRequested)
    {
      _cancelRequested = false;
      player.WantsToCancelRailSlide = false;
      ExitWithMomentum(player);
      return;
    }

    if (player.WantsToCancelRailSlide)
    {
      player.WantsToCancelRailSlide = false;
      ExitWithMomentum(player);
      return;
    }

    var railObject = CurrentRail.GetComponent<RailObject>();
    float targetSpeed = railObject != null ? railObject.SlideSpeed : 10f;

    _currentSpeed = Mathf.MoveTowards(
      _currentSpeed,
      targetSpeed,
      targetSpeed / _speedRampDuration * Time.deltaTime
    );

    float distanceThisFrame = _currentSpeed * Time.deltaTime;
    _railProgress += distanceThisFrame * _direction / _currentRailLength;

    if (_railProgress >= 1f || _railProgress <= 0f)
    {
      _railProgress = Mathf.Clamp01(_railProgress);
      ExitWithMomentum(player);
      return;
    }

    CalculateTargetPosition(player);
    OnScoreAwarded?.Invoke(_slideIncrementScore);
  }

  public void FixedUpdate(Player player) { }

  public bool UpdateKCCVelocity(Player player, ref Vector3 currentVelocity, float deltaTime)
  {
    currentVelocity = Vector3.zero;
    return true;
  }

  public void ApplyTargetPosition(Player player)
  {
    if (!_hasTargetPosition)
      return;

    player.Motor.Engine.SetPosition(_targetPosition);
    _hasTargetPosition = false;
  }

  // ═══════════════════════════════════════════════════════════════════════

  private void SetupPlayerForSlide(Player player)
  {
    player.AnimatorComponent.SetBool(IsSlidingHash, true);
    player.CurrentJumpCount = 0;
    player.CurrentDashCount = 0;
    player.SpeedLines?.Invoke(true);
    player.LocomotionLayer.ChangeState(player.Locked, player);
  }

  private void CalculateTargetPosition(Player player)
  {
    Vector3 splinePos = CurrentRail.transform.TransformPoint(
      CurrentRail.Spline.EvaluatePosition(_railProgress)
    );
    Vector3 tangent = CurrentRail.transform.TransformDirection(
      CurrentRail.Spline.EvaluateTangent(_railProgress)
    );
    Vector3 up = CurrentRail.transform.TransformDirection(
      CurrentRail.Spline.EvaluateUpVector(_railProgress)
    );

    if (tangent.sqrMagnitude > 0.0001f)
    {
      player.transform.rotation = Quaternion.LookRotation(tangent * _direction, up);
    }

    Vector3 finalPos = splinePos + ComputeOffsetFromVectors(up, tangent);

    if (_isSnapping)
    {
      _snapProgress += Time.deltaTime / _snapDuration;
      float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(_snapProgress), 3f);
      _targetPosition = Vector3.Lerp(_snapStartPosition, finalPos, ease);

      if (_snapProgress >= 1f)
        _isSnapping = false;
    }
    else
    {
      _targetPosition = finalPos;
    }

    _hasTargetPosition = true;
  }

  private void ExitWithMomentum(Player player)
  {
    if (CurrentRail == null || _isExiting)
    {
      if (!_isExiting)
        player.ActionLayer.ExitState(this, player);
      return;
    }

    Vector3 tangent = CurrentRail.transform.TransformDirection(
      CurrentRail.Spline.EvaluateTangent(_railProgress)
    );
    Vector3 exitDir = tangent.normalized * _direction;
    float exitSpeed = _currentSpeed * _exitVelocityMultiplier;

    Vector3 horizontal = new(exitDir.x, 0f, exitDir.z);
    if (horizontal.sqrMagnitude < 0.001f)
      horizontal = player.transform.forward;

    horizontal =
      horizontal.normalized * Mathf.Max(horizontal.magnitude * exitSpeed, _exitMinHorizontalSpeed);

    float verticalComponent = exitDir.y * exitSpeed * _exitVerticalBias;

    Vector3 exitVelocity = new Vector3(
      horizontal.x,
      Mathf.Max(verticalComponent, player.Motor.Engine.BaseVelocity.y),
      horizontal.z
    );

    player.Motor.Engine.BaseVelocity = exitVelocity;

    ApplyExitBuffAsync(player);

    player.ActionLayer.ExitState(this, player);
  }

  private async void ApplyExitBuffAsync(Player player)
  {
    _exitBuffCts?.Dispose();
    _exitBuffCts = new CancellationTokenSource();

    try
    {
      var playerLifetime = player.GetCancellationToken();
      using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
        _exitBuffCts.Token,
        playerLifetime
      );

      await player.Stats.ApplyMultiplierAsync(StatType.JumpForce, 2f, 1f, linkedCts.Token);
    }
    catch (OperationCanceledException)
    {
      Debug.Log("[RailSlide] Buff de saída cancelado.");
    }
    catch (Exception ex)
    {
      Debug.LogError($"[RailSlide] Erro ao aplicar buff: {ex.Message}");
    }
  }

  private void CleanupPlayerState(Player player)
  {
    if (player == null)
      return;

    player.AnimatorComponent.SetBool(IsSlidingHash, false);
    player.transform.up = Vector3.up;
    player.CurrentJumpCount = 0;
    player.CurrentDashCount = 0;
    player.SpeedLines?.Invoke(false);
    player.LocomotionLayer.ChangeState(player.Moving, player);
  }

  public void Dispose()
  {
    _isActive = false;
    _isSnapping = false;
    _cancelRequested = false;
    _hasTargetPosition = false;
    _exitBuffCts?.Cancel();
    _exitBuffCts?.Dispose();
    CurrentRail = null;
  }

  private Vector3 ComputeOffset(float t)
  {
    Vector3 tangent = CurrentRail.transform.TransformDirection(
      CurrentRail.Spline.EvaluateTangent(t)
    );
    Vector3 up = CurrentRail.transform.TransformDirection(CurrentRail.Spline.EvaluateUpVector(t));
    return ComputeOffsetFromVectors(up, tangent);
  }

  private Vector3 ComputeOffsetFromVectors(Vector3 up, Vector3 tangent)
  {
    Vector3 right = Vector3.Cross(up, tangent.normalized).normalized;
    return (up * _modelOffset.y) + (right * _modelOffset.x) + (tangent.normalized * _modelOffset.z);
  }
}
