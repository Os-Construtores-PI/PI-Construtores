using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sistema de salvamento e carregamento de dados do jogo.
/// Gerencia múltiplos slots de save, jogadores, inventário e itens dropados.
/// </summary>
public class DataSystem : MonoBehaviour
{
    [Header("Referências de Cena")]
    [Tooltip("Lista de jogadores ativos na cena (arrastados pelo editor).")]
    private List<Player> players = new();
    private List<ItemDropZone> droppedItems = new();
    [Header("Configuração")]
    private readonly int maxSlots = 3;

    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    private void Start()
    {
        // encontra todos os players ativos
        players.Clear();
        players.AddRange(FindObjectsByType<Player>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));

        // encontra todos os itens dropados ativos
        droppedItems.Clear();
        droppedItems.AddRange(FindObjectsByType<ItemDropZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));

        Debug.Log($"[DataSystem] Encontrados {players.Count} players e {droppedItems.Count} itens dropados na cena.");
    }
    #region SAVE
    // Salva checkpoint (players + dropped items da cena atual)
    public void SaveCheckpoint(int index, Vector3? checkpointPos = null)
    {
        if (!IsValidSlot(index)) return;

        SavedGameData gameData = GetGameData() ?? new SavedGameData(maxSlots);
        EnsureSlotsInitialized(gameData);

        Vector3 pos = checkpointPos ?? (players.Count > 0 ? players[0].transform.position : Vector3.zero);

        SavePlayersAtCheckpoint(gameData, index, pos);
        SaveDroppedItems(gameData, index);

        var json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(SavePath, Encrypt(json));

        Debug.Log($"[DataSystem] Checkpoint salvo no slot {index} em {SavePath}.");
    }


    // Salva progresso global (coletáveis/moedas/desbloqueios) — por enquanto placeholder
    public void Save(int index)
    {
        if (!IsValidSlot(index)) return;

        SavedGameData gameData = GetGameData() ?? new SavedGameData(maxSlots);
        EnsureSlotsInitialized(gameData);

        // aqui vão os dados globais (moedas, coletáveis, upgrades etc.)

        var json = JsonUtility.ToJson(gameData, true);
        var encrypted = Encrypt(json);

        File.WriteAllText(SavePath, encrypted);

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

        // pega último checkpoint da cena
        var levelData = slot.savedLevelDatas[^1];

        // restaura players
        for (int i = 0; i < Mathf.Min(players.Count, levelData.savedPlayers.Count); i++)
        {
            var p = players[i];
            var pdata = levelData.savedPlayers[i];

            p.transform.position = pdata.position;
            p.Health = pdata.health;

            p.Inventory.ClearItems(); // garante inventário limpo
            foreach (SavedItemEntry savedItem in pdata.inventory)
            {
                ItemData data = Resources.Load<ItemData>($"Items/{savedItem.savedItemName}");
                if (data != null)
                {
                    p.Inventory.AddItem(data, savedItem.savedItemQuantity);
                }
                else
                {
                    Debug.LogWarning($"[DataSystem] ItemData '{savedItem.savedItemName}' não encontrado em Resources/Items!");
                }
            }
        }

        // restaura dropped items
        foreach (var drop in FindObjectsByType<ItemDropZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            Destroy(drop.gameObject); // limpa cena

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

    #endregion

    #region AUXILIARES

    private void SavePlayersAtCheckpoint(SavedGameData gameData, int index, Vector3 checkpointPos)
    {
        var slot = gameData.savedSlots[index];
        var sceneName = SceneManager.GetActiveScene().name;

        // encontra ou cria LevelData para a cena
        var levelData = slot.savedLevelDatas.Find(l => l.levelName == sceneName);
        if (levelData == null)
        {
            levelData = new SavedLevelData(sceneName);
            slot.savedLevelDatas.Add(levelData);
        }

        // substitui lista de players
        levelData.savedPlayers.Clear();

        foreach (var p in players)
        {
            var pdata = new SavedPlayerData
            {
                position = checkpointPos, // todos no mesmo ponto
                health = p.Health
            };

            foreach (InventoryItem item in p.Inventory.GetItems())
            {
                pdata.inventory.Add(new SavedItemEntry(item.data.itemName, item.quantity));
            }

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

        // Carrega o jogo ou cria novo
        SavedGameData gameData = GetGameData() ?? new SavedGameData(maxSlots);

        // Garante que a lista de slots está inicializada
        gameData.savedSlots ??= new List<SavedSlotData>();

        // Preenche slots até o índice desejado
        while (gameData.savedSlots.Count <= index)
            gameData.savedSlots.Add(new SavedSlotData());

        // Salva o arquivo atualizado para manter consistência
        File.WriteAllText(SavePath, Encrypt(JsonUtility.ToJson(gameData, true)));

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
            data[i] ^= key[i % key.Length]; // XOR simples com a chave

        return Convert.ToBase64String(data);
    }

    private string Decrypt(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        byte[] data = Convert.FromBase64String(input);
        byte[] key = Encoding.UTF8.GetBytes(Constants.PersistentNames.CryptoKey);

        for (int i = 0; i < data.Length; i++)
            data[i] ^= key[i % key.Length]; // desfaz o XOR

        return Encoding.UTF8.GetString(data);
    }
    #endregion
}

