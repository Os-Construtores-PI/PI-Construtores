using DG.Tweening;
using UnityEngine;

public class AmethystItemDropZone : ItemDropZone, IRespawnable
{
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
  private AudioClip _pickupAudio;

  [Header("Perseguição")]
  [SerializeField]
  private float _pursuitSpeed = 10;

  [SerializeField]
  private float _pursuitThresold = 1;

  [SerializeField]
  private float _pursuitMaxDistance = 8f;

  [SerializeField]
  private float _minScaleFactor = 0.2f;

  [SerializeField]
  private float _pursuitAcceleration = 5f;

  [SerializeField]
  private float _pursuitMaxSpeedMultiplier = 3f;

  private Vector3 _initialScale;
  private Player _target;
  private float _pursuitElapsedTime;
  private Tween _currentTweener;
  private Sequence _currentSequence;
  private Tween _respawnTween;

  public bool IsAlive { get; private set; } = true;

  public override void Awake()
  {
    base.Awake();
    GameDirector.RespawnManager.Register(this);
  }

  public override void OnDestroy()
  {
    base.OnDestroy();
    GameDirector.RespawnManager.Unregister(this);
    KillExistingAnimations();
  }

  public override void Initialize()
  {
    base.Initialize();
    _initialScale = transform.localScale;
  }

  public void Update()
  {
    if (_target == null)
      return;

    float distance = Vector3.Distance(transform.position, _target.transform.position);

    if (distance < _pursuitThresold)
    {
      CommitCollect(_target);
      _target = null;
      return;
    }

    _pursuitElapsedTime += Time.deltaTime;

    float speedMultiplier = Mathf.Min(
      1f + _pursuitElapsedTime * _pursuitAcceleration,
      _pursuitMaxSpeedMultiplier
    );

    Vector3 playerDirection = (_target.transform.position - transform.position).normalized;
    transform.position += _pursuitSpeed * speedMultiplier * Time.deltaTime * playerDirection;

    float t = Mathf.InverseLerp(_pursuitThresold, _pursuitMaxDistance, distance);
    float scaleFactor = Mathf.Lerp(_minScaleFactor, 1f, t);
    transform.localScale = _initialScale * scaleFactor;
  }

  protected override void OnCollectTriggered(Player player)
  {
    if (!CanCollect(player))
      return;

    if (_boxCollider != null)
      _boxCollider.enabled = false;

    bool isInBoost = player.ActionLayer.GetActive<PlayerActionStateBoost>() != null;
    if (isInBoost)
    {
      _target = player;
      _pursuitElapsedTime = 0f;
    }
    else
    {
      CommitCollect(player);
    }
  }

  protected override void CommitCollect(Player player)
  {
    IsAlive = false;
    base.CommitCollect(player);
  }

  protected override void AddItem(Player player)
  {
    if (player == null)
      return;

    player.AddAmethysts(_quantity);
    player.BoostValue += _boostGrace;
  }

  protected override void AfterCollect()
  {
    PlayAudioFeedback();
    PlayVisualFeedback();
  }

  private void PlayAudioFeedback()
  {
    if (AudioManager.Instance != null && _pickupAudio != null)
      AudioManager.Instance.PlaySFX(_pickupAudio);
  }

  private void PlayVisualFeedback()
  {
    KillExistingAnimations();

    _currentSequence = DOTween.Sequence();

    _currentSequence
      .Append(
        transform.DOShakePosition(
          _shakeDuration,
          _shakeStrength,
          _shakeVibrato,
          _shakeRandomness,
          false,
          true
        )
      )
      .Append(
        transform
          .DOScale(_initialScale * _scaleMultiplier, _shrinkDuration / 2f)
          .SetEase(Ease.OutBack)
      )
      .Append(transform.DOScale(Vector3.zero, _shrinkDuration / 2f).SetEase(Ease.InBack))
      .AppendCallback(HandlePostAnimation)
      .Play();
  }

  private void HandlePostAnimation()
  {
    base.DisableZone();
  }

  public void Respawn()
  {
    gameObject.SetActive(true);
    IsAlive = true;
    KillExistingAnimations();
    ResetZone();
  }

  public override void ResetZone()
  {
    KillExistingAnimations();
    transform.localScale = _initialScale;
    base.ResetZone();
  }

  private void KillExistingAnimations()
  {
    _currentTweener?.Kill();
    _currentSequence?.Kill();
    _respawnTween?.Kill();
    DOTween.Kill(transform);
  }

  private void OnDisable()
  {
    if (IsAlive)
      KillExistingAnimations();
  }
}
