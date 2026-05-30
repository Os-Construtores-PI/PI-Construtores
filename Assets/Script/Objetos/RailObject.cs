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

  private SplineContainer _spline;

  public enum RailDirection
  {
    Forward,
    Backward,
  }

  private void Awake()
  {
    _spline = GetComponent<SplineContainer>();
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

#if UNITY_EDITOR
  private void OnDrawGizmosSelected()
  {
    var splineContainer = GetComponent<SplineContainer>();
    if (splineContainer == null || splineContainer.Spline.Count == 0)
      return;

    Gizmos.color = Color.cyan;
    Vector3 prev = transform.TransformPoint(splineContainer.Spline.EvaluatePosition(0f));
    const int segments = 30;
    for (int i = 1; i <= segments; i++)
    {
      Vector3 curr = transform.TransformPoint(
        splineContainer.Spline.EvaluatePosition(i / (float)segments)
      );
      Gizmos.DrawLine(prev, curr);
      prev = curr;
    }
  }
#endif
}
