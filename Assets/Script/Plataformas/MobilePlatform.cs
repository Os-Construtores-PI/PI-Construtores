using System.Collections.Generic;
using DG.Tweening;
using KinematicCharacterController;
using UnityEngine;

[RequireComponent(typeof(PhysicsMover))]
public class MobilePlatform : BasePlatform, IMoverController
{
  private readonly List<Vector3> _targets = new();

  [Header("Path Configuration")]
  [SerializeField]
  private PathType _pathType = PathType.Linear;

  [SerializeField]
  private PathMode _pathMode = PathMode.Full3D;

  [SerializeField]
  private Ease _animationType = Ease.Linear;

  [SerializeField]
  private LoopType _loopType = LoopType.Yoyo;

  [SerializeField]
  private int _pathResolution = 10;

  [SerializeField]
  private Color _gizmoColor = Color.white;

  [Header("Timing")]
  [SerializeField]
  private float _duration = 4f;

  [SerializeField]
  private int _loopNum = -1;

  [Header("Physics Mover")]
  [SerializeField]
  private PhysicsMover _mover;

  private Vector3 _originalPosition;
  private Quaternion _originalRotation;
  private Tweener _tweener;
  private float _currentProgress;

  public override void Start()
  {
    base.Start();
    InitializeTargets();

    if (_mover == null)
    {
      Debug.LogError("PhysicsMover is not assigned.", this);
      enabled = false;
      return;
    }

    _originalPosition = _mover.Rigidbody.position;
    _originalRotation = _mover.Rigidbody.rotation;
    _mover.MoverController = this;

    if (_targets.Count > 0)
    {
      SetupTween();
    }
  }

  private void InitializeTargets()
  {
    foreach (Transform child in transform)
    {
      if (child.name.ToLower().Contains("target"))
      {
        _targets.Add(child.position);
      }
    }
  }

  private void SetupTween()
  {
    _tweener = DOTween
      .To(() => 0f, value => _currentProgress = value, 1f, _duration)
      .SetLoops(_loopNum, _loopType)
      .SetEase(_animationType)
      .SetUpdate(UpdateType.Fixed);
  }

  public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
  {
    goalPosition = EvaluatePathPosition(_currentProgress);
    goalRotation = _originalRotation;
  }

  private Vector3 EvaluatePathPosition(float progress)
  {
    if (_targets.Count == 0)
    {
      return _originalPosition;
    }

    if (_targets.Count == 1)
    {
      return Vector3.Lerp(_originalPosition, _targets[0], progress);
    }

    if (_pathType == PathType.Linear)
    {
      return EvaluateLinearPath(progress);
    }

    return EvaluateCatmullRomPath(progress);
  }

  private Vector3 EvaluateLinearPath(float progress)
  {
    float totalLength = _targets.Count;
    float scaledProgress = progress * totalLength;
    int segmentIndex = Mathf.FloorToInt(scaledProgress);
    float segmentProgress = scaledProgress - segmentIndex;

    segmentIndex = Mathf.Clamp(segmentIndex, 0, _targets.Count - 1);
    int nextIndex = Mathf.Clamp(segmentIndex + 1, 0, _targets.Count - 1);

    Vector3 from = segmentIndex == 0 ? _originalPosition : _targets[segmentIndex - 1];
    Vector3 to = _targets[nextIndex == 0 ? 0 : nextIndex - 1];

    return Vector3.Lerp(from, to, segmentProgress);
  }

  private Vector3 EvaluateCatmullRomPath(float progress)
  {
    float totalLength = _targets.Count;
    float scaledProgress = progress * totalLength;
    int segmentIndex = Mathf.FloorToInt(scaledProgress);
    float segmentProgress = scaledProgress - segmentIndex;

    segmentIndex = Mathf.Clamp(segmentIndex, 0, _targets.Count - 1);

    Vector3 p0 = segmentIndex == 0 ? _originalPosition : _targets[segmentIndex - 1];
    Vector3 p1 = _targets[Mathf.Clamp(segmentIndex, 0, _targets.Count - 1)];
    Vector3 p2 = _targets[Mathf.Clamp(segmentIndex + 1, 0, _targets.Count - 1)];
    Vector3 p3 = _targets[Mathf.Clamp(segmentIndex + 2, 0, _targets.Count - 1)];

    return CatmullRom(p0, p1, p2, p3, segmentProgress);
  }

  private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
  {
    float t2 = t * t;
    float t3 = t2 * t;

    return 0.5f
      * (
        (2f * p1)
        + (-p0 + p2) * t
        + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2
        + (-p0 + 3f * p1 - 3f * p2 + p3) * t3
      );
  }

  private void OnDestroy()
  {
    if (_tweener != null && _tweener.IsActive())
    {
      _tweener.Kill();
    }
  }
}
