using UnityEngine;
using UnityEngine.Events;

public class HurtboxComponent : MonoBehaviour
{
  private CombatEntities entity;
  private float _initialCooldown;
  private float damagedCooldownWalker = 0.0f;

  [HideInInspector]
  public bool CanTakeDamage = true;

  [SerializeField]
  private float _damageCooldown = 1f;

  [Header("Layers que podem causar dano")]
  [SerializeField]
  private LayerMask _defaultDamageLayers;
  private LayerMask _currentDamageLayers;

  private void Start()
  {
    SetEntity();
    _initialCooldown = _damageCooldown;
    _currentDamageLayers = _defaultDamageLayers;

    if (entity == null || !TryGetComponent(out Collider collider) || !collider.isTrigger)
    {
      print(
        "PARENTE NÃO PODE RECEBER DANO OU ESTE GAMEOBJ FILHO ESTÁ SEM COLISÃO OU ESTÁ NO MODO NÃO TRIGGER"
      );
    }
  }

  private void Update()
  {
    if (!CanTakeDamage)
    {
      damagedCooldownWalker += Time.deltaTime;
      if (damagedCooldownWalker >= _damageCooldown)
      {
        ResetInvulnerability();
      }
    }
  }

  public void TriggerInvulnerability(float duration)
  {
    CanTakeDamage = false;
    _damageCooldown = duration;
    damagedCooldownWalker = 0f;
  }

  public void ResetInvulnerability()
  {
    damagedCooldownWalker = 0.0f;
    CanTakeDamage = true;
    _damageCooldown = _initialCooldown;
  }

  public void OverrideDamageLayers(LayerMask mask)
  {
    _currentDamageLayers = mask;
  }

  public void ResetDamageLayers()
  {
    _currentDamageLayers = _defaultDamageLayers;
  }

  public void OnTriggerEnter(Collider other) => DamageLogic(other);

  public void OnTriggerStay(Collider other) => DamageLogic(other);

  private void DamageLogic(Collider collider)
  {
    if (!CanTakeDamage)
      return;

    if (((1 << collider.gameObject.layer) & _currentDamageLayers) == 0)
      return;

    if (!collider.TryGetComponent(out HitboxComponent hitbox))
      return;

    entity.Health -= hitbox.Damage;
    entity.Damaged = true;
    hitbox.Hit.Invoke();
    TriggerInvulnerability(_initialCooldown);
  }

  private void SetEntity()
  {
    Transform parent = transform.parent;
    entity = parent.GetComponent<CombatEntities>();
  }
}
