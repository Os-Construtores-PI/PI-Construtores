using DG.Tweening;
using UnityEngine;

public class PendulumObject : MonoBehaviour
{
  [Header("Pendulum Settings")]
  [Range(0f, 90f)]
  public float maxAngle = 30f;

  [Range(0.5f, 10f)]
  public float period = 2f;

  public Vector3 rotationAxis = Vector3.forward;

  public Ease swingEase = Ease.InOutSine;

  public bool autoStart = true;

  [Header("Optional: External Pivot")]
  public Transform pivotPoint;

  private Tween _swingTween;
  private bool _isSwinging = false;
  private Quaternion _restRotation;
  private Vector3 _axisNormalized;
  private Transform _target;

  void Awake()
  {
    _axisNormalized = rotationAxis.normalized;
    _target = pivotPoint ?? transform;
    _restRotation = _target.localRotation;
  }

  void Start()
  {
    if (autoStart)
      StartSwinging();
  }

  void OnDestroy()
  {
    _swingTween?.Kill();
  }

  public void StartSwinging()
  {
    if (_isSwinging)
      return;
    _isSwinging = true;

    _swingTween?.Kill();

    float halfPeriod = period / 2f;

    // Inicia já no extremo esquerdo para oscilação contínua imediata
    _target.localRotation = _restRotation * Quaternion.AngleAxis(-maxAngle, _axisNormalized);

    // Oscila suavemente entre -maxAngle e +maxAngle para sempre
    _swingTween = DOTween
      .To(
        () => -maxAngle,
        angle =>
          _target.localRotation = _restRotation * Quaternion.AngleAxis(angle, _axisNormalized),
        maxAngle,
        halfPeriod
      )
      .SetEase(swingEase)
      .SetLoops(-1, LoopType.Yoyo)
      .SetUpdate(true);
  }

  public void StopSwinging()
  {
    _isSwinging = false;
    _swingTween?.Kill();

    _target
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
    if (Application.isPlaying)
      return;

    Vector3 pivot = pivotPoint != null ? pivotPoint.position : transform.position;
    Vector3 refDir = pivotPoint != null ? pivotPoint.up : transform.up;

    Gizmos.color = Color.yellow;
    Gizmos.DrawLine(pivot, pivot + refDir * 2f);

    Gizmos.color = new Color(0, 1, 1, 0.5f);
    float radius = 2f;
    Vector3 prev = pivot + Quaternion.AngleAxis(-maxAngle, rotationAxis) * refDir * radius;

    for (int i = -10; i <= 10; i++)
    {
      float t = i / 10f;
      Vector3 pt = pivot + Quaternion.AngleAxis(maxAngle * t, rotationAxis) * refDir * radius;
      Gizmos.DrawLine(prev, pt);
      prev = pt;
    }
  }
}
