

using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataSystem : MonoBehaviour
{
    [System.Serializable]
    public class PlayerReference
    {
        public string playerId;
        public InventoryComponent inventory;
        public EquipamentComponent equipment;
        public Transform transform;
        public HealthComponent health;
    }

    public List<PlayerReference> players = new(); // Arraste Player1 e Player2 aqui
    public List<SavedDroppedItem> droppedItems = new();

    private string SavePath => Application.persistentDataPath + "/GAMEDATA.json";

    public void Save()
    {
        var gameData = new SavedGameData();

        foreach (var p in players)
        {
            var playerData = new SavedPlayerData
            {
                playerId = p.playerId,
                position = p.transform.position,
                health = p.health.GetAttribute<float>("health"),
                equippedItemName = p.equipment.currentItem != null ? p.equipment.currentItem.itemName : null
            };
            foreach (var item in p.inventory.GetItems())
            {
                playerData.inventory.Add(new SavedItemEntry
                {
                    itemName = item.data.itemName,
                    quantity = item.quantity
                });
            }

            gameData.players.Add(playerData);
        }
        // Salva os itens dropados no mundo
        droppedItems.Clear();

        // Procure todos os objetos com ItemDropZone ativos na cena
        var drops = FindObjectsByType<ItemDropZone>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        foreach (var drop in drops)
        {
            Debug.Log($" - {drop.name} at {drop.transform.position} with item: {(drop.itemData != null ? drop.itemData.itemName : null)}");
            if (drop.itemData != null)
            {
                droppedItems.Add(new SavedDroppedItem
                {
                    itemName = drop.itemData.itemName,
                    position = drop.transform.position,
                    quantity = drop.quantity
                });
            }
        }

        gameData.droppedItems = droppedItems;

        var json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Jogo salvo com múltiplos jogadores e itens dropados.");

    }

    public void Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("Nenhum save encontrado.");
            return;
        }

        var json = File.ReadAllText(SavePath);
        var gameData = JsonUtility.FromJson<SavedGameData>(json);

        // Limpa itens dropados atuais
        foreach (var drop in FindObjectsByType<ItemDropZone>(FindObjectsSortMode.None))
        {
            Destroy(drop.gameObject);
        }

        // Recria os itens dropados salvos na cena
        foreach (var savedDrop in gameData.droppedItems)
        {
            var itemData = Resources.Load<ItemData>("Items/" + savedDrop.itemName);
            if (itemData != null)
            {
                GameObject go = new GameObject("ItemDrop_" + savedDrop.itemName);
                go.transform.position = savedDrop.position;
                var dropZone = go.AddComponent<ItemDropZone>();
                dropZone.itemData = itemData;
                dropZone.quantity = savedDrop.quantity;
                dropZone.Initialize();
            }
        }
        foreach (var savedPlayer in gameData.players)
        {
            var refPlayer = players.Find(p => p.playerId == savedPlayer.playerId);
            if (refPlayer == null) continue;

            refPlayer.inventory.ClearItems();

            foreach (var entry in savedPlayer.inventory)
            {
                var itemData = Resources.Load<ItemData>("Items/" + entry.itemName);
                if (itemData != null)
                {
                    refPlayer.inventory.AddItem(itemData, entry.quantity);
                }
            }

            if (!string.IsNullOrEmpty(savedPlayer.equippedItemName))
            {
                var equipped = Resources.Load<ItemData>("Items/" + savedPlayer.equippedItemName);
                if (equipped != null)
                {
                    refPlayer.equipment.Equip(equipped);
                }
            }

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
            refPlayer.health.SetAttribute("health", savedPlayer.health);
        }

        Debug.Log("Jogo carregado com múltiplos jogadores.");
    }
}
