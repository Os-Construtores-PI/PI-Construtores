using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    private DataDirector _dataSystem;
    private GameDirector _gameDirector;

    [SerializeField]
    private bool startDialogueOnStart = false;

    private void Start()
    {
        _dataSystem = DataDirector.Instance;
        _gameDirector = FindAnyObjectByType<GameDirector>();

        if (_dataSystem == null)
            Debug.LogError(
                "[LevelManager] DataDirector.Instance é null! Verifique se existe na cena."
            );

        if (_gameDirector == null)
            Debug.LogError("[LevelManager] GameDirector não encontrado!");

        StartLevel();

        // Registra listeners
        GlobalEventBus.Instance.PLAYERTRIGGEREDDEATH.AddListener(PlayerDeathHandler);
        GlobalEventBus.Instance.PLAYERTRIGGEREDRESPAWN.AddListener(RespawnPlayers);
        GlobalEventBus.Instance.PLAYERTRIGGEREDENDGAME.AddListener(PlayerEndGameHandler);
    }

    private void OnDestroy()
    {
        // CRÍTICO: sem isso, listeners acumulam se a cena recarregar.
        // Na segunda morte, PlayerDeathHandler roda duas vezes → double-pause → crash silencioso.
        if (GlobalEventBus.Instance == null)
            return;

        GlobalEventBus.Instance.PLAYERTRIGGEREDDEATH.RemoveListener(PlayerDeathHandler);
        GlobalEventBus.Instance.PLAYERTRIGGEREDRESPAWN.RemoveListener(RespawnPlayers);
        GlobalEventBus.Instance.PLAYERTRIGGEREDENDGAME.RemoveListener(PlayerEndGameHandler);

        Debug.Log("[LevelManager] Listeners removidos do GlobalEventBus.");
    }

    private void StartLevel()
    {
        if (!_gameDirector)
        {
            Debug.LogError("[LevelManager] GameDirector não encontrado, StartLevel abortado.");
            return;
        }

        _gameDirector.StartWorld();

        if (startDialogueOnStart) { }
    }

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
        _dataSystem.Commit();
        _dataSystem.RespawnAllPlayers(_dataSystem.GetCurrentSlot());

        // Frame 1: yield interno do DataDirector.RespawnRoutine roda aqui.
        // O CharacterController é desativado, transform.position é aplicado,
        // depois CC e behaviours são reativados — tudo isso neste frame.
        yield return null;

        // Frame 2: margem extra para garantir que o CharacterController
        // já processou o novo transform antes de reativar o input.
        // Em builds IL2CPP isso é necessário — o pipeline de física
        // pode atrasar 1 frame a mais que no Editor Mono.
        yield return null;

        Debug.Log("[LevelManager] Pós-respawn: aplicando SetParent e reativando input.");

        // SetParent ANTES do ActivateInput — o input ativo com parent errado
        // pode causar comportamento de câmera ou movimento incorreto no mesmo frame.
        foreach (
            Player player in FindObjectsByType<Player>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            )
        )
        {
            if (player == null)
            {
                Debug.LogError("[LevelManager] Player null após respawn, pulando.");
                continue;
            }

            // Remove o pai mantendo posição mundial
            player.transform.SetParent(null, true);
            Debug.Log($"[LevelManager] SetParent(null) aplicado em {player.name}.");
        }

        // Ativa input num passo separado, depois que todos os transforms estão corretos
        SetPlayersInput(true);
    }

    // Centraliza a lógica de input para evitar GetComponent sem null check espalhado
    private void SetPlayersInput(bool active)
    {
        foreach (
            Player player in FindObjectsByType<Player>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            )
        )
        {
            if (player == null)
            {
                Debug.LogWarning("[LevelManager] SetPlayersInput: player null, pulando.");
                continue;
            }

            var input = player.GetComponent<PlayerInput>();
            if (input == null)
            {
                Debug.LogWarning($"[LevelManager] PlayerInput não encontrado em {player.name}.");
                continue;
            }

            if (active)
                input.ActivateInput();
            else
                input.DeactivateInput();

            Debug.Log(
                $"[LevelManager] Input {(active ? "ativado" : "desativado")} em {player.name}."
            );
        }
    }
}
