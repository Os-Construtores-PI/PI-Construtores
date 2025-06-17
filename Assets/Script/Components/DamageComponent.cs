using System.Collections;
using System.Linq;
using UnityEngine;

public class DamageComponent : EntityBehavior
{
    [SerializeField] private int damage;
    [SerializeField] private float damageCooldown;
    [SerializeField] private EntityType[] enemies;
    private bool can_damage;


    void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out HealthComponent healthComponent) && enemies.Contains(healthComponent.entity))
        {
            if (can_damage)
            {
                healthComponent.SubtractHealth(damage);
                StartCoroutine(DamageCD(damageCooldown));
            }
        }
    }
    IEnumerator DamageCD(float CD)
    {
        can_damage = false;
        yield return new WaitForSeconds(CD);
        can_damage = true;
    }
}
