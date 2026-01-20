using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(ManualPlayersSpawner))]
public class PlayerDirector : MonoBehaviour
{
    [Header("Prefabs de Player")]
    [SerializeField] private GameObject firstPlayerPrefab;
    [SerializeField] private GameObject fallbackPlayerPrefab;
    [SerializeField] private GameObject secondPlayerPrefab;

    [Header("Prefabs de HUD e Câmera")]
    [SerializeField] private GameObject hudPrefab;
    [SerializeField] private string hudCanvasParent = "Canvas";

    [SerializeField] private GameObject mainCameraPrefab;
    [SerializeField] private GameObject freeLookPrefab;

    [Header("Referências de cena")]
    [SerializeField] private HudDirector hudDirector;

    private ManualPlayersSpawner playersSpawner;
    private Transform hudParent;

    #region === CONFIGURAÇÃO ===
    [SerializeField] private ConfigPlayer configPlayer;
    #endregion


    private List<Player> allPlayers = new List<Player>();

    // Referências instanciadas
    private Dictionary<int, GameObject> playerHUDInstances = new();
    private Dictionary<int, GameObject> playerCameras = new();

    

    private void Awake()
    {
        playersSpawner = GetComponent<ManualPlayersSpawner>();
        playersSpawner.SetObjects(new() { firstPlayerPrefab, fallbackPlayerPrefab, secondPlayerPrefab });

        GameObject canvasObj = GameObject.FindWithTag(hudCanvasParent);
        if (canvasObj != null)
            hudParent = canvasObj.transform;
        else
            Debug.LogError($"Canvas '{hudCanvasParent}' não encontrado na cena!");
        CacheAllPlayers();
    }

    private void CacheAllPlayers()
    {
        allPlayers.Clear();
        for (int i = 0; i < playersSpawner.DeactivatedObjectsCount; i++)
        {
            Player playerComp = playersSpawner.GetDeactivatedObject(i).GetComponent<Player>();
            allPlayers.Add(playerComp);
        }
    }

    public void ActivatePlayers()
    {
        switch (DataDirector.Instance.GetGameMode())
        {
            case GameMode.SINGLEPLAYER:
                ActivateSinglePlayer();
                break;
            case GameMode.MULTIPLAYER:
                ActivateMultiplayer();
                break;
        }
        // Garante que o PauseMenu só é instanciado uma vez

    }


    private void ActivateSinglePlayer()
    {
        Player fp = playersSpawner.Spawn(0).GetComponent<Player>();
        Player fb = allPlayers[1];
        fb.gameObject.SetActive(false);

        SetupPlayer(fp, new Rect(0f, 0f, 1f, 1f));
    }

    private void ActivateMultiplayer()
    {
        Player fp = playersSpawner.Spawn(0).GetComponent<Player>();
        Player sp = playersSpawner.Spawn(2).GetComponent<Player>();

        SetupPlayer(fp, new Rect(0f, 0f, 0.5f, 1f));
        SetupPlayer(sp, new Rect(0.5f, 0f, 0.5f, 1f));
    }

    private void SetupPlayer(Player player, Rect viewport)
    {
        int playerID = player.ID;
        // Cria HUD apenas uma vez
        if (!playerHUDInstances.ContainsKey(playerID))
        {
            // Agora o HUD é inicializado APENAS pelo HUDDirector
            GameObject hudInstance = hudDirector.InitializeHUD(player, hudParent, hudPrefab);
            playerHUDInstances[playerID] = hudInstance;
        }

        // Cria câmera apenas uma vez
        if (!playerCameras.ContainsKey(playerID))
        {
            GameObject camObj = Instantiate(mainCameraPrefab);
            Camera unityCam = camObj.GetComponent<Camera>();
            CameraLogic camLogic = camObj.GetComponent<CameraLogic>();
            unityCam.rect = viewport;

            GameObject freeLookObj = Instantiate(freeLookPrefab);
            if (freeLookObj.TryGetComponent<CinemachineCamera>(out var freeLook))
            {
                camLogic.SetTarget(player, freeLook);
                player.SetCinemachineCamera(freeLook);
            }
            playerCameras[playerID] = camObj;
            LinkCanvasToCamera(unityCam, playerID);

        }
        player.gameObject.SetActive(true);
        player._OnHealthChanged.Invoke(player.Health / player.MaxHealth);
        ConfigPlayer(player);
    }
    private void LinkCanvasToCamera(Camera camera, int playerID)
    {
        if (playerHUDInstances.TryGetValue(playerID, out var hudInstance))
        {
            if (hudInstance.TryGetComponent<Canvas>(out var canvas))
            {
                canvas.worldCamera = camera;
                canvas.planeDistance = .4f;
            }
        }
    }
    private void ConfigPlayer(Player player)
    {
        if(configPlayer == null)
        {
            print("CONFIG PLAYER NULL");
            return;
        }
        configPlayer.SetConfig(player.Context);
        print($"Speed:{player.Context.PlayerSpeed}");
       
    }   

    public PlayerContext FirstPlayerContext
    {
        get
        {
            if (allPlayers.Count > 0)
                return allPlayers[0].Context;
            return null;
        }
    }
}
