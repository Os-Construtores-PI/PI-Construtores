using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TutorialGlobal;

public class GameDirector : MonoBehaviour
{
  private bool _worldStarted = false;

  [SerializeField]
  private AudioSource backgroundMusic;

  [SerializeField]
  public PlayerDirector playerDirector;

  [SerializeField]
  private StageIntroDirector stageIntro;

  [SerializeField]
  private StageIntroData stageData;

  private void Start()
  {
    GlobalEventBus.Instance.Pause.AddListener(SetPauseWorld);
    GlobalEventBus.Instance.LockDialogue.AddListener(SetLockPlayer);

    if (Instance != null)
      Instance.OnTutorialStateChanged += OnTutorialStateChanged;
  }

  private void OnDestroy()
  {
    if (GlobalEventBus.Instance != null)
    {
      GlobalEventBus.Instance.Pause.RemoveListener(SetPauseWorld);
      GlobalEventBus.Instance.LockDialogue.RemoveListener(SetLockPlayer);
    }

    if (Instance != null)
      Instance.OnTutorialStateChanged -= OnTutorialStateChanged;
  }

  // ─── Inicialização ────────────────────────────────────────────────────────

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

    if (GameContext.ShowStageIntro)
    {
      GameContext.ShowStageIntro = false;
      StartCoroutine(StartStageRoutine());
    }
    else
    {
      Player player = playerDirector.FirstPlayerContext;

      if (player != null)
        SetLockPlayer(player, false);
    }
  }

  // ─── Reset ────────────────────────────────────────────────────────────────

  public void ResetWorld()
  {
    foreach (
      var component in FindObjectsByType<Component>(
        FindObjectsInactive.Include,
        FindObjectsSortMode.None
      )
    )
    {
      if (component is IRespawnable respawnable && !respawnable.IsAlive)
      {
        respawnable.Respawn();
      }
    }
  }

  // ─── Pause ────────────────────────────────────────────────────────────────

  public void TogglePauseWorld()
  {
    SetPauseWorld(!GameContext.IsPaused);
  }

  public void SetPauseWorld(bool setPause)
  {
    if (setPause && !GameContext.CanPause())
      return;

    Time.timeScale = setPause ? 0f : 1f;
    GameContext.IsPaused = setPause;

    if (!setPause && playerDirector != null ? playerDirector.FirstPlayerContext : null != null)
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

  public void SetLockPlayer(Player player, bool set)
  {
    if (player == null)
      return;

    if (player.Motor != null)
    {
      player.Motor.Engine.enabled = !set;
      player.Motor.enabled = !set;
    }

    player.CameraLocked = set;
    player.IsHardLocked = set;
  }

  // ─── Tutorial ─────────────────────────────────────────────────────────────

  private void OnTutorialStateChanged(bool active)
  {
    if (!playerDirector || playerDirector.FirstPlayerContext == null)
      return;

    var player = playerDirector.FirstPlayerContext;

    if (active)
    {
      SetLockPlayer(player, true);
    }
    else
    {
      player.IgnoreGameplayInputThisFrame = true;
      StartCoroutine(UnlockPlayerInNextFrame(player));
    }
  }

  private IEnumerator UnlockPlayerInNextFrame(Player player)
  {
    yield return null;

    player.PlayerInput.actions.Disable();
    player.PlayerInput.actions.Enable();

    SetLockPlayer(player, false);

    player.IgnoreGameplayInputThisFrame = false;
    player.WaitForJumpRelease = true;
  }

  private IEnumerator StartStageRoutine()
  {
    yield return null;

    Player player = playerDirector.FirstPlayerContext;

    if (player != null)
      SetLockPlayer(player, true);

    HudDirector hudDirector = FindAnyObjectByType<HudDirector>();
    if (hudDirector != null)
      hudDirector.ResetAllStopwatches();

    yield return StartCoroutine(stageIntro.Play(stageData));

    if (player != null)
      SetLockPlayer(player, false);
  }
}
