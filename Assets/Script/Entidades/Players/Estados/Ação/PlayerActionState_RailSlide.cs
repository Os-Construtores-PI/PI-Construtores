using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[System.Serializable]
public class PlayerActionStateRailSlide : IState<Player>
{
  private static readonly int IsSlidingHash = Animator.StringToHash("IsSliding");

  public ActionType Type => ActionType.RailSlide;

  private readonly HashSet<ActionType> _incompatibleActions = new()
  {
    ActionType.Dash,
    ActionType.GroundSlam,
  };
  public HashSet<ActionType> IncompatibleActions => _incompatibleActions;

  [HideInInspector]
  public SplineContainer CurrentRail;

  private float _currentRailLength;
  private float _t;
  private float _direction = 1f;
  private bool _isActive;
  private Vector3 _snapStartPosition;
  private Vector3 _snapTargetPosition;
  private float _snapProgress;
  private bool _isSnapping;

  [SerializeField]
  private float _snapDuration = 0.12f;

  private float _currentSpeed;

  [SerializeField]
  private float _speedRampDuration = 0.25f;

  // ─── Exit ─────────────────────────────────────────────────────────────────
  [SerializeField]
  private float exitVelocityMultiplier = 1.35f;

  [SerializeField]
  private float exitVerticalBias = 1.6f;

  [SerializeField]
  private float exitMinHorizontalSpeed = 8f;

  [SerializeField]
  private Vector3 modelOffset = new(0f, 2f, 0f);

  // ─── Enter ────────────────────────────────────────────────────────────────
  public void Enter(Player player)
  {
    player.WantsToCancelRailSlide = false;

    Debug.Log(
      $"[RailSlide.Enter] CurrentRail={CurrentRail?.name} | SplineCount={CurrentRail?.Spline.Count}"
    );

    if (CurrentRail == null || CurrentRail.Spline.Count == 0)
    {
      Debug.LogWarning("[RailSlide.Enter] CurrentRail inválido! Saindo sem snap.");
      _isActive = false;
      player.ActionLayer.ExitState(this, player);
      return;
    }

    _currentRailLength = CurrentRail.Spline.GetLength();

    float3 localPlayerPos = CurrentRail.transform.InverseTransformPoint(player.transform.position);
    SplineUtility.GetNearestPoint(
      CurrentRail.Spline,
      localPlayerPos,
      out float3 nearestLocal,
      out _t
    );

    _snapStartPosition = player.transform.position;
    _snapTargetPosition = CurrentRail.transform.TransformPoint(nearestLocal) + ComputeOffset(_t);
    _snapProgress = 0f;
    _isSnapping = true;

    var railObject = CurrentRail.GetComponent<RailObject>();
    float entrySpeed = new Vector3(player.MovementVector.x, 0f, player.MovementVector.z).magnitude;
    float minRailSpeed = railObject != null ? railObject.SlideSpeed * 0.4f : 4f;
    _currentSpeed = Mathf.Max(entrySpeed, minRailSpeed);

    player.CharacterController.enabled = false;
    player.AnimatorComponent.SetBool(IsSlidingHash, true);
    player.CurrentJumpCount = 0;
    player.CurrentDashCount = 0;

    float3 tangentLocal = CurrentRail.Spline.EvaluateTangent(_t);
    Vector3 tangentWorld = CurrentRail.transform.TransformDirection(tangentLocal);
    float angle = Vector3.Angle(tangentWorld, player.transform.forward);
    _direction = angle > 90f ? -1f : 1f;

    _isActive = true;
    player.LocomotionLayer.ChangeState(player.Locked, player);
  }

  // ─── Exit ─────────────────────────────────────────────────────────────────
  public void Exit(Player player)
  {
    CurrentRail = null;
    _isActive = false;
    _isSnapping = false;

    player.CharacterController.enabled = true;
    player.AnimatorComponent.SetBool("IsSliding", false);
    player.transform.up = Vector3.up;
    player.CurrentJumpCount = 0;
    player.CurrentDashCount = 0;
    player.Stats.ModifyStatByMultiplierCoroutine(StatType.JumpForce, 2, 1f);

    player.LocomotionLayer.ChangeState(player.Moving, player);
  }

  public void Update(Player player)
  {
    UpdateMovement(player);
  }

  public void FixedUpdate(Player player) { }

  // ─── Update ───────────────────────────────────────────────────────────────
  private void UpdateMovement(Player player)
  {
    if (!_isActive || CurrentRail == null)
      return;

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
    _t += distanceThisFrame * _direction / _currentRailLength;

    if (_t >= 1f || _t <= 0f)
    {
      _t = Mathf.Clamp01(_t);
      ExitWithMomentum(player);
      return;
    }

    Vector3 splinePos = CurrentRail.transform.TransformPoint(
      CurrentRail.Spline.EvaluatePosition(_t)
    );
    Vector3 tangent = CurrentRail.transform.TransformDirection(
      CurrentRail.Spline.EvaluateTangent(_t)
    );
    Vector3 up = CurrentRail.transform.TransformDirection(CurrentRail.Spline.EvaluateUpVector(_t));

    if (tangent.sqrMagnitude > 0.0001f)
      player.transform.rotation = Quaternion.LookRotation(tangent * _direction, up);

    Vector3 finalPos = splinePos + ComputeOffsetFromVectors(up, tangent);

    if (_isSnapping)
    {
      _snapProgress += Time.deltaTime / _snapDuration;
      float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(_snapProgress), 3f); // EaseOutCubic
      player.transform.position = Vector3.Lerp(_snapStartPosition, finalPos, ease);

      if (_snapProgress >= 1f)
        _isSnapping = false;
    }
    else
    {
      player.transform.position = finalPos;
    }
  }

  // ─── Saída com Momentum ────────────────────────────────────────────────────
  private void ExitWithMomentum(Player player)
  {
    if (CurrentRail == null)
    {
      player.ActionLayer.ExitState(this, player);
      return;
    }

    Vector3 tangent = CurrentRail.transform.TransformDirection(
      CurrentRail.Spline.EvaluateTangent(_t)
    );
    Vector3 exitDir = tangent.normalized * _direction;
    float exitSpeed = _currentSpeed * exitVelocityMultiplier;

    Vector3 horizontal = new(exitDir.x, 0f, exitDir.z);
    if (horizontal.sqrMagnitude < 0.001f)
      horizontal = player.transform.forward;

    horizontal =
      horizontal.normalized * Mathf.Max(horizontal.magnitude * exitSpeed, exitMinHorizontalSpeed);

    float verticalComponent = exitDir.y * exitSpeed * exitVerticalBias;

    player.MovementVector = new Vector3(
      horizontal.x,
      Mathf.Max(verticalComponent, player.MovementVector.y),
      horizontal.z
    );

    player.ActionLayer.ExitState(this, player);
  }

  // ─── Helpers ──────────────────────────────────────────────────────────────
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
    return (up * modelOffset.y) + (right * modelOffset.x) + (tangent.normalized * modelOffset.z);
  }
}
