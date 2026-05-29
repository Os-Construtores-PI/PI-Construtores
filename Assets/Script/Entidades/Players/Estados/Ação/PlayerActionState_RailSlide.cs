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

  private RailObject _currentRailObject;
  private SplineContainer _currentRail;
  private float _currentRailLength;
  private float _t;
  private float _direction = 1f;
  private bool _isActive;

  [SerializeField]
  private float exitVelocityMultiplier = 1.2f;

  [SerializeField]
  private Vector3 modelOffset = new(0f, 2f, 0f);

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

    float3 localPlayerPos = _currentRail.transform.InverseTransformPoint(player.transform.position);
    SplineUtility.GetNearestPoint(
      _currentRail.Spline,
      localPlayerPos,
      out float3 nearestLocal,
      out _t
    );

    player.transform.position = _currentRail.transform.TransformPoint(nearestLocal);
    player.CharacterController.enabled = false;
    player.AnimatorComponent.SetBool(Constants.AnimatorBoolNames.IsSliding, true);
    player.CurrentJumpCount = 0;
    player.CurrentDashCount = 0;

    if (player.TryGetComponent<Rigidbody>(out var rb))
      rb.interpolation = RigidbodyInterpolation.Interpolate;

    float3 tangentLocal = _currentRail.Spline.EvaluateTangent(_t);
    Vector3 tangentWorld = _currentRail.transform.TransformDirection(tangentLocal);
    float angle = Vector3.Angle(tangentWorld, player.transform.forward);
    _direction = angle > 90f ? -1f : 1f;

    _isActive = true;
    player.LocomotionLayer.ChangeState(player.Locked, player);
  }

  public void Exit(Player player)
  {
    _currentRailObject = null;
    _currentRail = null;
    _isActive = false;

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

    float distanceThisFrame = _currentRailObject.SlideSpeed * Time.deltaTime;
    _t += distanceThisFrame * _direction / _currentRailLength;

    if (_t >= 1f || _t <= 0f)
    {
      _t = Mathf.Clamp01(_t);
      TryTransitionOrExit(player);
      return;
    }

    Vector3 position = _currentRail.transform.TransformPoint(
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

    Vector3 right = Vector3.Cross(up, tangent.normalized).normalized;
    Vector3 stableOffset =
      (up * modelOffset.y) + (right * modelOffset.x) + (tangent.normalized * modelOffset.z);
    player.transform.position = position + stableOffset;
  }

  private void TryTransitionOrExit(Player player)
  {
    if (_currentRail == null || _currentRailObject == null || !player.NextRailCanditate)
    {
      ExitWithMomentum(player);
      return;
    }

    if (!_currentRailObject.CanChain)
    {
      ExitWithMomentum(player);
      return;
    }

    var nextSpline = player.NextRailCanditate.Spline;
    if (nextSpline.Count == 0)
    {
      ExitWithMomentum(player);
      return;
    }

    Vector3 nextEntryPoint = player.NextRailCanditate.transform.TransformPoint(
      (Vector3)nextSpline.EvaluatePosition(0f)
    );

    float distance = Vector3.Distance(player.transform.position, nextEntryPoint);

    if (distance <= _currentRailObject.TransitionRadius)
    {
      TransitionToNextRail(player);
      return;
    }

    ExitWithMomentum(player);
  }

  private void TransitionToNextRail(Player player)
  {
    if (player.NextRailCanditate == null)
    {
      ExitWithMomentum(player);
      return;
    }

    var nextRailObject = player.NextRailCanditate.GetComponent<RailObject>();
    if (nextRailObject == null)
    {
      ExitWithMomentum(player);
      return;
    }

    if (!nextRailObject.TryAttachPlayer(player))
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
    float exitSpeed = _currentRailObject.SlideSpeed * exitVelocityMultiplier;
    player.MovementVector += tangent.normalized * _direction * exitSpeed;
    player.ActionLayer.ExitState(this, player);
  }
}
