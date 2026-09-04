using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class RailObject : MonoBehaviour
{
  [Header("Configurações do Rail")]
  [SerializeField]
  private float slideSpeed = 12f;

  [SerializeField]
  private RailDirection defaultDirection = RailDirection.Forward;

  public float SlideSpeed => slideSpeed;
  public RailDirection DefaultDirection => defaultDirection;

  [SerializeField]
  private float _lockRange = 50;

  [SerializeField, Range(0, 100)]
  private float _boostGrace = 0;

  [Header("Cooldown de Reentrada")]
  [SerializeField]
  private float _reEntryCooldown = 0.35f;
  private float _cooldownUntil = -1f;
  public bool IsOnCooldown => Time.time < _cooldownUntil;

  public void StartReEntryCooldown() => _cooldownUntil = Time.time + _reEntryCooldown;

  [Header("Colliders de Detecção (Scanner)")]
  [SerializeField]
  private float _segmentLength = 3f;

  [SerializeField]
  private float _segmentRadius = 1.5f;

  private SplineContainer _spline;

  public enum RailDirection
  {
    Forward,
    Backward,
  }

  private void Awake()
  {
    _spline = GetComponent<SplineContainer>();
    BuildSegmentColliders();
  }

  private void Start()
  {
    RailManager.Register(this);
  }

  public bool GetNearestPointOnSpline(Vector3 worldPosition, out Vector3 nearestPoint, out float t)
  {
    nearestPoint = Vector3.zero;
    t = 0f;
    if (_spline == null || _spline.Spline.Count == 0)
      return false;
    float3 localPos = _spline.transform.InverseTransformPoint(worldPosition);
    SplineUtility.GetNearestPoint(_spline.Spline, localPos, out float3 nearestLocal, out t);
    nearestPoint = _spline.transform.TransformPoint(nearestLocal);
    return true;
  }

  public Vector3 GetTangentAt(float t)
  {
    if (_spline == null || _spline.Spline.Count == 0)
      return Vector3.forward;
    float3 tangentLocal = _spline.Spline.EvaluateTangent(t);
    return _spline.transform.TransformDirection(tangentLocal).normalized;
  }

  private void BuildSegmentColliders()
  {
    if (_spline == null || _spline.Spline.Count == 0)
      return;

    float totalLength = _spline.Spline.GetLength();
    int segmentCount = Mathf.Max(1, Mathf.CeilToInt(totalLength / _segmentLength));

    for (int i = 0; i < segmentCount; i++)
    {
      float tStart = i / (float)segmentCount;
      float tEnd = (i + 1) / (float)segmentCount;
      float tMid = (tStart + tEnd) * 0.5f;

      float3 localPos = _spline.Spline.EvaluatePosition(tMid);
      float3 localTangent = _spline.Spline.EvaluateTangent(tMid);
      Vector3 tangentDir = ((Vector3)localTangent).normalized;

      var segmentGO = new GameObject($"RailSegment_{i}");
      segmentGO.transform.SetParent(transform, false);
      segmentGO.layer = gameObject.layer;
      segmentGO.transform.localPosition = localPos;

      if (tangentDir.sqrMagnitude > 0.0001f)
        segmentGO.transform.localRotation = Quaternion.LookRotation(tangentDir, Vector3.up);

      var capsule = segmentGO.AddComponent<CapsuleCollider>();
      capsule.isTrigger = true;
      capsule.direction = 2;
      capsule.radius = _segmentRadius;
      capsule.height = (totalLength / segmentCount) + _segmentRadius;

      var marker = segmentGO.AddComponent<RailSegmentMarker>();
      marker.Owner = this;
      marker.LockRange = _lockRange;
      marker.BoostGrace = _boostGrace;
    }
  }
}
