using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
  private DataDirector _dataSystem;
  private GameDirector _gameDirector;
  private HudDirector _hudDirector;

  [SerializeField]
  private DialogueTrigger introDialogue;

  [SerializeField]
  private List<RankTime> _rankTimes = new();
  public List<RankTime> RankTimes => _rankTimes;

  public RankType GetRank(float seconds)
  {
    var sorted = _rankTimes.OrderBy(rt => rt.Seconds).ToList();

    foreach (var rankTime in sorted)
    {
      if (seconds <= rankTime.Seconds)
        return rankTime.Rank;
    }

    return sorted.Count > 0 ? sorted[^1].Rank : default;
  }

  public void Start()
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

  public void OnDestroy()
  {
    if (GlobalEventBus.Instance == null)
      return;

    GlobalEventBus.Instance.Death.RemoveListener(PlayerDeathHandler);
    GlobalEventBus.Instance.Respawn.RemoveListener(RespawnPlayers);
    GlobalEventBus.Instance.EndGame.RemoveListener(PlayerEndGameHandler);

    Debug.Log("[LevelManager] Listeners removidos do GlobalEventBus.");
  }

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

  private IEnumerator IntroDialogueRoutine()
  {
    yield return new WaitUntil(() => DialogueGlobal.Instance != null);

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
      DialogueGlobal.Instance.OnDialogueEnd -= OnDialogueEnd;
    }

    DialogueGlobal.Instance.OnDialogueEnd += OnDialogueEnd;
    DialogueGlobal.Instance.StartDialogue(introDialogue.DialogueLines);

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
      GameObject stopwatchObj = _hudDirector.GetPanel(player.ID, HudPanelType.Stopwatch)[0];

      if (stopwatchObj != null && stopwatchObj.TryGetComponent(out StopwatchHUD stopwatchHUD))
      {
        float elapsedSeconds = (float)stopwatchHUD.Elapsed;
        int timeBonus = player.CalculateTimeScoreCurve(elapsedSeconds);
        player.AddScore(timeBonus);

        _dataSystem.SavePreviewScore(
          _dataSystem.GetCurrentSlot(),
          SceneManager.GetActiveScene().name,
          playerIndex: player.ID,
          score: player.CurrentScore
        );

        _dataSystem.SaveLevelRecord(
          slot: _dataSystem.GetCurrentSlot(),
          scene: SceneManager.GetActiveScene().name,
          playerIndex: player.ID,
          score: player.CurrentScore,
          time: elapsedSeconds,
          comboIndex: player.HighestComboIndex
        );

        // Debug.Log(
        //   $"[LevelManager] Recorde salvo! Player {player.ID} | Score Final: {player.CurrentScore} (Bônus Tempo: {timeBonus}) | Tempo: {elapsedSeconds}s"
        // );
      }
    });

    GlobalEventBus.Instance.EndGameProcessed.Invoke();
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
      player.ActionLayer.PopEveryState(player);
      player.MovementVector = Vector3.zero;

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
