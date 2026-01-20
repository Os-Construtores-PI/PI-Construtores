using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema de salvamento e carregamento de dados do jogo.
/// Gerencia múltiplos slots de save, jogadores, inventário e itens dropados.
/// </summary>

[DefaultExecutionOrder(2)]
public class DataDirector : MonoBehaviour
{
    public static DataDirector Instance {get; private set;}
    private bool _initialized;


    private List<Player> _players = new();
    private List<ItemDropZone> _droppedItems = new();
    private int _currentSlot = 0;
    private readonly int _maxSlots = 3;
    private bool _loadFromSave = false;
    private string _savePath => Constants.PersistentNames.DataPath;

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
#if UNITY_EDITOR
            DestroyImmediate(gameObject);
#else
            Destroy(gameObject);
#endif
            return;
        }

        Instance = this;
        Initialize();
    }
    #endregion

    #region Private
    private void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        DontDestroyOnLoad(gameObject); // persistente entre cenas
    }
    #endregion

    public void AddReferences()
    {
        // encontra todos os players ativos
        _players.Clear();
        _players.AddRange(FindObjectsByType<Player>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        _droppedItems.Clear();
        _droppedItems.AddRange(FindObjectsByType<ItemDropZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        Debug.LogWarning($"[DataSystem] Encontrados {_players.Count} players e {_droppedItems.Count} itens dropados na cena.");
    }


    #region SAVE

    public void SaveCheckpoint(int index)
    {
        if (!IsValidSlot(index)) return;

        SavedGameData gameData = GetGameData() ?? new SavedGameData(_maxSlots);
        EnsureSlotsInitialized(gameData);

        SavePlayersAtCheckpoint(gameData, index);
        SaveDroppedItems(gameData, index);

        var json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(_savePath, DataCryptography.Encrypt(json));

        Debug.Log($"[DataSystem] Checkpoint salvo no slot {index} em {_savePath}.");
    }

    public void Save(int index)
    {
        if (!IsValidSlot(index)) return;

        SavedGameData gameData = GetGameData() ?? new SavedGameData(_maxSlots);
        EnsureSlotsInitialized(gameData);

        // aqui vão os dados globais (moedas, coletáveis, upgrades etc.)

        var json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(_savePath, DataCryptography.Encrypt(json));

        Debug.Log($"[DataSystem] Progresso global salvo no slot {index} em {_savePath}.");
    }

    #endregion

    #region LOAD

    public void Load(int index)
    {
        if (!IsValidSlot(index)) return;

        SavedGameData gameData = GetGameData();
        if (gameData == null) return;

        var slot = gameData.savedSlots[index];
        if (slot.savedLevelDatas.Count == 0) return;

        var levelData = slot.savedLevelDatas[^1];

        // restaura players
        for (int i = 0; i < Mathf.Min(_players.Count, levelData.savedPlayers.Count); i++)
        {
            RespawnPlayer(_players[i], index, levelData.savedPlayers[i]);
        }

        // restaura dropped items
        foreach (var drop in FindObjectsByType<ItemDropZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            Destroy(drop.gameObject);

        foreach (var ditemData in levelData.savedDroppedItems)
        {
            ItemData data = Resources.Load<ItemData>($"Items/{ditemData.itemName}");
            if (data != null)
            {
                GameObject go = new("ItemDrop_" + ditemData.itemName);
                go.transform.position = ditemData.position;

                var dropZone = go.AddComponent<ItemDropZone>();
                dropZone.itemData = data;
                dropZone.SetId(ditemData.ID);
                dropZone.Initialize();
            }
            else
            {
                Debug.LogWarning($"[DataSystem] ItemData '{ditemData.itemName}' não encontrado em Resources/Items!");
            }
        }

        Debug.Log($"[DataSystem] Checkpoint carregado do slot {index}.");
    }

    /// <summary>
    /// Respawn seguro de um player específico usando savedPlayerData.
    /// </summary>
public void RespawnPlayer(Player player, int slotIndex, SavedPlayerData pdata = null)
{
    StartCoroutine(RespawnRoutine(player, slotIndex, pdata));
}

    private IEnumerator RespawnRoutine(Player player, int slotIndex, SavedPlayerData pdata)
    {
        yield return null; // espera 1 frame

        if (!IsValidSlot(slotIndex))
        {
            Debug.LogWarning($"[RespawnRoutine] Slot {slotIndex} não é válido.");
            yield break;
        }

        var gameData = GetGameData();
        if (gameData == null)
        {
            Debug.LogWarning($"[RespawnRoutine] Slot {slotIndex} não possui GameData salvo.");
            yield break;
        }

        var slot = gameData.savedSlots[slotIndex];
        if (slot.savedLevelDatas.Count == 0)
        {
            Debug.LogWarning($"[RespawnRoutine] Slot {slotIndex} não possui LevelData salvo.");
            yield break;
        }

        var levelData = slot.savedLevelDatas[^1];

        if (pdata == null)
        {
            int playerIndex = _players.IndexOf(player);
            if (playerIndex < 0 || playerIndex >= levelData.savedPlayers.Count)
            {
                Debug.LogWarning($"[RespawnRoutine] Player não encontrado.");
                yield break;
            }
            pdata = levelData.savedPlayers[playerIndex];
        }

        // ---- DESATIVAR COMPONENTES QUE PODEM SOBRESCREVER A POSIÇÃO ----
        if (player.TryGetComponent<CharacterController>(out var controller)) controller.enabled = false;
        var movementScripts = player.GetComponents<MonoBehaviour>();
        foreach (var script in movementScripts)
        {
            if (script != this) script.enabled = false; // desativa todos os scripts de movimento do player
        }

        // ---- APLICAR POSIÇÃO E VIDA ----
        player.transform.position = pdata.position;
        player.Charactercontroller.velocity.Set(0,0,0);
        player.Health = pdata.health;
        player.SetAmethysts(pdata.amethystsCount);

        // ---- RESTAURAR INVENTÁRIO ----
        player.Inventory.ClearItems();
        foreach (var item in pdata.inventory)
        {
            var itemData = Resources.Load<ItemData>($"Items/{item.savedItemName}");
            if (itemData != null)
                player.Inventory.AddItem(itemData, item.savedItemQuantity);
        }

        // ---- REATIVAR COMPONENTES ----
        if (controller != null) controller.enabled = true;
        foreach (var script in movementScripts)
        {
            if (script != this) script.enabled = true;
        }
    }





    #endregion

    #region AUXILIARES

    private void SavePlayersAtCheckpoint(SavedGameData gameData, int index)
    {
        var slot = gameData.savedSlots[index];
        var sceneName = SceneManager.GetActiveScene().name;

        var levelData = slot.savedLevelDatas.Find(l => l.levelName == sceneName);
        if (levelData == null)
        {
            levelData = new SavedLevelData(sceneName);
            slot.savedLevelDatas.Add(levelData);
        }

        levelData.savedPlayers.Clear();

        foreach (var p in _players)
        {
            var pdata = new SavedPlayerData
            {
                position = p.transform.position, // posição real de cada player
                health = p.Health,
                amethystsCount = p.Amethysts
            };

            foreach (InventoryItem item in p.Inventory.GetItems())
                pdata.inventory.Add(new SavedItemEntry(item.data.itemName, item.quantity));

            levelData.savedPlayers.Add(pdata);
        }
    }

    private void SaveDroppedItems(SavedGameData gameData, int index)
    {
        var slot = gameData.savedSlots[index];
        var sceneName = SceneManager.GetActiveScene().name;

        var levelData = slot.savedLevelDatas.Find(l => l.levelName == sceneName);
        if (levelData == null)
        {
            levelData = new SavedLevelData(sceneName);
            slot.savedLevelDatas.Add(levelData);
        }

        levelData.savedDroppedItems.Clear();

        foreach (var drop in FindObjectsByType<ItemDropZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (drop.itemData == null) continue;

            var ditemData = new SavedDroppedItem
            {
                ID = drop.ID,
                itemName = drop.itemData.itemName,
                position = drop.transform.position,
            };
            levelData.savedDroppedItems.Add(ditemData);
        }
    }

    private void EnsureSlotsInitialized(SavedGameData gameData)
    {
        if (gameData.savedSlots == null) gameData.savedSlots = new List<SavedSlotData>();
        while (gameData.savedSlots.Count < _maxSlots)
            gameData.savedSlots.Add(new SavedSlotData());
    }

    private bool IsValidSlot(int index) => index >= 0 && index < _maxSlots;

    public SavedGameData GetGameData()
    {
        if (!File.Exists(_savePath)) return null;

        try
        {
            var encrypted = File.ReadAllText(_savePath);
            var json = DataCryptography.Decrypt(encrypted);
            return JsonUtility.FromJson<SavedGameData>(json);
        }
        catch
        {
            Debug.LogWarning("[DataSystem] Falha ao carregar save. Arquivo corrompido?");
            return null;
        }
    }

    public SavedSlotData GetSlotData(int index)
    {
        if (!IsValidSlot(index))
        {
            Debug.LogWarning($"[DataSystem] Slot {index} inválido. Retornando null.");
            return null;
        }

        SavedGameData gameData = GetGameData() ?? new SavedGameData(_maxSlots);
        EnsureSlotsInitialized(gameData);

        return gameData.savedSlots[index];
    }

    public GameMode GetGameMode()
    {
        SavedGameData tmpData = GetGameData();
        return tmpData.savedConfig.GameMode;
    }
    public List<Player> GetPlayers()
    {
        return _players;
    }
    public int GetMaxSlots()
    {
        return _maxSlots;
    }
    public int GetCurrentSlot()
    {
        return _currentSlot;   
    }
    public void SetCurrentSlot(int index)
    {
        if(!IsValidSlot(index)) return;
        _currentSlot = index;
    }
    #endregion
}

