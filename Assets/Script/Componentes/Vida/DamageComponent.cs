using System;
using System.Linq;
using UnityEngine;

// Componente que aplica dano a qualquer CombatEntities que entrar na área
public class DamageComponent : MonoBehaviour
{
    [Header("Parâmetros de Dano")]
    [SerializeField] private float _maxDamage = 10f;       // Dano inicial
    [SerializeField] private float damageCooldown = 1f;    // Tempo entre danos consecutivos
    [SerializeField] protected Constants.Tags[] tags_to_damage;

    private float damage;
    private float damageCooldownWalker = 0.0f;
    private bool can_damage = true;

    // Propriedade pública de acesso ao dano
    [HideInInspector]
    public float Damage
    {
        get => damage;
        set => damage = value;
    }

    private void Start()
    {
        Damage = _maxDamage;
    }

    private void Update()
    {
        if (!can_damage)
        {
            damageCooldownWalker += Time.deltaTime;
            if (damageCooldownWalker >= damageCooldown)
            {
                damageCooldownWalker = 0.0f;
                can_damage = true;
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
        if (!collider.gameObject.layer.Equals(LayerMask.NameToLayer("Entity"))) return;

        // Verifica se tem componente CombatEntities e se o dano está liberado.
        if (collider.TryGetComponent(out CombatEntities entity) && can_damage && ComparisonTags(collider.tag))
        {
            // Calcula fator de defesa (máx 80% de redução)
            float factor = Mathf.Clamp(entity.Defense / entity.MAX_DEFENSE, 0f, 0.80f);

            // Aplica dano reduzido pela defesa
            print("VIDA: "+entity.Health);
            entity.Health -= Damage * (1 - factor);
            print("VIDA: "+entity.Health);
            entity.Damaged = true;

            // Ativa cooldown
            can_damage = false;
        }
    }
    private bool ComparisonTags(string tag)
    {
        foreach (Constants.Tags tg in tags_to_damage)
        {
            if (tag == tg.ToString())
            {
                return true;
            }
        }
        return false;
    }
}
