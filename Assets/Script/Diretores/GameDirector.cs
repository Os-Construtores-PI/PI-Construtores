using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameDirector : MonoBehaviour
{
    private DataSystem dataSystem;
    private bool worldStarted = false; 


    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private PlayerDirector playerDirector;

    private void Start()
    {
        Debug.Log("GameDirector START rodou!");
        TryGetComponent(out dataSystem);
        GlobalEventBus.Instance.PLAYERTRIGGEREDPAUSE.AddListener(SetPauseWorld);        
        GlobalEventBus.Instance.PLAYERTRIGGEREDLOCKDIALOGUE.AddListener(SetLockPlayer);
        // O painel de Start agora é gerenciado por outro script,
        // então não precisamos fazer nada aqui.
    }

    /// <summary>
    /// Inicia o mundo após o painel de Start chamar este método.
    /// </summary>
    public void StartWorld()
    {
        if(worldStarted)
        {
            Debug.LogError("[GameDirector] MUNDO JÁ INICIALIZADO, LÓGICA DUPLICADA");
            return;
        }
        worldStarted = true;
        // 🔹 Garante que o DataSystem exista
        if (!dataSystem)
        {
            dataSystem = FindAnyObjectByType<DataSystem>();
            if (!dataSystem)
            {
                Debug.LogError("[GameDirector] Nenhum DataSystem encontrado na cena!");
                return; // sem DataSystem não dá para continuar
            }
        }

        // 🔹 Garante que o PlayerDirector exista
        if (!playerDirector)
        {
            playerDirector = FindAnyObjectByType<PlayerDirector>();
            if (!playerDirector)
            {
                Debug.LogError("[GameDirector] Nenhum PlayerDirector encontrado. Cena Debug pode continuar sem jogadores.");
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
        if(playerDirector)
        {
            playerDirector.ActivatePlayers();
        }
        if(backgroundMusic)
        {
            backgroundMusic.Play();
        }
        dataSystem.AddReferences();

        Debug.Log("[GameDirector] StartWorld executado com sucesso!");
    }
    public void TogglePauseWorld()
    {
        SetPauseWorld(!GameContext.IsPaused);
    }
    public void SetPauseWorld(bool setPause)
    {
        Time.timeScale = setPause ? 0f : 1f;
        GameContext.IsPaused = setPause;
    }
    public void ShutdownWorld()
    {
        // Aqui você pode desativar players, limpar câmeras, salvar progresso etc.
    }

    public void SetLockPlayer(PlayerContext playerContext, bool set)
    {
        /*if(set)
        {
            playerContext.PlayerInput.ActivateInput();
            playerContext.PlayerController.enabled = false;
        }
        else
        {
            playerContext.PlayerInput.DeactivateInput();
            playerContext.PlayerController.enabled = true;
        }
        */


        if (playerContext == null)
            return;

        if (playerContext.PlayerController != null)
            playerContext.PlayerController.enabled = !set;
        //playerContext.PlayerController.enabled = set;

        playerContext.CameraLocked = set;

    }
}
