using Unity.Mathematics;
using UnityEngine;

public abstract class CombatEntities : LiveEntities
{
  #region --- Configurações de Combate ---

  [Header("Atributos de Combate")]
  [SerializeField, Min(5f)]
  private float combatCooldown = 5f;

  [SerializeField, Min(2f)]
  private float damagedCooldown = 2f;

  [Header("Atributos de Regeneração")]
  [SerializeField]
  private bool _enableRegen = true;

  [SerializeField, Min(3f)]
  private float regenerationInterval = 5f;

  [HideInInspector]
  public bool Damaged;

  private bool _inCombat;
  private float combatTimer;
  private float damagedTimer;

  #endregion

  #region --- Componentes e HUD ---

  protected HealthHUD _healthHUD;

  #endregion

  #region --- Stats e Dicionários ---

  [HideInInspector]
  [Stat(nameof(EnableRegen))]
  public bool EnableRegen
  {
    get => _enableRegen;
    set
    {
      if (value == false)
      {
        CancelInvoke(nameof(RegenerateHealth));
      }
      _enableRegen = value;
    }
  }

  [HideInInspector]
  [Stat(nameof(regenerationInterval))]
  public float RegenerationInterval
  {
    get => regenerationInterval;
    set => regenerationInterval = value;
  }

  [HideInInspector]
  [Stat(nameof(combatCooldown))]
  public float CombatCooldown
  {
    get => combatCooldown;
    set => combatCooldown = value;
  }

  [HideInInspector]
  [Stat(nameof(damagedCooldown))]
  public float DamagedCooldown
  {
    get => damagedCooldown;
    set => damagedCooldown = value;
  }

  #endregion

  #region --- Ciclo de Vida ---

  public override void Awake()
  {
    base.Awake();
    _OnDamage.AddListener(EnterCombat);

    Stats.OnNumModified.AddListener(HandleNumericStatChange);
    Stats.OnBoolModified.AddListener(HandleBoolStatChange);
    ;
    InitializeStats();

    if (EnableRegen)
    {
      InvokeRepeating(nameof(RegenerateHealth), 0f, regenerationInterval);
    }
  }

  public virtual void Update()
  {
    HandleCombatTimer();
    HandleDamagedCooldown();
  }

  #endregion

  #region --- Combate e Regeneração ---

  private void EnterCombat() => _inCombat = true;

  private void HandleCombatTimer()
  {
    if (!_inCombat)
      return;

    combatTimer += Time.deltaTime;
    if (combatTimer >= combatCooldown)
    {
      _inCombat = false;
      combatTimer = 0f;
    }
  }

  private void HandleDamagedCooldown()
  {
    if (!Damaged)
      return;

    damagedTimer += Time.deltaTime;
    if (damagedTimer >= damagedCooldown)
    {
      Damaged = false;
      damagedTimer = 0f;
    }
  }

  private void RegenerateHealth()
  {
    if (!_inCombat)
    {
      float regenAmount = MaxHealth * 0.06f;
      Health += regenAmount;
    }
  }

  #endregion

  #region --- HUD ---

  public void SetHealthHUD(HealthHUD hud)
  {
    if (hud == null)
      return;

    _healthHUD = hud;

    UpdateHUDVisibility(gameObject.activeInHierarchy);
  }

  private void OnEnable()
  {
    UpdateHUDVisibility(true);
  }

  private void OnDisable()
  {
    UpdateHUDVisibility(false);
  }

  private void UpdateHUDVisibility(bool isActive)
  {
    if (_healthHUD != null)
    {
      _healthHUD.gameObject.SetActive(isActive);
    }
  }
  #endregion
}
