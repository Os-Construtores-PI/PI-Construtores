using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[DefaultExecutionOrder(-100)]
public class CameraDirector : MonoBehaviour
{
  // =========================================================
  // CONSTANTS
  // =========================================================

  private const float MinTangentSqr = 0.0001f;

  // =========================================================
  // INSPECTOR
  // =========================================================

  [Header("Rail")]
  [SerializeField]
  private SplineContainer _cameraPath;

  [Tooltip("Check if players move opposite to the spline direction.")]
  [SerializeField]
  private bool _flipDirection;

  [Tooltip("Extra distance another branch must win to switch splines.")]
  [SerializeField]
  private float _branchSwitchMargin = 1.5f;

  [Tooltip("Ignore height when choosing a branch, avoiding unwanted switches during jumps.")]
  [SerializeField]
  private bool _ignoreHeightOnBranchSelection = true;

  [SerializeField]
  private int _nearestResolution = 16;

  [SerializeField]
  private int _nearestIterations = 4;

  [Header("Framing")]
  [Tooltip("Lateral offset relative to the rail.")]
  [SerializeField]
  private float _lateralOffset;

  [Tooltip("Camera height above the player (follows jumps).")]
  [SerializeField]
  private float _heightAboveRail = 2f;

  [Tooltip("Distance behind the player along the rail.")]
  [SerializeField]
  private float _distanceBehind = 6f;

  [Header("Smoothing")]
  [Tooltip("Time to follow the player along the rail. 0 = instant.")]
  [SerializeField]
  private float _followSmoothTime = 0.1f;

  [Tooltip("Transition time between splines.")]
  [SerializeField]
  private float _transitionSmoothTime = 0.35f;

  [SerializeField]
  private float _rotationSpeed = 6f;

  [Header("Debug")]
  [SerializeField]
  private bool _drawGizmos = true;

  // =========================================================
  // AUXILIARIES
  // =========================================================

  private class CameraEntry
  {
    public Transform Target;
    public Transform Rig;
    public int SplineIndex = -1;
    public float Distance;
    public float DistanceVelocity;
    public Vector3 LastRailPosition;
    public Vector3 LastForward = Vector3.forward;
    public bool Initialized;
  }

  private readonly List<CameraEntry> _entries = new();

  // =========================================================
  // SETUP
  // =========================================================

  public void Register(Transform target, Transform rig)
  {
    if (target == null || rig == null)
      return;

    if (_entries.Exists(e => e.Target == target))
      return;

    _entries.Add(new CameraEntry { Target = target, Rig = rig });
  }

  public void Register(Transform target, Camera camera)
  {
    if (camera != null)
      Register(target, camera.transform);
  }

  public void Unregister(Transform target)
  {
    _entries.RemoveAll(e => e.Target == target);
  }

  public void SetPath(SplineContainer path)
  {
    _cameraPath = path;
    foreach (CameraEntry entry in _entries)
    {
      entry.SplineIndex = -1;
      entry.Initialized = false;
    }
  }

  // =========================================================
  // LIFECYCLE
  // =========================================================

  private void LateUpdate()
  {
    if (_cameraPath == null || _cameraPath.Splines.Count == 0)
      return;

    for (int i = _entries.Count - 1; i >= 0; i--)
    {
      CameraEntry entry = _entries[i];

      if (entry.Target == null || entry.Rig == null)
      {
        _entries.RemoveAt(i);
        continue;
      }

      UpdateEntry(entry);
    }
  }

  // =========================================================
  // CORE
  // =========================================================

  private void UpdateEntry(CameraEntry entry)
  {
    Vector3 targetPosition = entry.Target.position;
    FindNearest(targetPosition, entry.SplineIndex, out int index, out float targetT);

    Spline spline = _cameraPath.Splines[index];
    float length = Mathf.Max(spline.GetLength(), 0.001f);
    float targetDistance = spline.ConvertIndexUnit(
      targetT,
      PathIndexUnit.Normalized,
      PathIndexUnit.Distance
    );

    bool switched = entry.Initialized && index != entry.SplineIndex;

    float desiredDistance = targetDistance - _distanceBehind;

    if (!entry.Initialized || switched)
    {
      entry.SplineIndex = index;
      entry.Distance = desiredDistance;
      entry.DistanceVelocity = 0f;
    }
    else
    {
      float delta = WrapDelta(desiredDistance - entry.Distance, length, spline.Closed);
      float step = _followSmoothTime <= 0f
        ? delta
        : Mathf.SmoothDamp(0f, delta, ref entry.DistanceVelocity, _followSmoothTime);
      entry.Distance = NormalizeDistance(entry.Distance + step, length, spline.Closed);
    }

    EvaluateRail(
      spline,
      length,
      entry.Distance,
      entry.LastForward,
      out Vector3 railPosition,
      out Vector3 forward
    );
    entry.LastForward = forward;
    entry.LastRailPosition = railPosition;

    Vector3 right = Vector3.Cross(Vector3.up, forward);
    if (right.sqrMagnitude < 1e-6f) right = Vector3.right;
    else right.Normalize();

    Vector3 finalPosition = railPosition + right * _lateralOffset;
    finalPosition.y = targetPosition.y + _heightAboveRail;

    Quaternion desiredRotation = Quaternion.LookRotation(forward, Vector3.up);

    if (!entry.Initialized)
    {
      entry.Rig.SetPositionAndRotation(finalPosition, desiredRotation);
      entry.Initialized = true;
      return;
    }

    float rotationBlend = 1f - Mathf.Exp(-_rotationSpeed * Time.deltaTime);
    entry.Rig.SetPositionAndRotation(
      finalPosition,
      Quaternion.Slerp(entry.Rig.rotation, desiredRotation, rotationBlend)
    );
  }

  private void EvaluateRail(
    Spline spline,
    float length,
    float distance,
    Vector3 fallbackForward,
    out Vector3 position,
    out Vector3 forward
  )
  {
    float clampedDistance = NormalizeDistance(distance, length, spline.Closed);
    float t = spline.ConvertIndexUnit(
      clampedDistance,
      PathIndexUnit.Distance,
      PathIndexUnit.Normalized
    );

    SplineUtility.Evaluate(spline, t, out float3 localPoint, out float3 localTangent, out float3 _);

    Transform pathTransform = _cameraPath.transform;
    position = pathTransform.TransformPoint(localPoint);

    Vector3 tangent = pathTransform.TransformDirection(localTangent);
    tangent.y = 0f;

    if (tangent.sqrMagnitude < MinTangentSqr)
    {
      forward = fallbackForward;
      return;
    }

    tangent.Normalize();
    forward = _flipDirection ? -tangent : tangent;
  }

  // =========================================================
  // NEAREST SEARCH
  // =========================================================

  private void FindNearest(Vector3 worldPoint, int currentIndex, out int bestIndex, out float bestT)
  {
    Transform pathTransform = _cameraPath.transform;
    float3 local = pathTransform.InverseTransformPoint(worldPoint);
    IReadOnlyList<Spline> splines = _cameraPath.Splines;

    if (currentIndex >= splines.Count)
      currentIndex = -1;

    bestIndex = 0;
    bestT = 0f;
    float bestDistance = float.MaxValue;

    float currentDistance = float.MaxValue;
    float currentT = 0f;

    for (int i = 0; i < splines.Count; i++)
    {
      float3 query = local;

      SplineUtility.GetNearestPoint(
        splines[i],
        query,
        out float3 nearest,
        out float t,
        _nearestResolution,
        _nearestIterations
      );

      if (_ignoreHeightOnBranchSelection)
      {
        query.y = nearest.y;
        SplineUtility.GetNearestPoint(
          splines[i],
          query,
          out nearest,
          out t,
          _nearestResolution,
          _nearestIterations
        );
      }

      Vector3 offset = pathTransform.TransformPoint(nearest) - worldPoint;
      if (_ignoreHeightOnBranchSelection)
        offset.y = 0f;

      float distance = offset.magnitude;

      if (i == currentIndex)
      {
        currentDistance = distance;
        currentT = t;
      }

      if (distance < bestDistance)
      {
        bestDistance = distance;
        bestIndex = i;
        bestT = t;
      }
    }

    if (currentIndex >= 0 && currentIndex != bestIndex)
    {
      if (currentDistance <= bestDistance + _branchSwitchMargin)
      {
        bestIndex = currentIndex;
        bestT = currentT;
      }
    }
  }

  // =========================================================
  // MATH HELPERS
  // =========================================================

  private static float WrapDelta(float delta, float length, bool closed)
  {
    if (!closed)
      return delta;

    float half = length * 0.5f;
    return Mathf.Repeat(delta + half, length) - half;
  }

  private static float NormalizeDistance(float distance, float length, bool closed) =>
    closed ? Mathf.Repeat(distance, length) : Mathf.Clamp(distance, 0f, length);

  // =========================================================
  // GIZMOS
  // =========================================================

  private void OnDrawGizmos()
  {
    if (!_drawGizmos || _cameraPath == null)
      return;

    Gizmos.color = Color.cyan;
    foreach (Spline spline in _cameraPath.Splines)
    {
      const int steps = 32;
      Vector3 prev = _cameraPath.transform.TransformPoint(spline.EvaluatePosition(0f));

      for (int i = 1; i <= steps; i++)
      {
        float t = i / (float)steps;
        Vector3 curr = _cameraPath.transform.TransformPoint(spline.EvaluatePosition(t));
        Gizmos.DrawLine(prev, curr);
        prev = curr;
      }
    }

    Gizmos.color = Color.yellow;
    foreach (CameraEntry entry in _entries)
    {
      if (entry.Rig != null && entry.Target != null)
      {
        Gizmos.DrawWireSphere(entry.Rig.position, 0.3f);
        Gizmos.DrawLine(entry.Rig.position, entry.Target.position);
      }
    }
  }
}
