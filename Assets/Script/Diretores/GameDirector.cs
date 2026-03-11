using System;
using System.Collections;
using TMPro;
using UnityEngine;
using static TutorialGlobal;

public class GameDirector : MonoBehaviour
{
  private bool worldStarted = false;

  [SerializeField]
  private AudioSource backgroundMusic;

  [SerializeField]
  public PlayerDirector playerDirector;

  [SerializeField]
  private DialogueTrigger introDialogue;

  private void Start()
  {
    Debug.Log("GameDirector START rodou!");
    GlobalEventBus.Instance.PLAYERTRIGGEREDPAUSE.AddListener(SetPauseWorld);
    GlobalEventBus.Instance.PLAYERTRIGGEREDLOCKDIALOGUE.AddListener(SetLockPlayer);

    if (TutorialGlobal.Instance != null)
      TutorialGlobal.Instance.OnTutorialStateChanged += OnTutorialStateChanged;

    StartCoroutine(FluxoIntro());
    // O painel de Start agora é gerenciado por outro script,
    // então não precisamos fazer nada aqui.
  }

  private void OnDestroy()
  {
    if (TutorialGlobal.Instance != null)
      TutorialGlobal.Instance.OnTutorialStateChanged -= OnTutorialStateChanged;
  }

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

  private IEnumerator DestravarPlayerNextFrame(PlayerContext ctx)
  {
    yield return null;

    ctx.PlayerInput.actions.Disable();
    ctx.PlayerInput.actions.Enable();

    SetLockPlayer(ctx, false);

    ctx.IgnoreGameplayInputThisFrame = false;
    ctx.WaitForJumpRelease = true;
  }

  /// <summary>
  /// Inicia o mundo após o painel de Start chamar este método.
  /// </summary>
  public void StartWorld()
  {
    if (worldStarted)
    {
      Debug.LogError("[GameDirector] MUNDO JÁ INICIALIZADO, LÓGICA DUPLICADA");
      return;
    }
    worldStarted = true;
    // 🔹 Garante que o DataSystem exista
    if (!DataDirector.Instance)
    {
      Debug.LogError("[GameDirector] Nenhum DataSystem encontrado na cena!");
      return; // sem DataSystem não dá para continuar
    }

    // 🔹 Garante que o PlayerDirector exista
    if (!playerDirector)
    {
      playerDirector = FindAnyObjectByType<PlayerDirector>();
      if (!playerDirector)
      {
        Debug.LogError(
          "[GameDirector] Nenhum PlayerDirector encontrado. Cena Debug pode continuar sem jogadores."
        );
      }
    }

    // 🔹 Garante que a música de fundo exista
    if (!backgroundMusic)
    {
      backgroundMusic = FindAnyObjectByType<AudioSource>();
      if (!backgroundMusic)
      {
        Debug.LogError("[GameDirector] Nenhuma música de fundo encontrada.");
      }
    }

    // 🔹 Executa os sistemas que conseguir encontrar
    if (playerDirector)
    {
      playerDirector.ActivatePlayers();
    }
    if (backgroundMusic)
    {
      backgroundMusic.Play();
    }
    DataDirector.Instance.CollectScene();

    Debug.Log("[GameDirector] StartWorld executado com sucesso!");
  }

  public void TogglePauseWorld()
  {
    SetPauseWorld(!GameContext.IsPaused);
  }

  public void SetPauseWorld(bool setPause)
  {
    if (setPause && !GameState.CanPause())
      return;

    Time.timeScale = setPause ? 0f : 1f;
    GameState.IsPaused = setPause;

    if(!setPause && playerDirector?.FirstPlayerContext != null)
    {
      var ctx = playerDirector?.FirstPlayerContext;
      ctx.IgnoreGameplayInputThisFrame = true;
      StartCoroutine(CLearIgnoreInputNextFrame(ctx));
    }
  }

  public void ShutdownWorld()
  {
    // Aqui você pode desativar players, limpar câmeras, salvar progresso etc.
  }

  public void SetLockPlayer(PlayerContext playerContext, bool set)
  {
   

    if (playerContext == null)
      return;

    if (playerContext.PlayerController != null)
      playerContext.PlayerController.enabled = !set;
   

    playerContext.CameraLocked = set;
    playerContext.IsHardLocked = set;
  }

  private IEnumerator FluxoIntro()
  {
    yield return new WaitUntil(() => DialogueGlobal.Instance != null);
    yield return new WaitUntil(() => DialogueGlobal.Instance._painelDialogo != null);

    yield return null;

    if (!playerDirector)
      playerDirector = FindAnyObjectByType<PlayerDirector>();

    if (playerDirector && playerDirector.FirstPlayerContext != null)
      SetLockPlayer(playerDirector.FirstPlayerContext, true);

    if (introDialogue != null)
    {
      DialogueGlobal.Instance.SetTrigger(introDialogue);

      yield return null;

      bool dialogueFinished = false;

      DialogueGlobal.Instance.OndialogueEnd += () =>
      {
        dialogueFinished = true;
      };

      DialogueGlobal.Instance.IniciarDialogo(introDialogue._dialogo);

      // 👇 ESPERA O DIÁLOGO TERMINAR
      yield return new WaitUntil(() => dialogueFinished);
    }

    // 👇 depois que o diálogo acabar o jogo continua
    if (playerDirector && playerDirector.FirstPlayerContext != null)
    {
      var ctx = playerDirector.FirstPlayerContext;

      ctx.PlayerInput.actions.Disable();
      ctx.PlayerInput.actions.Enable();

      SetLockPlayer(ctx, false);
    }
  }

  private IEnumerator CLearIgnoreInputNextFrame(PlayerContext ctx)
  {
    yield return null;
    ctx.IgnoreGameplayInputThisFrame = false;
  }
}
