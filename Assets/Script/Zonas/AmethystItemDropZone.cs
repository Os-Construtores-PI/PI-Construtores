using DG.Tweening;
using UnityEngine;

public class AmethystItemDropZone : ItemDropZone
{
  private readonly float _scaleMultiplier = 1.5f;
  private readonly float _durationScale = .25f;

  protected override void AddItem(Player player)
  {
    Vector3 _initialScale = transform.localScale;
    player.AddAmethysts(quantity, transform.position);
    Sequence _sequence = DOTween.Sequence();
    _sequence.Append(transform.DOScale(_initialScale * _scaleMultiplier, _durationScale / 2));
    _sequence.Append(transform.DOScale(0, _durationScale / 2));
    _sequence.AppendCallback(() => gameObject.SetActive(false));
    _sequence.Play();
  }
}
