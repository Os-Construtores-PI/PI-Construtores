using DG.Tweening;
using UnityEngine;

public class AmethystItemDropZone : ItemDropZone
{
  #region Fields
  [Header("Visual")]
  [SerializeField]
  private float _scaleMultiplier = 1.5f;

  [SerializeField]
  private float _shrinkDuration = 0.25f;

  [Header("Feedback")]
  [SerializeField]
  private float _shakeDuration = 0.15f;

  [SerializeField]
  private float _shakeStrength = 0.3f;

  [SerializeField]
  private int _shakeVibrato = 20;

  [SerializeField]
  private float _shakeRandomness = 45f;

  [Header("Gameplay")]
  [SerializeField]
  private float _boostGrace = 10f;

  [Header("Áudio")]
  [SerializeField]
  private somMenu _somMenu;

  private Tweener _currentTweener;
  private Sequence _currentSequence;
  #endregion

  protected override void AddItem(Player player)
  {
    if (player == null)
      return;

    PlayAudioFeedback();

    DisableInteraction();

    ApplyGameplayEffects(player);

    PlayVisualFeedback();
  }

  private void PlayAudioFeedback()
  {
    if (AudioManager.Instance != null && _somMenu != null && _somMenu.amestinstSong != null)
    {
      AudioManager.Instance.PlaySFX(_somMenu.amestinstSong);
    }
  }

  private void DisableInteraction()
  {
    if (_boxCollider != null)
    {
      _boxCollider.enabled = false;
    }
    enabled = false;
  }

  private void ApplyGameplayEffects(Player player)
  {
    player.AddAmethysts(quantity, transform.position);

    if (player.DashSlashBoostButton != null)
    {
      player.DashSlashBoostButton.Value += _boostGrace;
    }
    else
    {
      Debug.LogWarning(
        "[AmethystItemDropZone] DashSlashBoostButton não configurado no Player.",
        player
      );
    }
  }

  private void PlayVisualFeedback()
  {
    Vector3 initialScale = transform.localScale;

    KillExistingAnimations();

    _currentSequence = DOTween.Sequence();

    _currentSequence.Append(
      transform.DOShakePosition(
        _shakeDuration,
        _shakeStrength,
        _shakeVibrato,
        _shakeRandomness,
        false,
        true
      )
    );

    _currentSequence.Append(
      transform.DOScale(initialScale * _scaleMultiplier, _shrinkDuration / 2f).SetEase(Ease.OutBack)
    );

    _currentSequence.Append(
      transform.DOScale(Vector3.zero, _shrinkDuration / 2f).SetEase(Ease.InBack)
    );

    _currentSequence.AppendCallback(() =>
    {
      HandlePostAnimation();
    });

    _currentSequence.Play();
  }

  private void HandlePostAnimation()
  {
    if (_destroyOnCollect)
    {
      Destroy(gameObject);
    }
    else
    {
      ResetZone();
    }
  }

  private void KillExistingAnimations()
  {
    _currentTweener?.Kill();
    _currentSequence?.Kill();
    DOTween.Kill(transform);
  }

  public void OnDisable()
  {
    KillExistingAnimations();
  }

  public void OnDestroy()
  {
    KillExistingAnimations();
  }
}
