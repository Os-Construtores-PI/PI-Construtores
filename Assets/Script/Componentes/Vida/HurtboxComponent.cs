using UnityEngine;

public class HurtboxComponent : MonoBehaviour
{
    private CombatEntities entity;
    private float damagedCooldownWalker = 0.0f;
    private bool can_take_damage = true;

    [SerializeField]
    private float damagedCooldown = 1f; // Tempo entre danos consecutivos

    private void Start()
    {
        SetEntity();
        if (entity == null || !TryGetComponent(out Collider collider) || !collider.isTrigger)
        {
            print(
                "PARENTE NÃO PODE RECEBER DANO OU ESTE GAMEOBJ FILHO ESTÁ SEM COLISÃO OU ESTÁ NO MODO NÃO TRIGGER"
            );
        }
    }

    private void Update()
    {
        if (!can_take_damage)
        {
            damagedCooldownWalker += Time.deltaTime;
            if (damagedCooldownWalker >= damagedCooldown)
            {
                damagedCooldownWalker = 0.0f;
                can_take_damage = true;
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
        // Verifica se o objeto está na camada "Entity".
        if (!collider.TryGetComponent(out HitboxComponent hitbox) || !can_take_damage)
            return;

        // Verifica se tem componente CombatEntities e se o dano está liberado.
        // Calcula fator de defesa (máx 80% de redução)
        float factor = Mathf.Clamp(entity.Defense / entity.MAX_DEFENSE, 0f, 0.80f);

        // Aplica dano reduzido pela defesa
        print($"VIDA // {entity.name} // (ANTES): {entity.Health}");
        entity.Health -= hitbox.Damage * (1 - factor);
        print($"VIDA // {entity.name} // (DEPOIS): {entity.Health}");
        entity.Damaged = true;

        // Ativa cooldown
        can_take_damage = false;
    }

    private void SetEntity()
    {
        Transform parent = transform.parent;
        entity = parent.GetComponent<CombatEntities>();
    }
}
