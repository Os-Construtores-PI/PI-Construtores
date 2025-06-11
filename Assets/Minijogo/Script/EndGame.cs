using UnityEngine;

public class EndGame : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        MiniPlayerEvents MPE = other.gameObject.GetComponent<MiniPlayerEvents>();
        MPE.WinPlayer();
    }
}
