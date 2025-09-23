using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema de salvamento e carregamento de dados do jogo.
/// Gerencia múltiplos slots de save, jogadores, inventário e itens dropados.
/// </summary>

[DefaultExecutionOrder(2)]
public class DataSystem : MonoBehaviour
{
    [Header("Referências de Cena")]
    [Tooltip("Lista de jogadores ativos na cena.")]
    private List<Player> players = new();
    private List<ItemDropZone> droppedItems = new();

    [Header("Configuração")]
    private readonly int maxSlots = 3;

    private string SavePath => Constants.PersistentNames.DataPath;

    public void AddReferences()
    {
        // encontra todos os players ativos
        players.Clear();
        players.AddRange(FindObjectsByType<Player>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        droppedItems.Clear();
        droppedItems.AddRange(FindObjectsByType<ItemDropZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
        Debug.Log($"[DataSystem] Encontrados {players.Count} players e {droppedItems.Count} itens dropados na cena.");
    }


    #region SAVE

    public void SaveCheckpoint(int index)
    {
        if (!IsValidSlot(index)) return;

        SavedGameData gameData = GetGameData() ?? new SavedGameData(maxSlots);
        EnsureSlotsInitialized(gameData);

        SavePlayersAtCheckpoint(gameData, index);
        SaveDroppedItems(gameData, index);

        var json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(SavePath, Encrypt(json));

        Debug.Log($"[DataSystem] Checkpoint salvo no slot {index} em {SavePath}.");
    }

    public void Save(int index)
    {
        if (!IsValidSlot(index)) return;

        SavedGameData gameData = GetGameData() ?? new SavedGameData(maxSlots);
        EnsureSlotsInitialized(gameData);

        // aqui vão os dados globais (moedas, coletáveis, upgrades etc.)

        var json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(SavePath, Encrypt(json));

        Debug.Log($"[DataSystem] Progresso global salvo no slot {index} em {SavePath}.");
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
        for (int i = 0; i < Mathf.Min(players.Count, levelData.savedPlayers.Count); i++)
        {
            RespawnPlayer(players[i], index, levelData.savedPlayers[i]);
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
                dropZone.quantity = ditemData.quantity;
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
            yield break;
        }

        var gameData = GetGameData();
        if (gameData == null)
        {
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
            Debug.LogWarning($"Players na lista: {string.Join(", ", players.Select(p => p.name))}");
            int playerIndex = players.IndexOf(player);
            if (playerIndex < 0 || playerIndex >= levelData.savedPlayers.Count)
            {
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
        player.Health = pdata.health;

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

        foreach (var p in players)
        {
            var pdata = new SavedPlayerData
            {
                position = p.transform.position, // posição real de cada player
                health = p.Health
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
                quantity = drop.quantity
            };
            levelData.savedDroppedItems.Add(ditemData);
        }
    }

    private void EnsureSlotsInitialized(SavedGameData gameData)
    {
        if (gameData.savedSlots == null) gameData.savedSlots = new List<SavedSlotData>();
        while (gameData.savedSlots.Count < maxSlots)
            gameData.savedSlots.Add(new SavedSlotData());
    }

    private bool IsValidSlot(int index) => index >= 0 && index < maxSlots;

    public SavedGameData GetGameData()
    {
        if (!File.Exists(SavePath)) return null;

        try
        {
            var encrypted = File.ReadAllText(SavePath);
            var json = Decrypt(encrypted);
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

        SavedGameData gameData = GetGameData() ?? new SavedGameData(maxSlots);
        EnsureSlotsInitialized(gameData);

        return gameData.savedSlots[index];
    }

    public List<Player> GetPlayers() => players;
    public int GetMaxSlots() => maxSlots;

    private string Encrypt(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        byte[] data = Encoding.UTF8.GetBytes(input);
        byte[] key = Encoding.UTF8.GetBytes(Constants.PersistentNames.CryptoKey);

        for (int i = 0; i < data.Length; i++)
            data[i] ^= key[i % key.Length];

        return Convert.ToBase64String(data);
    }

    private string Decrypt(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        byte[] data = Convert.FromBase64String(input);
        byte[] key = Encoding.UTF8.GetBytes(Constants.PersistentNames.CryptoKey);

        for (int i = 0; i < data.Length; i++)
            data[i] ^= key[i % key.Length];

        return Encoding.UTF8.GetString(data);
    }

    #endregion
}

