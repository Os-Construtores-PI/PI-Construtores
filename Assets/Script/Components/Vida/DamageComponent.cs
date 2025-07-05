using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Componente que gerencia o dano causado a entidades específicas ao colidir
public class DamageComponent : ComponentBehaviour
{
    [Header("Inimigos que irão ativar o dano")]
    [SerializeField] private CombatEntities[] enemies;  // Lista de tipos de entidades que podem ser danificadas
    private HashSet<CombatEntities> hashenemies = new(); // Conjunto para busca rápida dos tipos permitidos

    [Header("Parâmetros de Dano")]
    private float damage;          // Quantidade de dano a ser aplicada
    [HideInInspector]
    public float Damage
    {
        get
        {
            return damage;
        }
        set
        {
            damage = value;
        }
    }
    [SerializeField] private float _maxDamage;       // Dano máximo permitido (usado para controle interno)

    [SerializeField] private float damageCooldown;  // Tempo mínimo entre danos consecutivos
    private float damageCooldownWalker = 0.0f;

    private bool can_damage = true;  // Flag para controlar se o dano pode ser aplicado (cooldown)
    void Update()
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

    void Start()
    {
        // Inicializa o HashSet para otimizar as buscas por tipo de entidade
        foreach (CombatEntities entity in enemies)
        {
            hashenemies.Add(entity);
        }
        Damage = _maxDamage;
    }

    // Evento chamado ao detectar colisão com outro collider
    void OnTriggerEnter(Collider other)
    {
        DamageLogic(other);
    }
    void OnTriggerStay(Collider other)
    {
        DamageLogic(other);
    }
    private void DamageLogic(Collider collider)
    {
        if (!collider.gameObject.layer.Equals(LayerMask.NameToLayer("Entity"))) return;
        if (collider.TryGetComponent(out CombatEntities entity) && can_damage)
        {
            if (hashenemies.Contains(entity))
            {
                float factor = Mathf.Clamp(entity.Defense / entity.MAX_DEFENSE, 0f, .80f);
                entity.Health -= Damage * (1 - factor);
                can_damage = false;
                entity.Damaged = true;
            }
        }
    }
}
