using DG.Tweening;
using UnityEngine;

public class ArrowEffect : MonoBehaviour
{
  private const float ArrowOffset = 1f;
  private SpriteRenderer _arrowSprite;
  private float _arrowBaseY;
  private DG.Tweening.Sequence _arrowSequence;

  private void Awake()
  {
    _arrowSprite = GetComponent<SpriteRenderer>();
    _arrowBaseY = transform.localPosition.y;
  }

  public void ArrowEffectSequence()
  {
    _arrowSequence?.Kill();

    Vector3 startPosition = transform.localPosition;
    startPosition.y = _arrowBaseY - ArrowOffset;
    transform.localPosition = startPosition;

    Color startColor = _arrowSprite.color;
    startColor.a = 0f;
    _arrowSprite.color = startColor;

    _arrowSequence = DOTween.Sequence();

    _arrowSequence.Append(transform.DOLocalMoveY(_arrowBaseY, .5f).SetEase(Ease.OutSine));
    _arrowSequence.Join(_arrowSprite.DOFade(1f, .25f));

    _arrowSequence.Append(
      transform.DOLocalMoveY(_arrowBaseY + ArrowOffset, .5f).SetEase(Ease.InSine)
    );
    _arrowSequence.Join(_arrowSprite.DOFade(0f, .25f));

    _arrowSequence.SetLink(gameObject);
  }
}
