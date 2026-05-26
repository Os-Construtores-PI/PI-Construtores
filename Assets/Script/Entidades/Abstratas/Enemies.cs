using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.Rendering;

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

  public float LockRange => _lockInRange;
  public float BoostGrace => _boostGrace;
  public bool IsActive => enabled && gameObject.activeInHierarchy;

  // ==== CONFIGURAÇÕES DE DETECÇÃO ====
  [Header("Configurações de Detecção")]
  [SerializeField]
  private LayerMask layer; // Camada usada para detectar alvos (ex: jogadores)

  [SerializeField, Min(10)]
  private float radius; // Raio da detecção de visão

  [SerializeField]
  private float attackRange = 2f; // Raio da detecção de ataque

  [SerializeField]
  private float memoryCooldown = 3f;

  [SerializeField]
  private float memoryCooldownWalker = 0.0f;
  private bool memoryTriggered = false;
  private bool playerInArea = false;

  // ==== COMPORTAMENTO DE IA ====
  [Header("IA")]
  [SerializeField]
  private bool can_AI = true; // Permite ativar/desativar IA

  [SerializeField]
  private float visionInterval = 0.5f; // Intervalo para verificar visão

  [SerializeField]
  private float attackInterval = 1f;
  private float visionIntervalwalker = 0.0f;
  private float attackIntervalwalker = 0.0f;

  // ==== CONFIGURAÇÂO DE LOOTTABLE ==== //
  protected WeightedTable<string> lootTable = new();
  protected SerializedDictionary<string, float> items = new()
  {
    { "item bom", 10 },
    { "item ruim", 90 },
  };

  // ==== Referência para o Scanner ==== //
  [HideInInspector]
  public Vector3 spawnpos;

  [Header("Knockback")]
  [SerializeField]
  private Collider _collider;

  [SerializeField]
  private float _knockbackForce = 50f;
  public float KnockBackForce => _knockbackForce;

  private readonly Timer _knockbackTimer = new();
  private readonly float _knockbackCooldown = 2f;
  private bool _canKnockback = true;

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

  public override void Start()
  {
    base.Start();
    SetupOriginals();
    AddItems();
  }

  public override void DeathHandler()
  {
    print(lootTable.PickEntry());
    print("MORREU");
    gameObject.SetActive(false);
  }

  private void AddItems()
  {
    if (items.Count > 0)
    {
      foreach (var item in items)
      {
        lootTable.AddEntry(item.Key, item.Value);
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
    KnockbackTimer();
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

  private void KnockbackTimer()
  {
    if (!_canKnockback)
    {
      if (_knockbackTimer.Tick(Time.deltaTime))
      {
        _canKnockback = true;
        _collider.enabled = true;
      }
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
    TriggerFlash();
    TriggerSquish();
    TriggerDamagePopup();
  }

  private void TriggerDamagePopup()
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
    popupSequence.OnComplete(() => _damagePopupEffect.gameObject.SetActive(false));
  }

  private void TriggerSquish()
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

    Sequence squishSequence = DOTween.Sequence();

    // IMPORTANTE: Executar após o Animator
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

  public void TriggerFlash()
  {
    if (!canFlash || flashMaterial == null)
      return;

    if (flashSequence != null && flashSequence.IsActive())
    {
      flashSequence.Kill();
      RestoreMaterials();
    }

    ApplyFlashMaterial();
    flashSequence = DOTween.Sequence();
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

  public void OnTriggerEnter(Collider col)
  {
    if (col.TryGetComponent(out Player player))
    {
      if (_canKnockback)
      {
        _canKnockback = false;
        player.CurrentDashCount = 0;
        player.IsDashing = false;
        player.CurrentJumpCount = 0;
        player.MovementVector = Vector3.up * KnockBackForce;
        _collider.enabled = false;
        _knockbackTimer.Start(_knockbackCooldown);
      }
    }
  }
}
