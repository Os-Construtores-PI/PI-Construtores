using UnityEngine;

public class LevelManager : MonoBehaviour
{
    DataSystem dataSystem;
    GameDirector gameDirector;
    private void Start()
    {
        dataSystem = FindAnyObjectByType<DataSystem>();
        gameDirector = FindAnyObjectByType<GameDirector>();
        GlobalEventBus.Instance.PLAYERTRIGGEREDDEATH.AddListener(PlayerDeathHandler);
    }
    private void PlayerDeathHandler(Player player)
    {
        if (!gameDirector) return;
        gameDirector.PauseWorld();
        
    }
    private void Respawn(Player player)
    {
        if (!dataSystem) return;

        dataSystem.RespawnPlayer(player, GameContext.currentSlot);
    }
}
