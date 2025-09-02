using UnityEngine;

public class AbyssEye : LiveEntities
{
    [Header("Abyss Eye Settings")]
    [SerializeField] private float damage = 5f;
    private void OnTriggerEnter(Collider collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            ApplyDamage(player);
        }
    }

    private void ApplyDamage(Player player)
    {
        player.TakeDamage(damage);
    }
}
