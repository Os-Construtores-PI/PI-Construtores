using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Sistema de salvamento e carregamento de dados do jogo.
/// Gerencia múltiplos slots de save, jogadores, inventário e itens dropados.
/// </summary>
public class DataSystem : MonoBehaviour
{
    [Header("Referências de Cena")]
    [Tooltip("Lista de jogadores ativos na cena (arrastados pelo editor).")]
    public List<Player> players = new();

    private readonly int maxSlots = 3;
    private readonly string cryptoKey = "MySecretKey123"; // chave simples para XOR

    // Caminho único do arquivo de save
    private string SavePath => Path.Combine(Application.persistentDataPath, "GAMEDATA.json");

    #region SAVE

    public void Save(int index)
    {
        if (!IsValidSlot(index)) return;

        var gameData = new SavedGameData();

        SavePlayers(gameData, index);
        SaveDroppedItems(gameData, index);

        var json = JsonUtility.ToJson(gameData, true);
        var encrypted = Encrypt(json);

        File.WriteAllText(SavePath, encrypted);

        Debug.Log($"[DataSystem] Jogo salvo no slot {index} em {SavePath}.");
    }

    private void SavePlayers(SavedGameData gameData, int index)
    {
        foreach (Player p in players)
        {
            SavedPlayerData playerData = new()
            {
                playerId = p.ID,
                position = p.transform.position,
                health = p.Health,
            };

            foreach (var item in p.Inventario.GetItems())
            {
                playerData.inventory.Add(new SavedItemEntry
                {
                    itemName = item.data.itemName,
                    quantity = item.quantity
                });
            }
            playerData.SaveStats(p.stats);
            gameData.savedSlots[index].players.Add(playerData);
        }
    }

    private void SaveDroppedItems(SavedGameData gameData, int index)
    {
        var drops = GetDroppedItemsInScene();
        gameData.savedSlots[index].droppedItems = drops;
    }

    #endregion

    #region LOAD

    public void Load(int index)
    {
        if (!IsValidSlot(index)) return;
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[DataSystem] Nenhum save encontrado.");
            return;
        }

        var encrypted = File.ReadAllText(SavePath);
        var json = Decrypt(encrypted);
        var gameData = JsonUtility.FromJson<SavedGameData>(json);

        RestoreDroppedItems(gameData, index);
        RestorePlayers(gameData, index);

        Debug.Log($"[DataSystem] Jogo carregado do slot {index}.");
    }

    private void RestoreDroppedItems(SavedGameData gameData, int index)
    {
        var savedDrops = gameData.savedSlots[index].droppedItems;
        var savedIds = new HashSet<int>(savedDrops.ConvertAll(d => d.ID));

        // Remove drops que não existem no save
        foreach (var drop in FindObjectsByType<ItemDropZone>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!savedIds.Contains(drop.ID))
                Destroy(drop.gameObject);
        }

        // Recria os drops
        foreach (var savedDrop in savedDrops)
        {
            var itemData = Resources.Load<ItemData>("Items/" + savedDrop.itemName);
            if (itemData == null) continue;

            GameObject go = new("ItemDrop_" + savedDrop.itemName);
            go.transform.position = savedDrop.position;

            var dropZone = go.AddComponent<ItemDropZone>();
            dropZone.itemData = itemData;
            dropZone.quantity = savedDrop.quantity;
            dropZone.SetId(savedDrop.ID);
            dropZone.Initialize();
        }
    }

    private void RestorePlayers(SavedGameData gameData, int index)
    {
        foreach (var savedPlayer in gameData.savedSlots[index].players)
        {
            var refPlayer = players.Find(p => p.ID == savedPlayer.playerId);
            if (refPlayer == null) continue;

            // Restaurar inventário
            refPlayer.Inventario.ClearItems();
            foreach (var entry in savedPlayer.inventory)
            {
                var itemData = Resources.Load<ItemData>("Items/" + entry.itemName);
                if (itemData != null)
                {
                    refPlayer.Inventario.AddItem(itemData, entry.quantity);
                }
            }

            // Restaurar stats
            savedPlayer.LoadStats(refPlayer.stats);

            // Restaurar posição
            if (refPlayer.transform.TryGetComponent(out CharacterController controller))
            {
                controller.enabled = false;
                refPlayer.transform.position = savedPlayer.position;
                controller.enabled = true;
            }
            else
            {
                refPlayer.transform.position = savedPlayer.position;
            }

            // Restaurar vida
            refPlayer.Health = savedPlayer.health;
        }
    }

    #endregion

    #region DELETE & RESET

    public void Delete(int? slotIndex = null)
    {
    if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[DataSystem] Nenhum save encontrado para deletar.");
            return;
        }

    if (slotIndex.HasValue)
    {
        int index = slotIndex.Value;
        if (!IsValidSlot(index))
        {
            Debug.LogWarning($"[DataSystem] Slot {index} inválido para deletar.");
            return;
        }

        // Lê e desserializa
        var encrypted = File.ReadAllText(SavePath);
        var json = Decrypt(encrypted);
        var gameData = JsonUtility.FromJson<SavedGameData>(json);

        // Limpa apenas o slot específico
        gameData.savedSlots[index] = new SavedSlotData();

        // Reescreve o arquivo
        var newJson = JsonUtility.ToJson(gameData, true);
        var newEncrypted = Encrypt(newJson);
        File.WriteAllText(SavePath, newEncrypted);

        Debug.Log($"[DataSystem] Slot {index} deletado com sucesso.");
    }
    else
    {
        // Deleta o arquivo todo
        File.Delete(SavePath);
        Debug.Log("[DataSystem] Arquivo de save deletado com sucesso.");
    }

    // Reseta os dados em memória (opcional, mantém comportamento antigo)
    ResetGameData();
}

    private void ResetGameData()
    {
        foreach (var p in players)
        {
            p.Inventario.ClearItems();
            p.Health = p.MaxHealth;
            p.transform.position = Vector3.zero;
        }

        foreach (var drop in FindObjectsByType<ItemDropZone>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            Destroy(drop.gameObject);
        }

        Debug.Log("[DataSystem] Dados de jogo resetados.");
    }

    #endregion

    #region UTIL

    private bool IsValidSlot(int index) => index >= 0 && index < maxSlots;

    private List<SavedDroppedItem> GetDroppedItemsInScene()
    {
        var result = new List<SavedDroppedItem>();

        var drops = FindObjectsByType<ItemDropZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var drop in drops)
        {
            if (drop.itemData == null) continue;

            result.Add(new SavedDroppedItem
            {
                ID = drop.ID,
                itemName = drop.itemData.itemName,
                position = drop.transform.position,
                quantity = drop.quantity
            });
        }

        return result;
    }

    private string Encrypt(string data)
    {
        var dataBytes = System.Text.Encoding.UTF8.GetBytes(data);
        var keyBytes = System.Text.Encoding.UTF8.GetBytes(cryptoKey);

        for (int i = 0; i < dataBytes.Length; i++)
            dataBytes[i] ^= keyBytes[i % keyBytes.Length];

        return System.Convert.ToBase64String(dataBytes);
    }

    private string Decrypt(string encrypted)
    {
        var dataBytes = System.Convert.FromBase64String(encrypted);
        var keyBytes = System.Text.Encoding.UTF8.GetBytes(cryptoKey);

        for (int i = 0; i < dataBytes.Length; i++)
            dataBytes[i] ^= keyBytes[i % keyBytes.Length];

        return System.Text.Encoding.UTF8.GetString(dataBytes);
    }

    #endregion
}
