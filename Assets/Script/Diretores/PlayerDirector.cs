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
  private PlayerConfig configPlayer;

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

  private void InitSpawner()
  {
    _playersSpawner = GetComponent<ManualPlayersSpawner>();
    _playersSpawner.SetObjects(
      new() { _firstPlayerPrefab, _fallbackPlayerPrefab, _secondPlayerPrefab }
    );
  }

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

  public void ActivatePlayers()
  {
    switch (DataDirector.Instance.GetGameMode())
    {
      case GameMode.Singleplayer:
        ActivateSinglePlayer();
        break;
      case GameMode.Multiplayer:
        ActivateMultiplayer();
        break;
    }

    TryRestoreSave();
  }

  private void ActivateSinglePlayer()
  {
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

  private Player SpawnPlayerAt(int index) => _playersSpawner.Spawn(index).GetComponent<Player>();

  private void TryRestoreSave()
  {
    if (DataDirector.Instance.GameHasSave())
      DataDirector.Instance.RespawnAllPlayers(DataDirector.Instance.GetCurrentSlot());
  }

  // =========================================================
  // SETUP INDIVIDUAL DE PLAYER
  // =========================================================

  private void SetupPlayer(Player player, Rect viewport)
  {
    int id = player.ID;

    if (!_playerHUDInstances.ContainsKey(id))
      _playerHUDInstances[id] = _hudDirector.InitializeHUD(player, _hudParent, _hudPrefab);

    if (!_playerCameras.ContainsKey(id))
      SetupCamera(player, viewport);

    player.Motor.Engine.BaseVelocity = Vector3.zero;

    player._OnHealthChanged.Invoke(player.Health / player.MaxHealth);

    QualityOfLife.CursorOptions(false);
    ApplyConfig(player);
  }

  // =========================================================
  // SETUP DE CÂMERA
  // =========================================================

  private void SetupCamera(Player player, Rect viewport)
  {
    int id = player.ID;

    GameObject camGroup = Instantiate(_cameraGroup);
    Transform groupRoot = camGroup.transform;

    GameObject camObj = groupRoot.Find(Constants.CameraGroup.MainCamera).gameObject;
    GameObject cinemachineObj = groupRoot.Find(Constants.CameraGroup.MainCinemachine).gameObject;
    GameObject boostCinemachineObj = groupRoot
      .Find(Constants.CameraGroup.BoostCinemachine)
      .gameObject;

    Camera unityCam = camObj.GetComponent<Camera>();
    CameraLogic camLogic = camObj.GetComponent<CameraLogic>();

    _hudDirector.InitializeCamera(id, camLogic);
    unityCam.rect = viewport;

    if (
      !cinemachineObj.TryGetComponent(out CinemachineCamera mainCinemachine)
      || !boostCinemachineObj.TryGetComponent(out CinemachineCamera boostCinemachine)
    )
    {
      return;
    }
    camLogic.SetTarget(player, mainCinemachine, boostCinemachine);
    player.SetCamera(mainCinemachine, boostCinemachine, unityCam);

    if (cinemachineObj.TryGetComponent(out CinemachineBasicMultiChannelPerlin noise))
      _hudDirector.InitializeNoise(id, noise);

    _playerCameras[id] = camObj;
  }

  // =========================================================
  // CONFIGURAÇÃO DE PLAYER
  // =========================================================

  private void ApplyConfig(Player player)
  {
    if (configPlayer == null)
    {
      Debug.LogWarning("[PlayerDirector] ConfigPlayer não atribuído no Inspector.");
      return;
    }
    configPlayer.SetConfig(player);
  }

  // =========================================================
  // PROPRIEDADES PÚBLICAS
  // =========================================================

  public Player FirstPlayerContext => _allPlayers.Count > 0 ? _allPlayers[0] : null;
}
