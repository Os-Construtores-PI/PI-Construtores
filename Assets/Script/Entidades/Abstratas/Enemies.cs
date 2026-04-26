using System.Collections.Generic;
using DG.Tweening;
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

  public float LockRange => _lockInRange;
  public bool IsActive => this.enabled && gameObject.activeInHierarchy;

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

  [Header("ENEMY KNOCKBACK PROPERTIES")]
  [SerializeField]
  private Collider _collider;

  [SerializeField]
  private float _knockbackForce = 40f;
  public float KnockBackForce => _knockbackForce;

  private readonly Timer _knockbackTimer = new();
  private readonly float _knockbackCooldown = 2f;
  private bool _canKnockback = true;

  // === Flash Requisitos ===
  private Renderer[] renderers;
  private List<Color> originalColors = new List<Color>();
  private List<Color> originalEmissionColors = new List<Color>();
  private MaterialPropertyBlock block;

  [Header("DAMAGE FLASH PROPERTIES")]
  [SerializeField]
  private bool canFlash = true;

  [SerializeField]
  private float flashDuration = 0.1f;

  [SerializeField]
  private Color flashColor = Color.white;

  [SerializeField]
  private Color flashEmission = Color.white; // mais intenso
  private Sequence flashSequence;

  public override void Awake()
  {
    base.Awake();
    SetupOriginals();
  }

  public override void Start()
  {
    base.Start();
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
    if (!playerInArea && !memoryTriggered) // só executa se o player saiu e ainda não rodou
    {
      memoryCooldownWalker += Time.deltaTime;

      if (memoryCooldownWalker >= memoryCooldown)
      {
        target = transform;
        memoryCooldownWalker = 0.0f;
        memoryTriggered = true; // marca que já rodou
      }
    }
    else if (playerInArea)
    {
      // se o player voltar, reseta o estado
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

    // Se não encontrar alvo, redefine o alvo para si mesmo
    playerInArea = false;
  }

  // Verifica se há algum alvo próximo o suficiente para ataque
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
  }

  private void SetupOriginals()
  {
    renderers = GetComponentsInChildren<Renderer>();
    block = new();
    foreach (Renderer r in renderers)
    {
      r.GetPropertyBlock(block);

      // Cor base
      Color baseColor = r.sharedMaterial.HasProperty("_BaseColor")
        ? r.sharedMaterial.GetColor("_BaseColor")
        : Color.white;

      // Emission (se tiver)
      Color emissionColor = r.sharedMaterial.HasProperty("_EmissionColor")
        ? r.sharedMaterial.GetColor("_EmissionColor")
        : Color.black;

      originalColors.Add(baseColor);
      originalEmissionColors.Add(emissionColor);
    }
  }

  public void TriggerFlash()
  {
    if (!canFlash)
    {
      return;
    }
    // se já tem uma animação rodando, mata ela
    if (flashSequence != null && flashSequence.IsActive())
      flashSequence.Kill();

    float intensity = 0f;

    flashSequence = DOTween.Sequence();

    // Sobe (0 -> 1) e desce (1 -> 0)
    flashSequence.Append(
      DOTween.To(
        () => intensity,
        x =>
        {
          intensity = x;
          ApplyFlash(intensity);
        },
        1f,
        flashDuration * 0.5f
      )
    );

    flashSequence.Append(
      DOTween.To(
        () => intensity,
        x =>
        {
          intensity = x;
          ApplyFlash(intensity);
        },
        0f,
        flashDuration * 0.5f
      )
    );
  }

  private void ApplyFlash(float intensity)
  {
    for (int i = 0; i < renderers.Length; i++)
    {
      var r = renderers[i];
      r.GetPropertyBlock(block);

      Color baseColor = Color.Lerp(originalColors[i], flashColor, intensity);
      Color emissionColor = Color.Lerp(originalEmissionColors[i], flashEmission, intensity);

      block.SetColor("_BaseColor", baseColor);
      block.SetColor("_EmissionColor", emissionColor);

      if (emissionColor != Color.black)
        r.material.EnableKeyword("_EMISSION");
      else
        r.material.DisableKeyword("_EMISSION");

      r.SetPropertyBlock(block);
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
