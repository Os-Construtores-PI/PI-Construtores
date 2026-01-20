using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    DataDirector dataSystem;
    GameDirector gameDirector;

    [SerializeField] private bool startDialogueOnStart = false;


    private void Start()
    {
        dataSystem = FindAnyObjectByType<DataDirector>();
        gameDirector = FindAnyObjectByType<GameDirector>();
        StartLevel();
        GlobalEventBus.Instance.PLAYERTRIGGEREDDEATH.AddListener(PlayerDeathHandler);
        GlobalEventBus.Instance.PLAYERTRIGGEREDRESPAWN.AddListener(RespawnPlayers);
        GlobalEventBus.Instance.PLAYERTRIGGEREDENDGAME.AddListener(PlayerEndGameHandler);
    }
    private void StartLevel()
    {
        if(!gameDirector)
        {
            Debug.LogError("[LevelManager] GameDirector Não Encontrado!");
            return;
        }
        gameDirector.StartWorld();
        if(startDialogueOnStart)
         {
           TutorialDialogos dialogue = FindAnyObjectByType<TutorialDialogos>();
           if(dialogue)
            {
                dialogue.AtivarDialogo();
            }
        
         }
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
            dataSystem.RespawnPlayer(player, DataDirector.Instance.GetCurrentSlot());
            player.transform.SetParent(null, true); // Remove o pai mantendo a posição mundial
            player.GetComponent<PlayerInput>().ActivateInput();
        }
    }
}
