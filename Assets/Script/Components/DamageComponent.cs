using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageComponent : ComponentBehaviour
{

    [Header("Inimigos que irá triggar o dano")]
    [SerializeField] private EntityType[] enemies;
    private HashSet<EntityType> hashenemies = new();


    [Header("Parâmetros de Dano")]
    [SerializeField] private float damage;
    [SerializeField] private float damageCooldown;


    
    private bool can_damage = true;



    void Start()
    {
        foreach (EntityType entity in enemies)
        {
            hashenemies.Add(entity);
        }
        SetAttribute(nameof(damage), damage);
        SetAttribute(nameof(damageCooldown), damageCooldown);

        SubscribeToAttribute(nameof(damage), (newDamage) =>
        {
            damage = (float)newDamage;
        });
    }
    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.layer.Equals(LayerMask.NameToLayer("Entity"))) return;

        if (other.TryGetComponent(out HealthComponent healthComponent) && other.TryGetComponent(out BrainComponent brainComponent))
            {
                if (hashenemies.Contains(brainComponent.identity.TipoEntidade) && can_damage)
                {
                    healthComponent.SubtractHealth(GetAttribute<float>(nameof(damage)));
                    can_damage = false;
                    StartCoroutine(DamageCD(GetAttribute<float>(nameof(damageCooldown))));
                }
            }
    }
    IEnumerator DamageCD(float CD)
    {
        yield return new WaitForSeconds(CD);
        can_damage = true;
    }
}
