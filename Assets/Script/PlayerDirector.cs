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
    [SerializeField] private GameObject mainCameraPrefab;   // Camera + Brain + CameraLogic
    [SerializeField] private GameObject freeLookPrefab;     // FreeLook virtual camera

    [Header("Referências de cena")]
    [SerializeField] private HUDDirector hudDirector;       // Arraste na cena

    private ManualPlayersSpawner playersSpawner;
    private Transform hudParent;

    private void Awake()
    {
        playersSpawner = GetComponent<ManualPlayersSpawner>();
        playersSpawner.SetObjects(new() { firstPlayerPrefab, fallbackPlayerPrefab, secondPlayerPrefab });

        // Encontra o Canvas da cena onde o HUD será filho
        GameObject canvasObj = GameObject.FindWithTag(hudCanvasParent);
        if (canvasObj != null)
            hudParent = canvasObj.transform;
        else
            Debug.LogError($"Canvas '{hudCanvasParent}' não encontrado na cena!");
    }

    private void Start()
    {
        if (playersSpawner == null) return;
        if (hudParent == null) return;

        InitiatePlayers();
    }

    private void InitiatePlayers()
    {
        switch (GameContext.gameMode)
        {
            case GameMode.SINGLEPLAYER:
                SetupSinglePlayer();
                break;
            case GameMode.MULTIPLAYER:
                SetupMultiplayer();
                break;
        }
    }

    private void SetupSinglePlayer()
    {
        GameObject fp = playersSpawner.Spawn(0); // primeiro player
        GameObject fb = playersSpawner.deactivatedObject[1]; // fallback
        fb.SetActive(false);                      // fallback inativo

        Player playerComp = fp.GetComponent<Player>();

        // Instancia HUD
        hudDirector.InitializeHUD(playerComp, hudParent, hudPrefab);

        // Notifica sistema de eventos
        SpawnCamera(playerComp, new Rect(0f, 0f, 1f, 1f));

    
    }

    private void SetupMultiplayer()
    {
        GameObject fp = playersSpawner.Spawn(0); // player 1
        GameObject sp = playersSpawner.Spawn(2); // player 2

        Player player1 = fp.GetComponent<Player>();
        Player player2 = sp.GetComponent<Player>();

        // Instancia HUDs
        hudDirector.InitializeHUD(player1, hudParent, hudPrefab);
        hudDirector.InitializeHUD(player2, hudParent, hudPrefab);

        SpawnCamera(player1, new Rect(0f, 0f, 0.5f, 1f));
        SpawnCamera(player2, new Rect(0.5f, 0f, 0.5f, 1f));
    }

    private void SpawnCamera(Player targetPlayer, Rect viewport)
    {
        if (mainCameraPrefab == null || freeLookPrefab == null || targetPlayer == null) return;

        GameObject camObj = Instantiate(mainCameraPrefab);
        Camera unityCam = camObj.GetComponent<Camera>();
        CameraLogic camLogic = camObj.GetComponent<CameraLogic>();

        if (unityCam == null || camLogic == null)
        {
            Debug.LogError("MainCameraPrefab precisa de Camera + CameraLogic!");
            return;
        }

        unityCam.rect = viewport;

        GameObject freeLookObj = Instantiate(freeLookPrefab);
        if (!freeLookObj.TryGetComponent<CinemachineCamera>(out var freeLook))
        {
            Debug.LogError("FreeLookPrefab não possui CinemachineCamera!");
            return;
        }

        camLogic.SetTarget(targetPlayer, freeLook);
        targetPlayer.SetCinemachineCamera(freeLook);
    }
}

