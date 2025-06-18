using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DamageComponent : EntityBehavior
{

    [Header("Inimigos que irá triggar o dano")]
    [SerializeField] private EntityType[] enemies;
    private HashSet<EntityType> hashenemies;


    [Header("Parâmetros de Dano")]
    [SerializeField] private int damage;
    [SerializeField] private float damageCooldown;


    
    private bool can_damage;



    void Start()
    {
        foreach (EntityType entity in enemies)
        {
            hashenemies.Add(entity);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.layer.Equals(LayerMask.NameToLayer("Entity"))) return;

        if (other.TryGetComponent(out HealthComponent healthComponent))
            {
                if (hashenemies.Contains(healthComponent.entity) && can_damage)
                {
                    healthComponent.SubtractHealth(damage);
                    can_damage = false;
                    StartCoroutine(DamageCD(damageCooldown));
                }
            }
    }
    IEnumerator DamageCD(float CD)
    {
        yield return new WaitForSeconds(CD);
        can_damage = true;
    }
}
