using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageComponent : ComponentBehaviour
{

    [Header("Inimigos que irá triggar o dano")]
    [SerializeField] private EntityType[] enemies;
    private HashSet<EntityType> hashenemies;


    [Header("Parâmetros de Dano")]
    [SerializeField] private int Damage;
    [SerializeField] private float DamageCooldown;


    
    private bool can_damage;



    void Start()
    {
        foreach (EntityType entity in enemies)
        {
            hashenemies.Add(entity);
        }
        SetAttribute(nameof(Damage), Damage);
        SetAttribute(nameof(DamageCooldown), DamageCooldown);
    }
    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.layer.Equals(LayerMask.NameToLayer("Entity"))) return;

        if (other.TryGetComponent(out HealthComponent healthComponent) && other.TryGetComponent(out BrainComponent brainComponent))
            {
                if (hashenemies.Contains(brainComponent.identity.entityType) && can_damage)
                {
                    healthComponent.SubtractHealth(GetAttribute<float>(nameof(Damage)));
                    can_damage = false;
                    StartCoroutine(DamageCD(GetAttribute<float>(nameof(DamageCooldown))));
                }
            }
    }
    IEnumerator DamageCD(float CD)
    {
        yield return new WaitForSeconds(CD);
        can_damage = true;
    }
}
