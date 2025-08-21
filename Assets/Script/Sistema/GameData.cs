using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SavedGameData
{
    public List<SavedPlayerData> players = new();
    public List<SavedDroppedItem> droppedItems = new();
}

[System.Serializable]
public class SavedPlayerData
{
    public int playerId;
    public List<SavedItemEntry> inventory = new();
    public string equippedItemName;
    public Vector3 position;
    public float health;
}

[System.Serializable]
public class SavedItemEntry
{
    public string itemName;
    public int quantity;
}

[System.Serializable]
public class SavedDroppedItem
{
    public string itemName;
    public Vector3 position;
    public int quantity;
    public CombatEntities[] allowedEntityTypes;
}


