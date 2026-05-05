using DG.Tweening;
using UnityEngine;

public class AmethystItemDropZone : ItemDropZone
{
  private readonly float _scaleMultiplier = 1.5f;
  private readonly float _durationScale = .25f;

  [SerializeField]
  private float _boostGrace = 20f;

  protected override void AddItem(Player player)
  {
    Vector3 initialScale = transform.localScale;

    player.AddAmethysts(quantity, transform.position);
    player.DashSlashBoostButton.Value += _boostGrace;
    _boxCollider.enabled = false;

    Sequence sequence = DOTween.Sequence();

    sequence.Append(
      transform.DOShakePosition(
        duration: 0.15f,
        strength: 0.3f,
        vibrato: 20,
        randomness: 45,
        snapping: false,
        fadeOut: true
      )
    );

    sequence.Append(
      transform.DOScale(initialScale * _scaleMultiplier, _durationScale / 2).SetEase(Ease.OutBack)
    );

    sequence.Append(transform.DOScale(0, _durationScale / 2).SetEase(Ease.InBack));

    sequence.AppendCallback(() => gameObject.SetActive(false));

    sequence.Play();
  }
}
