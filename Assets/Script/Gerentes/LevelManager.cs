using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    DataSystem dataSystem;
    GameDirector gameDirector;

    private void Start()
    {
        dataSystem = FindAnyObjectByType<DataSystem>();
        gameDirector = FindAnyObjectByType<GameDirector>();
        GlobalEventBus.Instance.PLAYERTRIGGEREDDEATH.AddListener(PlayerDeathHandler);
        GlobalEventBus.Instance.PLAYERTRIGGEREDRESPAWN.AddListener(RespawnPlayers);
        GlobalEventBus.Instance.PLAYERTRIGGEREDENDGAME.AddListener(PlayerEndGameHandler);
    }
    private void PlayerDeathHandler()
    {
        if (!gameDirector) return;
        gameDirector.SetPauseWorld(true);
        foreach(Player player in FindObjectsByType<Player>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
        {
            player.GetComponent<PlayerInput>().DeactivateInput();
        }
    }

    private void PlayerEndGameHandler()
    {
        if(!gameDirector) return;
        gameDirector.SetPauseWorld(true);
        foreach(Player player in FindObjectsByType<Player>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
        {
            player.GetComponent<PlayerInput>().DeactivateInput();
        }
    }
    private void RespawnPlayers()
    {
        if (!dataSystem) return;
        gameDirector.SetPauseWorld(false);
        foreach(Player player in FindObjectsByType<Player>(FindObjectsInactive.Exclude,FindObjectsSortMode.None))
        {
            dataSystem.RespawnPlayer(player, GameContext.CurrentSlot);
            player.GetComponent<PlayerInput>().ActivateInput();
        }
    }
}
