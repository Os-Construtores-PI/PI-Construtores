using UnityEngine;

public class AbyssEye : LiveEntities
{
    [Header("Abyss Eye Settings")]
    [SerializeField] private float damage;
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
        player.Health -= damage; // ignora a defesa
        Debug.Log($"{player.name} tomou {damage} de dano fixo do Abyss Eye!! vida restante: {player.Health}");
    }
}
