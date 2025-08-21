using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class DataSystem : MonoBehaviour
{
    public List<Player> players = new(); // Lista de jogadores ativos na cena (arrastados pelo editor)
    public List<SavedDroppedItem> droppedItems = new(); // Lista de itens dropados a serem salvos

    // Caminho do arquivo de save no sistema
    private string SavePath => Application.persistentDataPath + "/GAMEDATA.json";

    // Método de salvamento geral do jogo
    public void Save()
    {
        var gameData = new SavedGameData(); // Cria o objeto de dados que será serializado em JSON

        // Salva os dados de cada jogador
        foreach (Player p in players)
        {
            SavedPlayerData playerData = new()
            {
                playerId = p.ID,
                position = p.transform.position,
                health = p.Health,
                //equippedItemName = p.EquipClassRef.currentItem != null ? p.EquipClassRef.currentItem.itemName : null
            };

            // Salva o inventário
            foreach (var item in p.Inventario.GetItems())
            {
                playerData.inventory.Add(new SavedItemEntry
                {
                    itemName = item.data.itemName,
                    quantity = item.quantity
                });
            }

            gameData.players.Add(playerData); // Adiciona o jogador ao save
        }

        // Salva os itens dropados no mundo
        droppedItems.Clear();

        // Procura todos os objetos com ItemDropZone na cena, incluindo inativos
        var drops = FindObjectsByType<ItemDropZone>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var drop in drops)
        {
            Debug.Log($" - {drop.name} at {drop.transform.position} with item: {(drop.itemData != null ? drop.itemData.itemName : null)}");

            // Salva apenas os que têm itemData válido
            if (drop.itemData != null)
            {
                droppedItems.Add(new SavedDroppedItem
                {
                    itemName = drop.itemData.itemName,
                    position = drop.transform.position,
                    quantity = drop.quantity,
                    allowedEntityTypes = drop.allowedEntityTypes
                });
            }
        }

        // Atribui ao objeto de save
        gameData.droppedItems = droppedItems;

        // Serializa para JSON e salva no disco
        var json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("Jogo salvo com múltiplos jogadores e itens dropados.");
    }

    // Método de carregamento do jogo
    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("Nenhum save encontrado.");
            return;
        }

        // Carrega e desserializa o arquivo JSON
        var json = File.ReadAllText(SavePath);
        var gameData = JsonUtility.FromJson<SavedGameData>(json);

        // Remove todos os itens dropados atuais da cena antes de recriar os salvos
        foreach (var drop in FindObjectsByType<ItemDropZone>(FindObjectsSortMode.None))
        {
            foreach (var saveDrop in gameData.droppedItems)
            {
                var subitemData = Resources.Load<ItemDataBase>("Items/" + saveDrop.itemName);
                if (drop.itemData == subitemData && drop.transform.position == saveDrop.position)
                {
                    Destroy(drop.gameObject);
                }
            }
        }

        // Recria todos os itens dropados salvos
        foreach (var savedDrop in gameData.droppedItems)
        {
            var itemData = Resources.Load<ItemDataBase>("Items/" + savedDrop.itemName);
            if (itemData != null)
            {
                GameObject go = new("ItemDrop_" + savedDrop.itemName);
                go.transform.position = savedDrop.position;

                var dropZone = go.AddComponent<ItemDropZone>();
                dropZone.itemData = itemData;
                dropZone.quantity = savedDrop.quantity;
                dropZone.allowedEntityTypes = savedDrop.allowedEntityTypes;
                dropZone.Initialize(); // Cria colisor, rigidbody e visual
            }
        }

        // Restaura os dados de cada jogador
        foreach (var savedPlayer in gameData.players)
        {
            var refPlayer = players.Find(p => p.ID == savedPlayer.playerId);
            if (refPlayer == null) continue;

            // Limpa o inventário
            refPlayer.Inventario.ClearItems();

            // Recarrega os itens
            foreach (var entry in savedPlayer.inventory)
            {
                var itemData = Resources.Load<ItemDataBase>("Items/" + entry.itemName);
                if (itemData != null)
                {
                    refPlayer.Inventario.AddItem(itemData, entry.quantity);
                }
            }

            // Move o jogador para a posição salva
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

            // Restaura a vida do jogador
            refPlayer.Health = savedPlayer.health;
        }

        Debug.Log("Jogo carregado com múltiplos jogadores.");
    }
}
