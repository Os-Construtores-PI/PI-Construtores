using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Orquestra o ciclo de vida dos jogadores: spawning, câmeras, HUD e configuração.
/// Requer um <see cref="ManualPlayersSpawner"/> no mesmo GameObject.
/// </summary>
[RequireComponent(typeof(ManualPlayersSpawner))]
public class PlayerDirector : MonoBehaviour
{
    // =========================================================
    // PREFABS DE PLAYER
    // =========================================================

    [Header("Prefabs de Player")]
    [SerializeField]
    private GameObject _firstPlayerPrefab;

    [SerializeField]
    private GameObject _fallbackPlayerPrefab; // Usado como placeholder inativo no single

    [SerializeField]
    private GameObject _secondPlayerPrefab;

    // =========================================================
    // PREFABS DE HUD E CÂMERA
    // =========================================================

    [Header("Prefabs de HUD e Câmera")]
    [SerializeField]
    private GameObject _hudPrefab;

    [Tooltip("Tag do Canvas pai onde os HUDs serão instanciados.")]
    [SerializeField]
    private string _hudCanvasParent = "Canvas";

    [Tooltip("Prefab do grupo de câmeras (MainCamera + Cinemachine + LockOn).")]
    [SerializeField]
    private GameObject _cameraGroup;

    // =========================================================
    // REFERÊNCIAS DE CENA
    // =========================================================

    [Header("Referências de cena")]
    [SerializeField]
    private HudDirector _hudDirector;

    [SerializeField]
    private ConfigPlayer configPlayer;

    // =========================================================
    // ESTADO INTERNO
    // =========================================================

    private ManualPlayersSpawner _playersSpawner;
    private Transform _hudParent;

    /// <summary>Todos os players registrados (ativos ou não).</summary>
    private readonly List<Player> _allPlayers = new();

    /// <summary>HUD instanciado por playerID.</summary>
    private readonly Dictionary<int, GameObject> _playerHUDInstances = new();

    /// <summary>Câmera principal instanciada por playerID.</summary>
    private readonly Dictionary<int, GameObject> _playerCameras = new();

    // =========================================================
    // VIEWPORT CONSTANTS
    // =========================================================

    private static readonly Rect SingleplayerViewport = new(0f, 0f, 1f, 1f);
    private static readonly Rect MultiplayerLeftViewport = new(0f, 0f, 0.5f, 1f);
    private static readonly Rect MultiplayerRightViewport = new(0.5f, 0f, 0.5f, 1f);

    // =========================================================
    // UNITY LIFECYCLE
    // =========================================================

    public void Awake()
    {
        InitSpawner();
        InitHudParent();
        CacheAllPlayers();
    }

    // =========================================================
    // INICIALIZAÇÃO
    // =========================================================

    /// <summary>Configura o spawner com a lista de prefabs na ordem correta.</summary>
    private void InitSpawner()
    {
        _playersSpawner = GetComponent<ManualPlayersSpawner>();
        _playersSpawner.SetObjects(
          new() { _firstPlayerPrefab, _fallbackPlayerPrefab, _secondPlayerPrefab }
        );
    }

    /// <summary>Localiza e armazena o Transform do Canvas para parenting dos HUDs.</summary>
    private void InitHudParent()
    {
        GameObject canvasObj = GameObject.FindWithTag(_hudCanvasParent);
        if (canvasObj != null)
            _hudParent = canvasObj.transform;
        else
            Debug.LogError(
              $"[PlayerDirector] Canvas com tag '{_hudCanvasParent}' não encontrado na cena!"
            );
    }

    /// <summary>
    /// Percorre todos os objetos desativados no spawner e armazena seus componentes <see cref="Player"/>.
    /// Deve ser chamado após <see cref="InitSpawner"/>.
    /// </summary>
    private void CacheAllPlayers()
    {
        _allPlayers.Clear();

        int count = _playersSpawner.DeactivatedObjectsCount;
        for (int i = 0; i < count; i++)
        {
            Player player = _playersSpawner.GetDeactivatedObject(i).GetComponent<Player>();
            _allPlayers.Add(player);
        }
    }

    // =========================================================
    // ATIVAÇÃO DE PLAYERS
    // =========================================================

    /// <summary>
    /// Ativa os jogadores de acordo com o modo de jogo atual e restaura saves, se existirem.
    /// </summary>
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

        TryRestoreSave();
    }

    private void ActivateSinglePlayer()
    {
        // Spawna o primeiro player e garante que o fallback fique desativado
        Player fp = SpawnPlayerAt(0);
        _allPlayers[1].gameObject.SetActive(false);

        SetupPlayer(fp, SingleplayerViewport);
    }

    private void ActivateMultiplayer()
    {
        Player fp = SpawnPlayerAt(0);
        Player sp = SpawnPlayerAt(2);

        SetupPlayer(fp, MultiplayerLeftViewport);
        SetupPlayer(sp, MultiplayerRightViewport);
    }

    /// <summary>Faz spawn de um player pelo índice e retorna seu componente <see cref="Player"/>.</summary>
    private Player SpawnPlayerAt(int index) => _playersSpawner.Spawn(index).GetComponent<Player>();

    /// <summary>Restaura dados salvos para todos os jogadores, se houver save ativo.</summary>
    private void TryRestoreSave()
    {
        if (DataDirector.Instance.GameHasSave())
            DataDirector.Instance.RespawnAllPlayers(DataDirector.Instance.GetCurrentSlot());
    }

    // =========================================================
    // SETUP INDIVIDUAL DE PLAYER
    // =========================================================

    /// <summary>
    /// Inicializa HUD e câmera para o player (caso ainda não existam) e aplica configurações.
    /// </summary>
    private void SetupPlayer(Player player, Rect viewport)
    {
        int id = player.ID;

        if (!_playerHUDInstances.ContainsKey(id))
            _playerHUDInstances[id] = _hudDirector.InitializeHUD(player, _hudParent, _hudPrefab);

        if (!_playerCameras.ContainsKey(id))
            SetupCamera(player, viewport);

        player.gameObject.SetActive(true);

        // Força atualização visual da barra de vida ao ativar o player
        player._OnHealthChanged.Invoke(player.Health / player.MaxHealth);

        ApplyConfig(player);
    }

    // =========================================================
    // SETUP DE CÂMERA
    // =========================================================

    /// <summary>
    /// Instancia o grupo de câmeras, configura viewport e vincula ao player e ao HUD.
    /// </summary>
    private void SetupCamera(Player player, Rect viewport)
    {
        int id = player.ID;

        // Instancia o grupo completo (MainCamera + Cinemachine + LockOn)
        GameObject camGroup = Instantiate(_cameraGroup);
        Transform groupRoot = camGroup.transform;

        GameObject camObj = groupRoot.Find(Constants.CameraGroup.MainCamera).gameObject;
        GameObject cinemachineObj = groupRoot.Find(Constants.CameraGroup.CinemachineCamera).gameObject;
        GameObject lockOnObj = groupRoot.Find(Constants.CameraGroup.CinemachineLockOn).gameObject;
        GameObject lockOnGroupObj = groupRoot.Find(Constants.CameraGroup.LockInGroup).gameObject;

        Camera unityCam = camObj.GetComponent<Camera>();
        CameraLogic camLogic = camObj.GetComponent<CameraLogic>();

        // Registra câmera no HUD e define a janela de viewport (split-screen)
        _hudDirector.InitializeCamera(id, camLogic);
        unityCam.rect = viewport;

        // Vincula Cinemachine e LockOn ao player, se todos os componentes existirem
        if (
          cinemachineObj.TryGetComponent(out CinemachineCamera cinemachine)
          && lockOnObj.TryGetComponent(out CinemachineCamera lockOnCinemachine)
          && lockOnGroupObj.TryGetComponent(out CinemachineTargetGroup group)
        )
        {
            camLogic.SetTarget(player, cinemachine, lockOnCinemachine);
            player.SetCamera(cinemachine, lockOnCinemachine, group, unityCam);
        }

        _playerCameras[id] = camObj;
    }

    // =========================================================
    // CONFIGURAÇÃO DE PLAYER
    // =========================================================

    /// <summary>Aplica o <see cref="ConfigPlayer"/> ao contexto do player.</summary>
    private void ApplyConfig(Player player)
    {
        if (configPlayer == null)
        {
            Debug.LogWarning("[PlayerDirector] ConfigPlayer não atribuído no Inspector.");
            return;
        }
        configPlayer.SetConfig(player.Context);
    }

    // =========================================================
    // PROPRIEDADES PÚBLICAS
    // =========================================================

    /// <summary>Retorna o contexto do primeiro player, ou <c>null</c> se não houver nenhum.</summary>
    public PlayerContext FirstPlayerContext => _allPlayers.Count > 0 ? _allPlayers[0].Context : null;
}
