using UnityEngine;

public class AbyssEye : Enemies
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
        if (player.Damaged) return;

        player.Health -= damage; // ignora a defesa
        player.Damaged = true;
        Debug.Log($"{player.name} tomou {damage} de dano fixo do Abyss Eye!! vida restante: {player.Health}");
    }
}
