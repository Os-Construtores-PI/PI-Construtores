using DG.Tweening;
using UnityEngine;

public class UIPulse : MonoBehaviour
{
  [SerializeField]
  private Vector3 pulseScale = new(1.05f, 1.05f, 1.05f);

  [SerializeField]
  private float duration = .8f;

  private Tween tween;

  public void Play()
  {
    Stop();

    transform.localScale = Vector3.one;
    tween = transform
      .DOScale(pulseScale, duration)
      .SetLoops(-1, LoopType.Yoyo)
      .SetEase(Ease.InOutSine);
  }

  public void Stop()
  {
    tween?.Kill();
    transform.localScale = Vector3.one;
  }
}
