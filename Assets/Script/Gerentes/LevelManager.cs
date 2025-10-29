using UnityEngine;

public class LevelManager : MonoBehaviour
{
    DataSystem dataSystem;
    private void Start()
    {
        dataSystem = FindAnyObjectByType<DataSystem>();
        GlobalEventBus.Instance.PLAYERTRIGGEREDDEATH.AddListener(Respawn);
    }
    private void Respawn(Player player)
    {
        if (!dataSystem) return;
        dataSystem.RespawnPlayer(player, GameContext.currentSlot);
    }
}
