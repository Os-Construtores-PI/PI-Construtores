using UnityEngine;

public class HurtboxComponent : MonoBehaviour
{
  private CombatEntities entity;
  private float _initialCooldown;
  private float damagedCooldownWalker = 0.0f;

  [HideInInspector]
  public bool CanTakeDamage = true;

  public float DamageCooldown = 1f; // Tempo entre danos consecutivos

  private void Start()
  {
    SetEntity();
    _initialCooldown = DamageCooldown;
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
      if (damagedCooldownWalker >= DamageCooldown)
      {
        damagedCooldownWalker = 0.0f;
        CanTakeDamage = true;
        DamageCooldown = _initialCooldown;
      }
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    DamageLogic(other);
  }

  private void OnTriggerStay(Collider other)
  {
    DamageLogic(other);
  }

  private void DamageLogic(Collider collider)
  {
    if (!collider.TryGetComponent(out HitboxComponent hitbox) || !CanTakeDamage)
      return;

    float factor = Mathf.Clamp(entity.Defense / entity.MAX_DEFENSE, 0f, 0.80f);

    print($"VIDA // {entity.name} // (ANTES): {entity.Health}");
    entity.Health -= hitbox.Damage * (1 - factor);
    print($"VIDA // {entity.name} // (DEPOIS): {entity.Health}");
    entity.Damaged = true;

    // Ativa cooldown
    CanTakeDamage = false;
  }

  private void SetEntity()
  {
    Transform parent = transform.parent;
    entity = parent.GetComponent<CombatEntities>();
  }
}
