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
    [SerializeField] private HUDDirector hudDirector;

    private ManualPlayersSpawner playersSpawner;
    private Transform hudParent;

    private List<Player> allPlayers = new List<Player>();

    // Referências instanciadas
    private Dictionary<int, GameObject> playerHUDInstances = new();
    private Dictionary<int, GameObject> playerCameras = new();

    [HideInInspector] public GameObject startPanelInstance;

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
        InitializeStartPanel();
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

    private void InitializeStartPanel()
    {
        if (allPlayers.Count == 0) return;

        startPanelInstance = Instantiate(hudPrefab, hudParent);
        HUDDirector tempHUD = startPanelInstance.GetComponent<HUDDirector>();
        if (tempHUD != null)
            tempHUD.SetupStartOnly(); // mostra apenas painel de Start
    }

    public void ActivatePlayers()
    {
        // Destrói painel Start
        if (startPanelInstance != null)
        {
            Destroy(startPanelInstance);
            startPanelInstance = null;
        }

        switch (GameContext.gameMode)
        {
            case GameMode.SINGLEPLAYER:
                ActivateSinglePlayer();
                break;
            case GameMode.MULTIPLAYER:
                ActivateMultiplayer();
                break;
        }
    }

    private void ActivateSinglePlayer()
    {
        Player fp = playersSpawner.Spawn(0).GetComponent<Player>();
        Player fb = allPlayers[1];
        fb.gameObject.SetActive(false);

        SetupPlayerHUDAndCamera(fp, new Rect(0f, 0f, 1f, 1f));
    }

    private void ActivateMultiplayer()
    {
        Player fp = playersSpawner.Spawn(0).GetComponent<Player>();
        Player sp = playersSpawner.Spawn(2).GetComponent<Player>();

        SetupPlayerHUDAndCamera(fp, new Rect(0f, 0f, 0.5f, 1f));
        SetupPlayerHUDAndCamera(sp, new Rect(0.5f, 0f, 0.5f, 1f));
    }

    private void SetupPlayerHUDAndCamera(Player player, Rect viewport)
    {
        int playerID = player.ID;

        // Instancia HUD apenas uma vez
        if (!playerHUDInstances.ContainsKey(playerID))
        {
            GameObject hudInstance = Instantiate(hudPrefab, hudParent);
            playerHUDInstances[playerID] = hudInstance;
            hudDirector.InitializeHUD(player, hudParent, hudPrefab);
        }

        // Instancia câmera apenas uma vez
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
        }

        player.gameObject.SetActive(true);
    }
}
