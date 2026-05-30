using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[System.Serializable]
public class PlayerActionStateRailSlide : IState<Player>
{
  public ActionType Type => ActionType.RailSlide;

  private readonly HashSet<ActionType> _incompatibleActions = new()
  {
    ActionType.Dash,
    ActionType.GroundSlam,
  };
  public HashSet<ActionType> IncompatibleActions => _incompatibleActions;

  [HideInInspector]
  public Vector3 RailExitMomentum = Vector3.zero;

  private RailObject _currentRailObject;
  private SplineContainer _currentRail;
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

  // ─── Speed suave na entrada ───────────────────────────────────────────────
  // O player pode entrar no rail com velocidade baixa; aceleramos até SlideSpeed
  // em _speedRampDuration para evitar o "freio brusco" ou "turbo instantâneo".
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

    if (player.CurrentRail == null || player.CurrentRail.Spline.Count == 0)
    {
      _isActive = false;
      player.ActionLayer.ExitState(this, player);
      return;
    }

    _currentRailObject = player.CurrentRail.GetComponent<RailObject>();
    if (_currentRailObject == null)
    {
      player.ActionLayer.ExitState(this, player);
      return;
    }

    _currentRail = player.CurrentRail;
    _currentRailLength = _currentRail.Spline.GetLength();

    // Ponto mais próximo no spline
    float3 localPlayerPos = _currentRail.transform.InverseTransformPoint(player.transform.position);
    SplineUtility.GetNearestPoint(
      _currentRail.Spline,
      localPlayerPos,
      out float3 nearestLocal,
      out _t
    );

    _snapStartPosition = player.transform.position;
    _snapTargetPosition = _currentRail.transform.TransformPoint(nearestLocal) + ComputeOffset(_t);
    _snapProgress = 0f;
    _isSnapping = true;

    float entrySpeed = new Vector3(player.MovementVector.x, 0f, player.MovementVector.z).magnitude;
    _currentSpeed = Mathf.Max(entrySpeed, _currentRailObject.SlideSpeed * 0.4f);

    player.CharacterController.enabled = false;
    player.AnimatorComponent.SetBool(Constants.AnimatorBoolNames.IsSliding, true);
    player.CurrentJumpCount = 0;
    player.CurrentDashCount = 0;

    if (player.TryGetComponent<Rigidbody>(out var rb))
      rb.interpolation = RigidbodyInterpolation.Interpolate;

    // Direção de travessia
    float3 tangentLocal = _currentRail.Spline.EvaluateTangent(_t);
    Vector3 tangentWorld = _currentRail.transform.TransformDirection(tangentLocal);
    float angle = Vector3.Angle(tangentWorld, player.transform.forward);
    _direction = angle > 90f ? -1f : 1f;

    _isActive = true;
    player.LocomotionLayer.ChangeState(player.Locked, player);
  }

  // ─── Exit ─────────────────────────────────────────────────────────────────

  public void Exit(Player player)
  {
    _currentRailObject = null;
    _currentRail = null;
    _isActive = false;
    _isSnapping = false;

    player.CharacterController.enabled = true;
    player.AnimatorComponent.SetBool(Constants.AnimatorBoolNames.IsSliding, false);
    player.transform.up = Vector3.up;
    player.CurrentJumpCount = 0;
    player.CurrentDashCount = 0;

    if (player.TryGetComponent<Rigidbody>(out var rb))
      rb.interpolation = RigidbodyInterpolation.Interpolate;

    player.LocomotionLayer.ChangeState(player.Moving, player);
  }

  public void Update(Player player) => UpdateMovement(player);

  public void FixedUpdate(Player player) { }

  // ─── Update ───────────────────────────────────────────────────────────────

  private void UpdateMovement(Player player)
  {
    if (!_isActive || _currentRail == null)
      return;

    if (player.WantsToCancelRailSlide)
    {
      player.WantsToCancelRailSlide = false;
      TryTransitionOrExit(player);
      return;
    }

    _currentSpeed = Mathf.MoveTowards(
      _currentSpeed,
      _currentRailObject.SlideSpeed,
      (_currentRailObject.SlideSpeed / _speedRampDuration) * Time.deltaTime
    );

    float distanceThisFrame = _currentSpeed * Time.deltaTime;
    _t += distanceThisFrame * _direction / _currentRailLength;

    if (_t >= 1f || _t <= 0f)
    {
      _t = Mathf.Clamp01(_t);
      TryTransitionOrExit(player);
      return;
    }

    Vector3 splinePos = _currentRail.transform.TransformPoint(
      _currentRail.Spline.EvaluatePosition(_t)
    );
    Vector3 tangent = _currentRail.transform.TransformDirection(
      _currentRail.Spline.EvaluateTangent(_t)
    );
    Vector3 up = _currentRail.transform.TransformDirection(
      _currentRail.Spline.EvaluateUpVector(_t)
    );

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

  // ─── Transição / Saída ────────────────────────────────────────────────────

  private void TryTransitionOrExit(Player player)
  {
    if (_currentRail == null || _currentRailObject == null || !_currentRailObject.CanChain)
    {
      ExitWithMomentum(player);
      return;
    }

    RailObject.RailDirection travelDir =
      _direction > 0f ? RailObject.RailDirection.Forward : RailObject.RailDirection.Backward;

    RailObject nextRailObject = _currentRailObject.GetNextCandidateForDirection(travelDir);

    if (
      nextRailObject == null
      || nextRailObject == _currentRailObject
      || nextRailObject.GetComponent<SplineContainer>() == null
    )
    {
      ExitWithMomentum(player);
      return;
    }

    var nextSpline = nextRailObject.GetComponent<SplineContainer>();
    if (nextSpline == null || nextSpline.Spline.Count == 0)
    {
      ExitWithMomentum(player);
      return;
    }

    Vector3 playerPos = player.transform.position;
    Vector3 entryA = nextSpline.transform.TransformPoint(nextSpline.Spline.EvaluatePosition(0f));
    Vector3 entryB = nextSpline.transform.TransformPoint(nextSpline.Spline.EvaluatePosition(1f));

    float distA = Vector3.Distance(playerPos, entryA);
    float distB = Vector3.Distance(playerPos, entryB);
    float closestDist = Mathf.Min(distA, distB);

    if (closestDist <= _currentRailObject.TransitionRadius)
    {
      RailObject.RailDirection entryDir =
        distA <= distB ? RailObject.RailDirection.Forward : RailObject.RailDirection.Backward;

      TransitionToNextRail(player, nextRailObject, entryDir);
      return;
    }

    ExitWithMomentum(player);
  }

  private void TransitionToNextRail(
    Player player,
    RailObject nextRailObject,
    RailObject.RailDirection entryDir
  )
  {
    if (!nextRailObject.TryAttachPlayer(player, entryDir))
    {
      Debug.LogWarning($"[RailSlide] TryAttachPlayer failed for {nextRailObject.name}");
      ExitWithMomentum(player);
      return;
    }

    player.NextRailCanditate = null;
    player.ActionLayer.ExitState(this, player);
    player.ActionLayer.PushState(player.RailSlide, player);
  }

  private void ExitWithMomentum(Player player)
  {
    if (_currentRail == null || _currentRailObject == null)
    {
      player.ActionLayer.ExitState(this, player);
      return;
    }

    Vector3 tangent = _currentRail.transform.TransformDirection(
      _currentRail.Spline.EvaluateTangent(_t)
    );
    Vector3 exitDir = tangent.normalized * _direction;
    float exitSpeed = _currentSpeed * exitVelocityMultiplier;

    Vector3 horizontal = new Vector3(exitDir.x, 0f, exitDir.z);
    if (horizontal.sqrMagnitude < 0.001f)
      horizontal = player.transform.forward;

    horizontal =
      horizontal.normalized * Mathf.Max(horizontal.magnitude * exitSpeed, exitMinHorizontalSpeed);

    float verticalComponent = exitDir.y * exitSpeed * exitVerticalBias;

    Vector3 exitVelocity = new Vector3(horizontal.x, verticalComponent, horizontal.z);

    RailExitMomentum = exitVelocity;
    player.Moving.RailExitMomentumTimer.Start(.3f);

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
    Vector3 tangent = _currentRail.transform.TransformDirection(
      _currentRail.Spline.EvaluateTangent(t)
    );
    Vector3 up = _currentRail.transform.TransformDirection(_currentRail.Spline.EvaluateUpVector(t));
    return ComputeOffsetFromVectors(up, tangent);
  }

  private Vector3 ComputeOffsetFromVectors(Vector3 up, Vector3 tangent)
  {
    Vector3 right = Vector3.Cross(up, tangent.normalized).normalized;
    return (up * modelOffset.y) + (right * modelOffset.x) + (tangent.normalized * modelOffset.z);
  }
}
