using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RandomSpinObject : BaseRenderedGameObject
{
  [SerializeField]
  private ActTransition _transitionListener;

  [SerializeField]
  private List<float> _rotationTargets = new();

  [SerializeField]
  private int _spins = 4;

  [SerializeField]
  private float _rotationDuration = 5f;

  public override void Start()
  {
    if (_transitionListener != null)
    {
      _transitionListener.Transition.AddListener(RotationAnimation);
    }
  }

  public void RotationAnimation()
  {
    int randomIndex = Random.Range(0, _rotationTargets.Count - 1);
    float randomRotationAngle = _rotationTargets[randomIndex];
    var angleVector = new Vector3(0, 360 * _spins + randomRotationAngle, 0);
    Sequence animationSequence = DOTween.Sequence();
    animationSequence.AppendInterval(3f);
    animationSequence.Append(
      transform.DORotate(angleVector, _rotationDuration, RotateMode.FastBeyond360)
    );
    animationSequence.Play();
  }
}
