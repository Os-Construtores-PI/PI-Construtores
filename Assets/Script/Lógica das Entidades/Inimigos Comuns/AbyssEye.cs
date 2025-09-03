using System.Collections;
using UnityEngine;

public class AbyssEye : Enemies
{
    [Header("Abyss Eye Settings")]
    [SerializeField] private float damage;
    [SerializeField] private float _dashBlockDuration;
    
    private void OnTriggerEnter(Collider other)
    {
        //
        if (other.TryGetComponent(out Player player))
        {
            ApplyDamage(player);

            Vector3 knowbackDirection = (player.transform.position - transform.position).normalized;

            player.ApplyKnockback(knowbackDirection, 35f);

            StartCoroutine(BlockPlayerDash(player, _dashBlockDuration));
        }
    }

    private void ApplyDamage(Player player)
    {
        if (player.Damaged) return;

        player.Health -= damage; // ignora a defesa
        player.Damaged = true;
        
        Debug.Log($"{player.name} tomou {damage} de dano fixo do Abyss Eye!! vida restante: {player.Health}");

        // aplicação de empurrão
        Vector3 knowbackDirection = (player.transform.position - transform.position).normalized;
        float knowbackForce = 5f;

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            player.ApplyKnockback(knowbackDirection, knowbackForce);
        }
        else
        {
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(knowbackDirection * knowbackForce, ForceMode.Impulse);
            }
        }
    }

    private IEnumerator BlockPlayerDash(Player player, float duration)
    {
        // acessa o campo privado "canDash" via reflexão
        var field = typeof(Player).GetField("canDash", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(player, false); // desativa o dash
            yield return new WaitForSeconds(duration);
            field.SetValue(player, true);
        }
        else
        {
            Debug.LogWarning("Não foi possível acessar 'canDash' no Player");
        }
    }

}
