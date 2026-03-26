using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SwingObject : InteractableObject
{
  [SerializeField]
  private Transform _parentOfPlayer;

  [SerializeField]
  private float _approachDuration = .5f;

  [SerializeField]
  private Ease _approachEase = Ease.OutSine;

  [SerializeField]
  private Transform _spinObjectTransform;

  [SerializeField]
  private Ease _spinEase = Ease.InBack;

  [SerializeField]
  private Vector3 _spinAngle = new(0, 0, 0);

  [SerializeField]
  private float _spinDuration = 2f;

  private bool _Isgoing = true;

  public override void Interaction(InfoPlayerInteraction info)
  {
    SpinSequence(info.PlayerContext);
  }

  private void SpinSequence(PlayerContext playerContext)
  {
    Sequence spinSequence = DOTween.Sequence().SetUpdate(UpdateType.Fixed);
    spinSequence.AppendCallback(() =>
      {
        playerContext.EntityTransform.SetParent(_parentOfPlayer);
        playerContext.OverrideGlobal = true;
        playerContext.PlayerController.enabled = false;
      }
      );
    spinSequence.Append(playerContext.EntityTransform.DOLocalMove(Vector3.zero, _approachDuration).SetEase(_approachEase));
    spinSequence.AppendInterval(.5f);
    Vector3 targetAngle = _Isgoing ? _spinAngle : -_spinAngle;
    spinSequence.Append(_spinObjectTransform.DORotate(targetAngle, _spinDuration).SetEase(_spinEase));
    spinSequence.AppendCallback(() =>
    {
      playerContext.EntityTransform.SetParent(null);
      playerContext.EntityTransform.eulerAngles = Vector3.zero;
      playerContext.OverrideGlobal = false;
      playerContext.PlayerController.enabled = true;
      _spinObjectTransform.localEulerAngles = Vector3.zero;
      _Isgoing = !_Isgoing;
    });
    spinSequence.Play();
  }
}
