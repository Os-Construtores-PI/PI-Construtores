using UnityEngine;

public class MercyGround : MonoBehaviour
{
    DataSystem dataSystem;
    private void Start()
    {
        dataSystem = FindAnyObjectByType<DataSystem>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Player player) || dataSystem == null) return;
        dataSystem.RespawnPlayer(player,GameContext.currentSlot);
    }
}
