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
  private bool _isExiting;

  private CancellationTokenSource _slideCts;
  private CancellationTokenSource _linkedCts;
  private Task _activeSlideTask;

  [SerializeField]
  private float _snapDuration = 0.12f;

  [SerializeField]
  private float _speedRampDuration = 0.25f;

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

  public void SetRail(SplineContainer rail) => CurrentRail = rail;

  public void RequestCancel() => _slideCts?.Cancel();

  public void Enter(Player player)
  {
    _isExiting = false;

    if (CurrentRail == null || CurrentRail.Spline.Count == 0)
    {
      Debug.LogWarning("[RailSlide.Enter] Rail inválido! Saindo imediatamente.");
      player.ActionLayer.ExitState(this, player);
      return;
    }

    _activeSlideTask = RunRailSlideLifecycleAsync(player);
  }

  public void Exit(Player player)
  {
    if (_isExiting)
      return;
    _isExiting = true;

    _slideCts?.Cancel();

    CleanupPlayerState(player);
  }

  public void Update(Player player) { }

  public void FixedUpdate(Player player) { }

  private async Task RunRailSlideLifecycleAsync(Player player)
  {
    _slideCts = new CancellationTokenSource();
    var playerLifetime = player.GetCancellationToken();
    _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_slideCts.Token, playerLifetime);

    try
    {
      InitializeRailData(player);
      SetupPlayerForSlide(player);

      await SnapToRailAsync(player, _linkedCts.Token);

      await SlideAlongRailAsync(player, _linkedCts.Token);

      if (!_linkedCts.Token.IsCancellationRequested && !_isExiting)
      {
        await ExitWithMomentumAsync(player);
      }
    }
    catch (OperationCanceledException)
    {
      Debug.Log("[RailSlide] Slide cancelado (input do jogador ou troca de state)");
      if (!_isExiting && player.WantsToCancelRailSlide)
      {
        player.WantsToCancelRailSlide = false;
        await ExitWithMomentumAsync(player);
      }
    }
    catch (Exception ex)
    {
      Debug.LogError($"[RailSlide] Erro inesperado: {ex.Message}");
    }
    finally
    {
      CleanupPlayerState(player);
      CleanupTokens();
      CurrentRail = null;
    }
  }

  private void InitializeRailData(Player player)
  {
    _currentRailLength = CurrentRail.Spline.GetLength();

    float3 localPlayerPos = CurrentRail.transform.InverseTransformPoint(player.transform.position);
    SplineUtility.GetNearestPoint(
      CurrentRail.Spline,
      localPlayerPos,
      out float3 nearestLocal,
      out _railProgress
    );

    float3 tangentLocal = CurrentRail.Spline.EvaluateTangent(_railProgress);
    Vector3 tangentWorld = CurrentRail.transform.TransformDirection(tangentLocal);
    float angle = Vector3.Angle(tangentWorld, player.transform.forward);
    _direction = angle > 90f ? -1f : 1f;

    var railObject = CurrentRail.GetComponent<RailObject>();
    float entrySpeed = new Vector3(player.MovementVector.x, 0f, player.MovementVector.z).magnitude;
    float minRailSpeed = railObject != null ? railObject.SlideSpeed * 0.4f : 4f;
    _currentSpeed = Mathf.Max(entrySpeed, minRailSpeed);
  }

  private void SetupPlayerForSlide(Player player)
  {
    player.CharacterController.enabled = false;
    player.AnimatorComponent.SetBool(IsSlidingHash, true);
    player.CurrentJumpCount = 0;
    player.CurrentDashCount = 0;
    player.SpeedLines?.Invoke(true);
    player.LocomotionLayer.ChangeState(player.Locked, player);
  }

  private async Task SnapToRailAsync(Player player, CancellationToken ct)
  {
    Vector3 startPos = player.transform.position;
    Vector3 targetPos =
      CurrentRail.transform.TransformPoint(CurrentRail.Spline.EvaluatePosition(_railProgress))
      + ComputeOffset(_railProgress);

    float elapsed = 0f;

    while (elapsed < _snapDuration)
    {
      ct.ThrowIfCancellationRequested();

      elapsed += Time.deltaTime;
      float t = Mathf.Clamp01(elapsed / _snapDuration);
      float ease = 1f - Mathf.Pow(1f - t, 3f);

      player.transform.position = Vector3.Lerp(startPos, targetPos, ease);
      await Task.Yield();
    }

    player.transform.position = targetPos;
  }

  private async Task SlideAlongRailAsync(Player player, CancellationToken ct)
  {
    var railObject = CurrentRail.GetComponent<RailObject>();
    float targetSpeed = railObject != null ? railObject.SlideSpeed : 10f;

    while (_railProgress > 0f && _railProgress < 1f)
    {
      ct.ThrowIfCancellationRequested();

      _currentSpeed = Mathf.MoveTowards(
        _currentSpeed,
        targetSpeed,
        targetSpeed / _speedRampDuration * Time.deltaTime
      );

      float distanceThisFrame = _currentSpeed * Time.deltaTime;
      _railProgress += distanceThisFrame * _direction / _currentRailLength;

      UpdatePlayerTransform(player);

      OnScoreAwarded?.Invoke(_slideIncrementScore);

      await Task.Yield();
    }

    _railProgress = Mathf.Clamp01(_railProgress);
  }

  private void UpdatePlayerTransform(Player player)
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

    player.transform.position = splinePos + ComputeOffsetFromVectors(up, tangent);
  }

  private async Task ExitWithMomentumAsync(Player player)
  {
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

    player.MovementVector = new Vector3(
      horizontal.x,
      Mathf.Max(verticalComponent, player.MovementVector.y),
      horizontal.z
    );

    await ApplyExitBuffAsync(player);

    if (!_isExiting)
    {
      _isExiting = true;
      player.ActionLayer.ExitState(this, player);
    }
  }

  private async Task ApplyExitBuffAsync(Player player)
  {
    try
    {
      await player.Stats.ApplyMultiplierAsync(StatType.JumpForce, 2f, 1f, _linkedCts.Token);
    }
    catch (OperationCanceledException) { }
  }

  private void CleanupPlayerState(Player player)
  {
    if (player == null)
      return;

    player.CharacterController.enabled = true;
    player.AnimatorComponent.SetBool(IsSlidingHash, false);
    player.transform.up = Vector3.up;
    player.CurrentJumpCount = 0;
    player.CurrentDashCount = 0;
    player.SpeedLines?.Invoke(false);
    player.LocomotionLayer.ChangeState(player.Moving, player);
  }

  private void CleanupTokens()
  {
    _slideCts?.Dispose();
    _slideCts = null;

    _linkedCts?.Dispose();
    _linkedCts = null;
  }

  public void Dispose()
  {
    _slideCts?.Cancel();
    CleanupTokens();
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
