

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

    private string SavePath => Application.persistentDataPath + "/save.json";

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

        var json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Jogo salvo com múltiplos jogadores.");
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
