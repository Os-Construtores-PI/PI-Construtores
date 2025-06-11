using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private int damage = 100000000;
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        MiniPlayerEvents MPE = other.gameObject.GetComponent<MiniPlayerEvents>();
        MPE.DamagePlayer(damage);
    }
}
