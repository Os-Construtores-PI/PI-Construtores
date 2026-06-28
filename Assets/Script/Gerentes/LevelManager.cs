using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Responsabilidade: ciclo de vida do nível.
/// - Orquestra o início da fase (StartWorld + intro dialogue)
/// - Responde a eventos de morte, respawn e fim de jogo
/// - Gerencia ativação/desativação de input dos players durante o nível
///
/// NÃO é responsabilidade deste script:
/// - Estado global de pause (→ GameDirector)
/// - Inicialização de sistemas da cena (→ GameDirector.StartWorld)
/// - Reação ao Tutorial (→ GameDirector)
/// </summary>
public class LevelManager : MonoBehaviour
{
  private DataDirector _dataSystem;
  private GameDirector _gameDirector;
  private HudDirector _hudDirector;

  [SerializeField]
  private DialogueTrigger introDialogue;

  private void Start()
  {
    _dataSystem = DataDirector.Instance;
    _gameDirector = FindAnyObjectByType<GameDirector>();
    _hudDirector = FindAnyObjectByType<HudDirector>();

    if (_dataSystem == null)
      Debug.LogError("[LevelManager] DataDirector.Instance é null! Verifique se existe na cena.");

    if (_gameDirector == null)
      Debug.LogError("[LevelManager] GameDirector não encontrado!");

    GlobalEventBus.Instance.Death.AddListener(PlayerDeathHandler);
    GlobalEventBus.Instance.Respawn.AddListener(RespawnPlayers);
    GlobalEventBus.Instance.EndGame.AddListener(PlayerEndGameHandler);

    StartCoroutine(StartLevelRoutine());
  }

  private void OnDestroy()
  {
    // CRÍTICO: sem isso, listeners acumulam se a cena recarregar.
    // Na segunda morte, PlayerDeathHandler roda duas vezes → double-pause → crash silencioso.
    if (GlobalEventBus.Instance == null)
      return;

    GlobalEventBus.Instance.Death.RemoveListener(PlayerDeathHandler);
    GlobalEventBus.Instance.Respawn.RemoveListener(RespawnPlayers);
    GlobalEventBus.Instance.EndGame.RemoveListener(PlayerEndGameHandler);

    Debug.Log("[LevelManager] Listeners removidos do GlobalEventBus.");
  }

  // ─── Início da fase ───────────────────────────────────────────────────────

  /// <summary>
  /// Orquestra o início do nível: inicializa sistemas via GameDirector,
  /// depois executa o diálogo de intro se houver um configurado.
  /// </summary>
  private IEnumerator StartLevelRoutine()
  {
    if (!_gameDirector)
    {
      Debug.LogError("[LevelManager] StartLevelRoutine abortado: GameDirector null.");
      yield break;
    }

    _gameDirector.StartWorld();

    if (introDialogue != null)
      yield return StartCoroutine(IntroDialogueRoutine());
  }

  /// <summary>
  /// Trava o player, exibe o diálogo de intro da fase e destrava ao fim.
  /// Fica aqui pois é um fluxo específico do nível, não do jogo global.
  /// </summary>
  private IEnumerator IntroDialogueRoutine()
  {
    yield return new WaitUntil(() => DialogueGlobal.Instance != null);
    yield return new WaitUntil(() => DialogueGlobal.Instance._painelDialogo != null);

    yield return null;

    var player = _gameDirector.playerDirector?.FirstPlayerContext;
    if (player != null)
      _gameDirector.SetLockPlayer(player, true);

    DialogueGlobal.Instance.SetTrigger(introDialogue);

    yield return null;

    bool dialogueFinished = false;

    void OnDialogueEnd()
    {
      dialogueFinished = true;
      DialogueGlobal.Instance.OndialogueEnd -= OnDialogueEnd;
    }

    DialogueGlobal.Instance.OndialogueEnd += OnDialogueEnd;
    DialogueGlobal.Instance.IniciarDialogo(introDialogue._dialogo);

    yield return new WaitUntil(() => dialogueFinished);

    player = _gameDirector.playerDirector?.FirstPlayerContext;
    if (player != null)
    {
      player.PlayerInput.actions.Disable();
      player.PlayerInput.actions.Enable();
      _gameDirector.SetLockPlayer(player, false);
    }
  }

  // ─── Eventos de nível ─────────────────────────────────────────────────────

  private void PlayerDeathHandler()
  {
    if (!_gameDirector)
    {
      Debug.LogError("[LevelManager] PlayerDeathHandler: GameDirector null.");
      return;
    }

    Debug.Log("[LevelManager] PlayerDeathHandler chamado.");
    _gameDirector.SetPauseWorld(true);
    SetPlayersInput(false);
  }

  private void PlayerEndGameHandler()
  {
    if (!_gameDirector)
    {
      Debug.LogError("[LevelManager] PlayerEndGameHandler: GameDirector null.");
      return;
    }

    Debug.Log("[LevelManager] PlayerEndGameHandler chamado.");
    _gameDirector.SetPauseWorld(true);
    QualityOfLife.ForEachPlayer(player =>
    {
      GameObject stopwatch = _hudDirector.GetPanel(player.ID, HudPanelType.Stopwatch)[0];
      if (stopwatch != null && stopwatch.TryGetComponent(out StopwatchHUD stopwatchHUD))
      {
        player.ApplyDiscount((int)stopwatchHUD.Elapsed);
        _dataSystem.SaveLevelRecord(
          _dataSystem.GetCurrentSlot(),
          SceneManager.GetActiveScene().name,
          player.ID,
          player.CurrentScore,
          player.HighestComboIndex
        );
      }
    });
    SetPlayersInput(false);
  }

  private void RespawnPlayers()
  {
    if (!_dataSystem || !_gameDirector)
    {
      Debug.LogError("[LevelManager] RespawnPlayers: dataSystem ou gameDirector null!");
      return;
    }

    Debug.Log("[LevelManager] RespawnPlayers chamado, iniciando coroutine.");
    StartCoroutine(RespawnRoutine());
  }

  private IEnumerator RespawnRoutine()
  {
    _gameDirector.SetPauseWorld(false);

    var slot = _dataSystem.GetCurrentSlot();
    _dataSystem.Commit();
    _dataSystem.RespawnAllPlayers(slot);

    yield return null;
    yield return null;

    Debug.Log("[LevelManager] Pós-respawn: aplicando SetParent e reativando input.");

    QualityOfLife.ForEachPlayer(player =>
    {
      if (player == null)
      {
        Debug.LogError("[LevelManager] Player null após respawn, pulando.");
        return;
      }

      player.transform.SetParent(null, true);
      Debug.Log($"[LevelManager] SetParent(null) aplicado em {player.name}.");

      GameObject stopwatch = _hudDirector.GetPanel(player.ID, HudPanelType.Stopwatch)[0];
      if (stopwatch != null && stopwatch.TryGetComponent(out StopwatchHUD stopwatchHUD))
      {
        stopwatchHUD.ResetStopwatch();
      }
    });

    SetPlayersInput(true);
  }

  // ─── Utilitários ──────────────────────────────────────────────────────────

  private void SetPlayersInput(bool active)
  {
    QualityOfLife.ForEachPlayer(player =>
    {
      if (player == null)
      {
        Debug.LogWarning("[LevelManager] SetPlayersInput: player null, pulando.");
        return;
      }

      var input = player.GetComponent<PlayerInput>();
      if (input == null)
      {
        Debug.LogWarning($"[LevelManager] PlayerInput não encontrado em {player.name}.");
        return;
      }

      if (active)
        input.ActivateInput();
      else
        input.DeactivateInput();

      Debug.Log($"[LevelManager] Input {(active ? "ativado" : "desativado")} em {player.name}.");
    });
  }
}
