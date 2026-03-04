using DG.Tweening;
using UnityEngine;

public class RotatingCube : BaseRenderedGameObject
{
  [SerializeField]
  private float _xRotateSpeed;

  [SerializeField]
  private float _yRotateSpeed;

  [SerializeField]
  private float _zRotateSpeed;

  [SerializeField]
  private float _rotationDuration = 1;

  [SerializeField]
  private float _rotationInterval = 5;

  public override void Start()
  {
    base.Start();
    RotationAnimation();
  }

  private void RotationAnimation()
  {
    Vector3 rotationStep = new(_xRotateSpeed, _yRotateSpeed, _zRotateSpeed);

    Sequence rotatingSequence = DOTween.Sequence();

    rotatingSequence.Append(
      transform
        .DORotate(rotationStep, _rotationDuration, RotateMode.WorldAxisAdd)
        .SetEase(Ease.InExpo)
    );

    rotatingSequence.AppendInterval(_rotationInterval);
    rotatingSequence.SetLoops(-1);
  }
}
