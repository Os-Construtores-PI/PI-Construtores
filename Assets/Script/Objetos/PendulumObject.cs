using DG.Tweening;
using UnityEngine;

public class PendulumObject : MonoBehaviour
{
  [Header("Pendulum Settings")]
  [Tooltip("Maximum swing angle in degrees (e.g., 30 = swings ±30°)")]
  [Range(0f, 90f)]
  public float maxAngle = 30f;

  [Tooltip("Time for one complete swing cycle (left → right → left) in seconds")]
  [Range(0.5f, 10f)]
  public float period = 2f;

  [Tooltip("Axis around which the pendulum rotates (local space)")]
  public Vector3 rotationAxis = Vector3.forward;

  [Tooltip("Ease style for swing motion")]
  public Ease swingEase = Ease.InOutSine;

  [Tooltip("Start swinging automatically on Start()")]
  public bool autoStart = true;

  [Header("Optional: External Pivot")]
  public Transform pivotPoint;

  private Sequence _swingSequence;
  private bool _isSwinging = false;
  private Quaternion _restRotation;

  void Start()
  {
    _restRotation = pivotPoint != null ? pivotPoint.localRotation : transform.localRotation;

    if (autoStart)
      StartSwinging();
  }

  void OnDestroy()
  {
    _swingSequence?.Kill();
  }

  public void StartSwinging()
  {
    if (_isSwinging)
      return;
    _isSwinging = true;

    _swingSequence?.Kill();
    _swingSequence = DOTween.Sequence();

    float halfPeriod = period / 2f;
    Vector3 axisVector = rotationAxis.normalized;
    Transform target = pivotPoint ?? transform;

    // Build swing sequence: Center → +Angle → -Angle → (loop)
    _swingSequence
      .Append(
        target
          .DOLocalRotate(
            _restRotation.eulerAngles + maxAngle * axisVector,
            halfPeriod,
            RotateMode.FastBeyond360
          )
          .SetEase(swingEase)
      )
      .Append(
        target
          .DOLocalRotate(
            _restRotation.eulerAngles - maxAngle * axisVector,
            halfPeriod,
            RotateMode.FastBeyond360
          )
          .SetEase(swingEase)
      )
      .SetLoops(-1, LoopType.Yoyo)
      .SetUpdate(true);

    // Ensure we start from rest
    target.localRotation = _restRotation;
  }

  public void StopSwinging()
  {
    _swingSequence?.Kill();
    _isSwinging = false;

    Transform target = pivotPoint != null ? pivotPoint : transform;
    target
      .DOLocalRotate(_restRotation.eulerAngles, 0.5f, RotateMode.LocalAxisAdd)
      .SetEase(Ease.OutQuad);
  }

  public void ToggleSwing()
  {
    if (_isSwinging)
      StopSwinging();
    else
      StartSwinging();
  }

  void OnDrawGizmosSelected()
  {
    if (!Application.isPlaying)
    {
      Gizmos.color = Color.yellow;
      Vector3 pivot = pivotPoint != null ? pivotPoint.position : transform.position;
      Vector3 upDir = pivotPoint != null ? pivotPoint.up : transform.up;
      Gizmos.DrawLine(pivot, pivot + upDir * 2f);

      Gizmos.color = new Color(0, 1, 1, 0.5f);
      float arcRadius = 2f;
      Vector3 prevPoint = pivot + Quaternion.AngleAxis(-maxAngle, rotationAxis) * upDir * arcRadius;

      for (int i = -10; i <= 10; i++)
      {
        float t = i / 10f;
        float angle = maxAngle * t;
        Vector3 point = pivot + Quaternion.AngleAxis(angle, rotationAxis) * upDir * arcRadius;
        Gizmos.DrawLine(prevPoint, point);
        prevPoint = point;
      }
    }
  }
}
