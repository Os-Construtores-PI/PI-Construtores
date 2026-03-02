using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SavedGameData
{
    public List<SavedSlotData> savedSlots = new();
    public SavedConfigData savedConfig = new();
    public SavedGameData(int maxSlots = 3)
    {
        for (int i = 0; i < maxSlots; i++)
        {
            savedSlots.Add(new SavedSlotData());
        }
    }
}


[System.Serializable]
public class SavedConfigData
{
    public GameMode GameMode = GameMode.SINGLEPLAYER;
}




[System.Serializable]
public class SavedSlotData
{
    public string lastLevelName;
    public bool gameCompleted;
    public List<SavedLevelData> savedLevelDatas = new();
}

[System.Serializable]
public class SavedLevelData
{
    public string levelName;
    public int levelScore;
    public List<SavedPlayerData> savedPlayers = new();
    public List<SavedDroppedItem> savedDroppedItems = new();
    public SavedLevelData(string levelname)
    {
        levelName = levelname;
    }
}

[System.Serializable]
public class SavedPlayerData
{
    public int playerId;
    public List<SavedItemEntry> inventory = new();
    public int amethystsCount;
    public Vector3 position;
    public float health;

    public List<SavedStatEntry> savedStats = new();
    public void SaveStats(Stats stats)
    {
        savedStats.Clear();

        // Salvar floats
        foreach (var kvp in stats.GetNumericStats())
        {
            savedStats.Add(new SavedStatEntry()
            {
                name = kvp.Key,
                type = "float",
                value = kvp.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
            });
        }

        // Salvar bools
        foreach (var kvp in stats.GetBoolStats())
        {
            savedStats.Add(new SavedStatEntry()
            {
                name = kvp.Key,
                type = "bool",
                value = kvp.Value.ToString()
            });
        }
    }

    public void LoadStats(Stats stats)
    {
        foreach (var stat in savedStats)
        {
            if (stat.type == "float")
            {
                if (float.TryParse(stat.value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float floatValue))
                {
                    stats.SetStat(stat.name, floatValue);
                }
            }
            else if (stat.type == "bool")
            {
                if (bool.TryParse(stat.value, out bool boolValue))
                {
                    stats.SetStat(stat.name, boolValue);
                }
            }
        }
    }
}

[System.Serializable]
public class SavedItemEntry
{
    public string savedItemName;
    public int savedItemQuantity;
    public SavedItemEntry(string name, int quantity)
    {
        savedItemName = name;
        savedItemQuantity = quantity;
    }
}

[System.Serializable]
public class SavedStatEntry
{
    public string name;
    public string type; // "float" ou "bool"
    public string value; // usamos string pra serializar genérico
}

[System.Serializable]
public class SavedDroppedItem
{
    public int ID;
    public string itemName;
    public Vector3 position;
    public int quantity;
    public List<CombatEntities> allowedEntityTypes;
}


