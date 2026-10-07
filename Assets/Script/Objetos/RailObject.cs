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

    SplineSegmentBuilder.Build(
      _spline,
      _segmentLength,
      _segmentRadius,
      ConfigureSegment,
      namePrefix: "RailSegment"
    );
  }

  private void Start()
  {
    RailManager.Register(this);
  }

  private void ConfigureSegment(GameObject segment)
  {
    var marker = segment.AddComponent<RailSegmentMarker>();
    marker.Owner = this;
    marker.LockRange = _lockRange;
    marker.BoostGrace = _boostGrace;
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
}
