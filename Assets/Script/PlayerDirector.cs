using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ManualPlayersSpawner))]
public class PlayerDirector : MonoBehaviour
{
    [SerializeField] private GameObject firstPlayer = new();
    [SerializeField] private GameObject fallbackPlayer = new();
    [SerializeField] private GameObject secondPlayer = new();
    private ManualPlayersSpawner playersSpawner;
    private void Awake()
    {
        playersSpawner = GetComponent<ManualPlayersSpawner>();
        playersSpawner.SetObjects(new() { firstPlayer, fallbackPlayer, secondPlayer });
    }
    private void Start()
    {
        if (playersSpawner == null) return;
        InitiatePlayers();
    }
    private void InitiatePlayers()
    {
        switch (GameContext.gameMode)
        {
            case GameMode.SINGLEPLAYER:
                //GameObject fP = playersSpawner.disabledObject.Where(go => go == firstPlayer);
                break;
            case GameMode.MULTIPLAYER:
                break;
        }
    }
}
