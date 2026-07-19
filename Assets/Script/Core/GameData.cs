using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SavedGameData
{
  public List<SavedSlotData> savedSlots = new();

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
  public bool HasSave = false;
}

[System.Serializable]
public class SavedSlotData
{
  public string lastLevelName;
  public DateTime lastLevelSaveTime;
  public bool gameCompleted;
  public List<SavedLevelData> savedLevelDatas = new();
}

[System.Serializable]
public class SavedLevelData
{
  public string levelName;
  public int levelScore;
  public LevelPathType lastPath;
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
  public int PlayerId;
  public DateTime Lastsave;
  public List<SavedItemEntry> Inventory = new();
  public int AmethystsCount;
  public Vector3 Position;
  public float Health;
  public float Time;
  public int Score;
  public int HighestComboIndex = -1;

  public List<SavedStatEntry> SavedStats = new();

  public void SaveStats(Stats stats)
  {
    SavedStats.Clear();

    // Salvar floats
    foreach (var kvp in stats.GetNumericStats())
    {
      SavedStats.Add(
        new SavedStatEntry()
        {
          statType = kvp.Key,
          type = "float",
          value = kvp.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
        }
      );
    }

    // Salvar bools
    foreach (var kvp in stats.GetBoolStats())
    {
      SavedStats.Add(
        new SavedStatEntry()
        {
          statType = kvp.Key,
          type = "bool",
          value = kvp.Value.ToString(),
        }
      );
    }
  }

  public void LoadStats(Stats stats)
  {
    foreach (var stat in SavedStats)
    {
      if (stat.type == "float")
      {
        if (
          float.TryParse(
            stat.value,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out float floatValue
          )
        )
        {
          stats.SetStat(stat.statType, floatValue);
        }
      }
      else if (stat.type == "bool")
      {
        if (bool.TryParse(stat.value, out bool boolValue))
        {
          stats.SetStat(stat.statType, boolValue);
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
  public StatType statType;
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
