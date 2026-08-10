using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody))]
public abstract class Enemies : CombatEntities, ILockable
{
  protected Transform target;
  private Collider[] result = new Collider[10];
  private Collider[] attackResult = new Collider[5];

  [Header("Configurações de LockIn")]
  [SerializeField]
  private float _lockInRange = 20;

  [Header("Boost")]
  [SerializeField]
  private float _boostGrace = 20f;

  [Header("Configurações de Morte")]
  [SerializeField]
  private float _deathEffectDuration = 1.6f;

  [Header("Knockback")]
  [SerializeField]
  protected float _knockbackForce = 60;

  [SerializeField]
  protected float _knockbackRadius = 10f;

  [SerializeField, Range(1f, 20f)]
  protected float _verticalMultiplier = 1f;

  protected Collider[] scanResult = new Collider[10];

  public float LockRange => _lockInRange;
  public float BoostGrace => _boostGrace;
  public bool IsActive => enabled && gameObject.activeInHierarchy;

  // ==== CONFIGURAÇÕES DE DETECÇÃO ====
  [Header("Configurações de Detecção")]
  [SerializeField]
  private LayerMask layer;

  [SerializeField, Min(10)]
  private float radius;

  [SerializeField]
  private float attackRange = 2f;

  [SerializeField]
  private float memoryCooldown = 3f;

  [SerializeField]
  private float memoryCooldownWalker = 0.0f;
  private bool memoryTriggered = false;
  private bool playerInArea = false;

  // ==== COMPORTAMENTO DE IA ====

  [Header("Componentes")]
  [SerializeField]
  protected Rigidbody _rb;

  [Header("IA")]
  [SerializeField]
  private bool can_AI = true;

  [SerializeField]
  private float visionInterval = 0.5f; // Intervalo para verificar visão

  [SerializeField]
  private float attackInterval = 1f;
  private float visionIntervalwalker = 0.0f;
  private float attackIntervalwalker = 0.0f;

  // ==== CONFIGURAÇÂO DE LOOTTABLE ==== //
  protected WeightedTable<int> _lootTable = new();
  protected SerializedDictionary<int, float> items = new()
  {
    { 10, 10 },
    { 5, 25 },
    { 1, 90 },
  };

  // ==== Score ==== //
  [Header("Pontuação")]
  [SerializeField]
  private int _scoreWhenDamaged = 50;

  [SerializeField]
  private int _scoreWhenKilled = 200;

  // ==== Referência para o Scanner ==== //
  [HideInInspector]
  public Vector3 spawnpos;

  // === Flash Requisitos ===
  private Renderer[] renderers;
  private Material[] originalMaterials;
  private Sequence flashSequence;

  [Header("Flash de Dano")]
  [SerializeField]
  private bool canFlash = true;

  [SerializeField]
  private float flashDuration = 0.1f;

  [SerializeField]
  private Material flashMaterial;

  [Header("Efeito de Dano")]
  [SerializeField]
  private RectTransform _damagePopupEffect;

  [Header("Respawn")]
  [SerializeField]
  private bool _canRespawn = true;

  [SerializeField]
  private float _respawnDelay = 5f;

  public override void Start()
  {
    base.Start();
    if (spawnpos == default)
    {
      spawnpos = transform.position;
    }
    SetupOriginals();
    AddItems();
  }

  public override void DeathHandler()
  {
    int quantity = Physics.OverlapSphereNonAlloc(
      transform.position,
      _knockbackRadius,
      scanResult,
      LayerMask.GetMask("Entity", "Player"),
      QueryTriggerInteraction.Collide
    );

    for (int i = 0; i < quantity; i++)
    {
      Collider hit = scanResult[i];
      if (!hit.CompareTag(Constants.Tags.Player.ToString()))
        continue;

      if (hit.TryGetComponent(out Player player))
      {
        player.AddScore(_scoreWhenKilled);
        player.AddAmethysts(_lootTable.PickEntry());
      }
    }

    EffectsSystem.PlayEffect(
      EntityEffectType.EntityDeathEffect,
      _deathEffectDuration,
      onComplete: HandleDeathPostEffect
    );
  }

  private void HandleDeathPostEffect()
  {
    gameObject.SetActive(false);

    if (_canRespawn)
    {
      DOTween.Sequence().AppendInterval(_respawnDelay).AppendCallback(Respawn);
    }
  }

  protected virtual void Respawn()
  {
    transform.position = spawnpos;
    ResetForRespawn();
    gameObject.SetActive(true);
  }

  protected virtual void ResetForRespawn()
  {
    EffectsSystem.ResetEffect(EntityEffectType.EntityDeathEffect);
    target = null;
    playerInArea = false;
    memoryTriggered = false;
    memoryCooldownWalker = 0f;
    visionIntervalwalker = 0f;
    attackIntervalwalker = 0f;
    Health = MaxHealth;
  }

  private void AddItems()
  {
    if (items.Count > 0)
    {
      foreach (var item in items)
      {
        _lootTable.AddEntry(item.Key, item.Value);
      }
    }
  }

  public override void Update()
  {
    base.Update();
    if (can_AI)
    {
      VisionTimer();
      AttackTimer();
      MemoryTimer();
    }
  }

  private void VisionTimer()
  {
    visionIntervalwalker += Time.deltaTime;
    if (visionIntervalwalker >= visionInterval)
    {
      UpdateTarget();
      visionIntervalwalker = 0f;
    }
  }

  private void AttackTimer()
  {
    attackIntervalwalker += Time.deltaTime;
    if (attackIntervalwalker >= attackInterval)
    {
      UpdateAttackLogic();
      attackIntervalwalker = 0f;
    }
  }

  private void MemoryTimer()
  {
    if (!playerInArea && !memoryTriggered)
    {
      memoryCooldownWalker += Time.deltaTime;

      if (memoryCooldownWalker >= memoryCooldown)
      {
        target = transform;
        memoryCooldownWalker = 0.0f;
        memoryTriggered = true;
      }
    }
    else if (playerInArea)
    {
      memoryCooldownWalker = 0.0f;
      memoryTriggered = false;
    }
  }

  private void UpdateTarget()
  {
    int quantity = Physics.OverlapSphereNonAlloc(transform.position, radius, result, layer);

    for (int i = 0; i < quantity; i++)
    {
      var subtarget = result[i].transform;

      if (subtarget == transform || subtarget.IsChildOf(transform))
        continue;
      if (subtarget.TryGetComponent(out Player _))
      {
        playerInArea = true;
        memoryCooldownWalker = .0f;
        target = subtarget;
        return;
      }
    }

    playerInArea = false;
  }

  private void UpdateAttackLogic()
  {
    int quantity = Physics.OverlapSphereNonAlloc(
      transform.position,
      attackRange,
      attackResult,
      layer
    );
    for (int i = 0; i < quantity; i++)
    {
      var nearby = attackResult[i].transform;
      if (nearby == transform || nearby.IsChildOf(transform))
        continue;
    }
  }

  public override void DamageHandler()
  {
    int quantity = Physics.OverlapSphereNonAlloc(
      transform.position,
      _knockbackRadius,
      scanResult,
      LayerMask.GetMask("Entity", "Player"),
      QueryTriggerInteraction.Collide
    );
    for (int i = 0; i < quantity; i++)
    {
      Collider hit = scanResult[i];
      if (!hit.CompareTag(Constants.Tags.Player.ToString()))
        continue;

      if (hit.TryGetComponent(out Player player))
      {
        TriggerKnockback(player);
        TriggerRewards(player);
      }
    }
    TriggerFlash();
    TriggerSquish();
    TriggerDamagePopup();
  }

  protected virtual void TriggerKnockback(Player player) { }

  protected virtual void TriggerRewards(Player player)
  {
    player.AddScore(_scoreWhenDamaged);
  }

  protected virtual void TriggerDamagePopup()
  {
    if (_damagePopupEffect == null)
      return;

    _damagePopupEffect.DOKill();
    _damagePopupEffect.localScale = Vector3.zero;
    _damagePopupEffect.gameObject.SetActive(true);
    Sequence popupSequence = DOTween.Sequence();
    popupSequence.Append(_damagePopupEffect.DOScale(Vector3.one, 0.1f).SetEase(Ease.OutBack));
    popupSequence.AppendInterval(0.25f);
    popupSequence.Append(_damagePopupEffect.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));
    popupSequence.OnComplete(() =>
    {
      _damagePopupEffect.gameObject.SetActive(false);
    });
  }

  protected virtual void TriggerSquish()
  {
    transform.DOKill();
    Vector3 originalScale = transform.localScale;
    float squashFactor = 1.8f;
    float compressFactor = 0.4f;

    Vector3 squishScale = new(
      originalScale.x * squashFactor,
      originalScale.y * compressFactor,
      originalScale.z * squashFactor
    );

    Sequence squishSequence = DOTween.Sequence().SetUpdate(true);

    squishSequence.Append(
      transform.DOScale(squishScale, 0.08f).SetEase(Ease.Linear).SetUpdate(UpdateType.Late)
    );
    squishSequence.Append(
      transform
        .DOScale(originalScale * 1.08f, 0.12f)
        .SetEase(Ease.OutBack)
        .SetUpdate(UpdateType.Late)
    );
    squishSequence.Append(
      transform
        .DOScale(originalScale * 0.97f, 0.08f)
        .SetEase(Ease.InOutSine)
        .SetUpdate(UpdateType.Late)
    );
    squishSequence.Append(
      transform.DOScale(originalScale, 0.1f).SetEase(Ease.OutQuad).SetUpdate(UpdateType.Late)
    );
    squishSequence.Play();
  }

  private void SetupOriginals()
  {
    renderers = GetComponentsInChildren<Renderer>();
    originalMaterials = new Material[renderers.Length];

    for (int i = 0; i < renderers.Length; i++)
    {
      originalMaterials[i] = renderers[i].material;
    }
  }

  protected virtual void TriggerFlash()
  {
    if (!canFlash || flashMaterial == null)
      return;

    if (flashSequence != null && flashSequence.IsActive())
    {
      flashSequence.Kill();
      RestoreMaterials();
    }

    ApplyFlashMaterial();
    flashSequence = DOTween.Sequence().SetUpdate(true);
    flashSequence.AppendInterval(flashDuration);
    flashSequence.OnComplete(() => RestoreMaterials());
    flashSequence.Play();
  }

  private void ApplyFlashMaterial()
  {
    for (int i = 0; i < renderers.Length; i++)
    {
      if (renderers[i] != null && flashMaterial != null)
      {
        renderers[i].material = flashMaterial;
      }
    }
  }

  private void RestoreMaterials()
  {
    for (int i = 0; i < renderers.Length; i++)
    {
      if (renderers[i] != null && originalMaterials[i] != null)
      {
        renderers[i].material = originalMaterials[i];
      }
    }
  }
}
