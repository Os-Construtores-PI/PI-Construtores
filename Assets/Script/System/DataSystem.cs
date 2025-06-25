using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class DataSystem : MonoBehaviour
{
    // Representa a referência de cada jogador no jogo
    [System.Serializable]
    public class PlayerReference
    {
        public string playerId;                            // ID único do jogador
        public InventoryComponent inventory;               // Inventário do jogador
        public EquipamentComponent equipment;              // Equipamento do jogador
        public Transform transform;                        // Posição no mundo
        public HealthComponent health;                     // Vida do jogador
    }

    public List<PlayerReference> players = new(); // Lista de jogadores ativos na cena (arrastados pelo editor)
    public List<SavedDroppedItem> droppedItems = new(); // Lista de itens dropados a serem salvos

    // Caminho do arquivo de save no sistema
    private string SavePath => Application.persistentDataPath + "/GAMEDATA.json";

    // Método de salvamento geral do jogo
    public void Save()
    {
        var gameData = new SavedGameData(); // Cria o objeto de dados que será serializado em JSON

        // Salva os dados de cada jogador
        foreach (var p in players)
        {
            var playerData = new SavedPlayerData
            {
                playerId = p.playerId,
                position = p.transform.position,
                health = p.health.GetAttribute<float>("health"),
                equippedItemName = p.equipment.currentItem != null ? p.equipment.currentItem.itemName : null
            };

            // Salva o inventário
            foreach (var item in p.inventory.GetItems())
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
            Destroy(drop.gameObject);
        }

        // Recria todos os itens dropados salvos
        foreach (var savedDrop in gameData.droppedItems)
        {
            var itemData = Resources.Load<ItemData>("Items/" + savedDrop.itemName);
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
            var refPlayer = players.Find(p => p.playerId == savedPlayer.playerId);
            if (refPlayer == null) continue;

            // Limpa o inventário
            refPlayer.inventory.ClearItems();

            // Recarrega os itens
            foreach (var entry in savedPlayer.inventory)
            {
                var itemData = Resources.Load<ItemData>("Items/" + entry.itemName);
                if (itemData != null)
                {
                    refPlayer.inventory.AddItem(itemData, entry.quantity);
                }
            }

            // Reequipa o item salvo
            if (!string.IsNullOrEmpty(savedPlayer.equippedItemName))
            {
                var equipped = Resources.Load<ItemData>("Items/" + savedPlayer.equippedItemName);
                if (equipped != null)
                {
                    refPlayer.equipment.Equip(equipped);
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
            refPlayer.health.SetAttribute("health", savedPlayer.health);
        }

        Debug.Log("Jogo carregado com múltiplos jogadores.");
    }
}
