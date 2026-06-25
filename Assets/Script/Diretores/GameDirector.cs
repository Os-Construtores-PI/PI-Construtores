using System.Collections;
using UnityEngine;
using static TutorialGlobal;

/// <summary>
/// Responsabilidade: estado global do jogo.
/// - Inicializa os sistemas da cena (PlayerDirector, música, DataDirector)
/// - Controla pause/unpause
/// - Expõe utilitário de lock de player (usado pelo GlobalEventBus e pelo LevelManager)
/// - Reage a mudanças de estado do Tutorial
///
/// NÃO é responsabilidade deste script:
/// - Diálogos de introdução de fase (→ LevelManager)
/// - Eventos de morte/respawn/fim de jogo (→ LevelManager)
/// </summary>
public class GameDirector : MonoBehaviour
{
  private bool _worldStarted = false;

  [SerializeField]
  private AudioSource backgroundMusic;

  [SerializeField]
  public PlayerDirector playerDirector;

  private void Start()
  {
    GlobalEventBus.Instance.Pause.AddListener(SetPauseWorld);
    GlobalEventBus.Instance.LockDialogue.AddListener(SetLockPlayer);

    if (TutorialGlobal.Instance != null)
      TutorialGlobal.Instance.OnTutorialStateChanged += OnTutorialStateChanged;
  }

  private void OnDestroy()
  {
    if (GlobalEventBus.Instance != null)
    {
      GlobalEventBus.Instance.Pause.RemoveListener(SetPauseWorld);
      GlobalEventBus.Instance.LockDialogue.RemoveListener(SetLockPlayer);
    }

    if (TutorialGlobal.Instance != null)
      TutorialGlobal.Instance.OnTutorialStateChanged -= OnTutorialStateChanged;
  }

  // ─── Inicialização ────────────────────────────────────────────────────────

  /// <summary>
  /// Inicializa todos os sistemas da cena. Chamado pelo LevelManager.
  /// </summary>
  public void StartWorld()
  {
    if (_worldStarted)
    {
      Debug.LogError("[GameDirector] MUNDO JÁ INICIALIZADO, LÓGICA DUPLICADA");
      return;
    }
    _worldStarted = true;

    if (!DataDirector.Instance)
    {
      Debug.LogError("[GameDirector] Nenhum DataDirector encontrado na cena!");
      return;
    }

    if (!playerDirector)
    {
      playerDirector = FindAnyObjectByType<PlayerDirector>();
      if (!playerDirector)
        Debug.LogError(
          "[GameDirector] Nenhum PlayerDirector encontrado. Cena Debug pode continuar sem jogadores."
        );
    }

    if (!backgroundMusic)
    {
      backgroundMusic = FindAnyObjectByType<AudioSource>();
      if (!backgroundMusic)
        Debug.LogError("[GameDirector] Nenhuma música de fundo encontrada.");
    }

    if (playerDirector)
      playerDirector.ActivatePlayers();

    if (backgroundMusic)
      backgroundMusic.Play();

    DataDirector.Instance.CollectScene();

    Debug.Log("[GameDirector] StartWorld executado com sucesso!");
  }

  // ─── Pause ────────────────────────────────────────────────────────────────

  public void TogglePauseWorld()
  {
    SetPauseWorld(!GameState.IsPaused);
  }

  public void SetPauseWorld(bool setPause)
  {
    if (setPause && !GameState.CanPause())
      return;

    Time.timeScale = setPause ? 0f : 1f;
    GameState.IsPaused = setPause;

    if (!setPause && playerDirector?.FirstPlayerContext != null)
    {
      var player = playerDirector.FirstPlayerContext;
      player.IgnoreGameplayInputThisFrame = true;
      StartCoroutine(ClearIgnoreInputNextFrame(player));
    }
  }

  private IEnumerator ClearIgnoreInputNextFrame(Player player)
  {
    yield return null;
    player.IgnoreGameplayInputThisFrame = false;
  }

  // ─── Lock de player ───────────────────────────────────────────────────────

  /// <summary>
  /// Trava ou destrava o controle e câmera de um player.
  /// Usado como listener do GlobalEventBus.LockDialogue e pelo LevelManager.
  /// </summary>
  public void SetLockPlayer(Player player, bool set)
  {
    if (player == null)
      return;

    if (player.CharacterController != null)
      player.CharacterController.enabled = !set;

    player.CameraLocked = set;
    player.IsHardLocked = set;
  }

  // ─── Tutorial ─────────────────────────────────────────────────────────────

  private void OnTutorialStateChanged(bool ativo)
  {
    if (!playerDirector || playerDirector.FirstPlayerContext == null)
      return;

    var ctx = playerDirector.FirstPlayerContext;

    if (ativo)
    {
      SetLockPlayer(ctx, true);
    }
    else
    {
      ctx.IgnoreGameplayInputThisFrame = true;
      StartCoroutine(DestravarPlayerNextFrame(ctx));
    }
  }

  private IEnumerator DestravarPlayerNextFrame(Player player)
  {
    yield return null;

    player.PlayerInput.actions.Disable();
    player.PlayerInput.actions.Enable();

    SetLockPlayer(player, false);

    player.IgnoreGameplayInputThisFrame = false;
    player.WaitForJumpRelease = true;
  }
}
