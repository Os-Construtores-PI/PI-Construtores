using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Componente que gerencia o dano causado a entidades específicas ao colidir
public class DamageComponent : ComponentBehaviour
{
    [Header("Inimigos que irão ativar o dano")]
    [SerializeField] private EntityType[] enemies;  // Lista de tipos de entidades que podem ser danificadas
    private HashSet<EntityType> hashenemies = new(); // Conjunto para busca rápida dos tipos permitidos

    [Header("Parâmetros de Dano")]
    [SerializeField] private float damage;          // Quantidade de dano a ser aplicada
    [SerializeField] private float damageCooldown;  // Tempo mínimo entre danos consecutivos
    [SerializeField] private float maxDamage;       // Dano máximo permitido (usado para controle interno)

    private bool can_damage = true;  // Flag para controlar se o dano pode ser aplicado (cooldown)

    void Start()
    {
        // Inicializa o HashSet para otimizar as buscas por tipo de entidade
        foreach (EntityType entity in enemies)
        {
            hashenemies.Add(entity);
        }

        // Define os atributos para acesso via ComponentBehaviour
        SetAttribute(nameof(damage), damage);
        SetAttribute(nameof(damageCooldown), damageCooldown);
        SetAttribute("MAX_" + nameof(damage), maxDamage);

        // Inscreve para atualizar o valor interno de damage se o atributo for alterado
        SubscribeToAttribute(nameof(damage), (newDamage) =>
        {
            damage = (float)newDamage;
        });
    }

    // Evento chamado ao detectar colisão com outro collider
    void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto colidido está na layer "Entity" para evitar danos a objetos errados
        if (!other.gameObject.layer.Equals(LayerMask.NameToLayer("Entity"))) return;

        // Tenta obter os componentes de saúde e cérebro para validar se pode causar dano
        if (other.TryGetComponent(out HealthComponent healthComponent) && other.TryGetComponent(out BrainComponent brainComponent))
        {
            // Aplica dano apenas se o tipo da entidade estiver na lista permitida e se o cooldown permitir
            if (hashenemies.Contains(brainComponent.identity.TipoEntidade) && can_damage)
            {
                // Subtrai a vida do alvo usando o valor do atributo damage
                healthComponent.SubtractHealth(GetAttribute<float>(nameof(damage)));

                // Desabilita dano temporariamente para respeitar o cooldown
                can_damage = false;

                // Inicia coroutine para reabilitar o dano após o cooldown
                StartCoroutine(DamageCD(GetAttribute<float>(nameof(damageCooldown))));
            }
        }
    }

    // Coroutine para aguardar o cooldown entre aplicações de dano
    IEnumerator DamageCD(float CD)
    {
        yield return new WaitForSeconds(CD);
        can_damage = true;  // Reabilita o dano após o tempo definido
    }
}
