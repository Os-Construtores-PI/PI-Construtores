using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(2)]
public sealed class DataDirector : MonoBehaviour
{
  public static DataDirector Instance { get; private set; }

  [SerializeField]
  private int _maxSlots = 3;
  private int _currentSlot;

  private SavedGameData _gameData;
  private SavedConfigData _configData;

  private readonly List<Player> _players = new();
  private readonly List<ItemDropZone> _drops = new();

  private string GamePath => Constants.PersistentNames.DataPath;
  private string ConfigPath => Constants.PersistentNames.ConfigPath;

  #region UNITY
  private void Awake()
  {
    if (Instance && Instance != this)
    {
#if UNITY_EDITOR
      DestroyImmediate(gameObject);
#else
      Destroy(gameObject);
#endif
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);
    LoadFromDisk();
  }
  #endregion

  #region RAM / DISK
  private void LoadFromDisk()
  {
    if (!File.Exists(GamePath))
    {
      _gameData = NewGameData();
    }
    else
    {
      try
      {
        string enc = File.ReadAllText(GamePath);
        string json = DataCryptography.Decrypt(enc);
        _gameData = JsonUtility.FromJson<SavedGameData>(json) ?? NewGameData();
      }
      catch
      {
        _gameData = NewGameData();
      }
    }

    if (!File.Exists(ConfigPath))
    {
      _configData = NewConfigData();
    }
    else
    {
      try
      {
        string enc = File.ReadAllText(ConfigPath);
        string json = DataCryptography.Decrypt(enc);
        _configData = JsonUtility.FromJson<SavedConfigData>(json) ?? NewConfigData();
      }
      catch
      {
        _configData = NewConfigData();
      }
    }

    EnsureInvariants();
  }

  public void Commit()
  {
    EnsureInvariants();
    QualityOfLife.WriteJsonInDisk<SavedGameData>(_gameData, GamePath);
    QualityOfLife.WriteJsonInDisk<SavedConfigData>(_configData, ConfigPath);
  }

  private SavedGameData NewGameData()
  {
    SavedGameData gd = new(_maxSlots);
    EnsureInvariants(gd);
    return gd;
  }

  private SavedConfigData NewConfigData()
  {
    SavedConfigData config = new();
    return config;
  }
  #endregion

  #region INVARIANTS / NORMALIZATION
  private void EnsureInvariants() => EnsureInvariants(_gameData);

  private void EnsureInvariants(SavedGameData gd)
  {
    gd.savedSlots ??= new List<SavedSlotData>();
    while (gd.savedSlots.Count < _maxSlots)
      gd.savedSlots.Add(new SavedSlotData());

    foreach (var s in gd.savedSlots)
      s.savedLevelDatas ??= new List<SavedLevelData>();
  }

  private int NormalizeSlot(int index)
  {
    if (_maxSlots <= 0)
      return 0;
    if (index < 0)
      return 0;
    if (index >= _maxSlots)
      return _maxSlots - 1;
    return index;
  }

  private SavedSlotData GetSafeSlot(int index)
  {
    EnsureInvariants();
    return _gameData.savedSlots[NormalizeSlot(index)];
  }

  private SavedLevelData GetSafeLevel(int slotIndex, string scene)
  {
    var slot = GetSafeSlot(slotIndex);
    var lvl = slot.savedLevelDatas.Find(l => l.levelName == scene);

    if (lvl == null)
    {
      lvl = new SavedLevelData(scene);
      slot.savedLevelDatas.Add(lvl);
    }

    lvl.savedPlayers ??= new List<SavedPlayerData>();
    lvl.savedDroppedItems ??= new List<SavedDroppedItem>();
    return lvl;
  }

  private SavedLevelData FindLevel(int slotIndex, string scene)
  {
    var slot = GetSafeSlot(slotIndex);
    return slot.savedLevelDatas.Find(l => l.levelName == scene);
  }
  #endregion

  #region SCENE COLLECTION
  public void CollectScene()
  {
    _players.Clear();
    _players.AddRange(
      FindObjectsByType<Player>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
    );

    _drops.Clear();
    _drops.AddRange(
      FindObjectsByType<ItemDropZone>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
    );
  }
  #endregion

  #region COLLECTORS
  private SavedPlayerData Collect(Player p)
  {
    var d = new SavedPlayerData
    {
      Position = p.transform.position,
      Health = p.Health,
      AmethystsCount = p.Amethysts,
      Score = p.CurrentScore,
      HighestComboIndex = p.HighestComboIndex,
      Lastsave = DateTime.Now,
    };

    foreach (var it in p.Inventory.GetItems())
      d.Inventory.Add(new SavedItemEntry(it.data.itemName, it.quantity));

    return d;
  }
  #endregion

  #region SAVE (RAM)
  public void SaveCheckpoint(int slot)
  {
    CollectScene();
    SavedSlotData slotData = GetSafeSlot(slot);
    slotData.lastLevelName = SceneManager.GetActiveScene().name;
    slotData.lastLevelSaveTime = DateTime.Now;

    SavedLevelData lvl = GetSafeLevel(slot, SceneManager.GetActiveScene().name);

    lvl.savedPlayers.Clear();

    foreach (Player p in _players)
    {
      lvl.savedPlayers.Add(Collect(p));
    }

    lvl.savedDroppedItems.Clear();
    foreach (ItemDropZone d in _drops)
    {
      if (!d || !d.itemData)
        continue;
      lvl.savedDroppedItems.Add(
        new SavedDroppedItem
        {
          ID = d.ID,
          itemName = d.itemData.itemName,
          position = d.transform.position,
        }
      );
    }

    Commit();
  }

  public void SaveLastPath(int slot, LevelPathType lastPath)
  {
    SavedSlotData slotData = GetSafeSlot(slot);
    slotData.lastLevelName = SceneManager.GetActiveScene().name;
    slotData.lastLevelSaveTime = DateTime.Now;

    SavedLevelData lvl = GetSafeLevel(slot, SceneManager.GetActiveScene().name);
    lvl.lastPath = lastPath;
    Commit();
  }

  public void SaveGameMode(GameMode mode)
  {
    _configData.GameMode = mode;
    Commit();
  }

  public void SaveHasSave(bool set)
  {
    _configData.HasSave = set;
    Commit();
  }

  public void SaveLevelRecord(
    int slot,
    string scene,
    int playerIndex,
    int score,
    float time,
    int comboIndex
  )
  {
    SavedLevelData lvl = GetSafeLevel(slot, scene);

    while (lvl.savedPlayers.Count <= playerIndex)
      lvl.savedPlayers.Add(new SavedPlayerData());

    SavedPlayerData pd = lvl.savedPlayers[playerIndex];
    pd.Score = Mathf.Max(pd.Score, score);
    pd.Time = time;
    pd.HighestComboIndex = Mathf.Max(pd.HighestComboIndex, comboIndex);

    Commit();
  }

  public void SavePlayerStats(int slot, string scene, int playerIndex, int health, int amethysts)
  {
    SavedLevelData lvl = GetSafeLevel(slot, scene);

    while (lvl.savedPlayers.Count <= playerIndex)
      lvl.savedPlayers.Add(new SavedPlayerData());

    SavedPlayerData pd = lvl.savedPlayers[playerIndex];
    pd.Health = health;
    pd.AmethystsCount = amethysts;

    Commit();
  }

  #endregion


  #region RESPAWN (CHECKPOINT RUNTIME)
  public void RespawnAllPlayers(int slot)
  {
    CollectScene();

    var lvl = FindLevel(slot, SceneManager.GetActiveScene().name);
    if (lvl == null)
      return;

    int count = Mathf.Min(_players.Count, lvl.savedPlayers.Count);
    for (int i = 0; i < count; i++)
      StartCoroutine(RespawnRoutine(_players[i], lvl.savedPlayers[i]));
  }

  public void RespawnPlayer(int slot, int playerIndex)
  {
    CollectScene();

    var lvl = FindLevel(slot, SceneManager.GetActiveScene().name);
    if (lvl == null)
      return;

    if (playerIndex < 0 || playerIndex >= _players.Count || playerIndex >= lvl.savedPlayers.Count)
      return;

    StartCoroutine(RespawnRoutine(_players[playerIndex], lvl.savedPlayers[playerIndex]));
  }

  private IEnumerator RespawnRoutine(Player player, SavedPlayerData data)
  {
    yield return null;

    if (player.TryGetComponent<CharacterController>(out var cc))
      cc.enabled = false;

    var behaviours = player.GetComponents<MonoBehaviour>().Where(b => b != player).ToArray();
    foreach (var b in behaviours)
      b.enabled = false;

    player.transform.position = data.Position;
    player.Health = data.Health;
    player.SetAmethysts(data.AmethystsCount);
    player.SetScore(data.Score);
    player.SetHighestComboIndex(data.HighestComboIndex);

    player.Inventory.ClearItems();
    foreach (var it in data.Inventory)
    {
      var itemData = Resources.Load<ItemData>($"Items/{it.savedItemName}");
      if (itemData)
        player.Inventory.AddItem(itemData, it.savedItemQuantity);
    }

    if (cc)
      cc.enabled = true;
    foreach (var b in behaviours)
      b.enabled = true;
  }
  #endregion

  #region LOAD (FRIO / SCENE)
  public void LoadDroppedItems(int slot)
  {
    var lvl = FindLevel(slot, SceneManager.GetActiveScene().name);
    if (lvl == null)
      return;

    foreach (
      var d in FindObjectsByType<ItemDropZone>(
        FindObjectsInactive.Exclude,
        FindObjectsSortMode.None
      )
    )
      Destroy(d.gameObject);

    foreach (var sd in lvl.savedDroppedItems)
    {
      var data = Resources.Load<ItemData>($"Items/{sd.itemName}");
      if (!data)
        continue;

      var go = new GameObject("ItemDrop_" + sd.itemName);
      go.transform.position = sd.position;

      var dz = go.AddComponent<ItemDropZone>();
      dz.itemData = data;
      dz.SetId(sd.ID);
      dz.Initialize();
    }
  }
  #endregion

  #region READ API
  public bool HasCheckpoint(int slot, string scene) => FindLevel(slot, scene) != null;

  public IReadOnlyList<SavedPlayerData> GetPlayersData(int slot, string scene)
  {
    var lvl = FindLevel(slot, scene);
    return lvl?.savedPlayers ?? new List<SavedPlayerData>();
  }

  public int GetPlayerHighestComboIndex(int slot, string scene, int playerIndex = 0)
  {
    var lvl = FindLevel(slot, scene);
    if (lvl == null || playerIndex < 0 || playerIndex >= lvl.savedPlayers.Count)
      return 0;

    return lvl.savedPlayers[playerIndex].HighestComboIndex;
  }

  public int GetPlayerScore(int slot, string scene, int playerIndex = 0)
  {
    var lvl = FindLevel(slot, scene);
    if (lvl == null || playerIndex < 0 || playerIndex >= lvl.savedPlayers.Count)
      return 0;

    return lvl.savedPlayers[playerIndex].Score;
  }

  public GameMode GetGameMode() => _configData.GameMode;

  public SavedGameData GetGameData() => _gameData;

  public SavedConfigData GetConfigData() => _configData;

  public bool GameHasSave() => _configData.HasSave;

  public int GetMaxSlots() => _maxSlots;

  public int GetCurrentSlot() => _currentSlot;

  public LevelPathType GetLastPath(int slot, string scene)
  {
    var lvl = FindLevel(slot, scene);
    return lvl != null ? lvl.lastPath : default;
  }

  public string GetLastLevelName(int index) => GetSafeSlot(index).lastLevelName;

  #endregion

  #region WRITE API

  public void SetCurrentSlot(int index)
  {
    _currentSlot = NormalizeSlot(index);
  }
  #endregion

  public bool IsSlotCompleted(int slotIndex)
  {
    var slot = GetSafeSlot(slotIndex);
    return slot.gameCompleted;
  }

  public void SetSlotCompleted(int slotIndex, bool value)
  {
    var slot = GetSafeSlot(slotIndex);
    slot.gameCompleted = value;
    Commit();
  }

  public bool AnySlotCompleted()
  {
    foreach (var slot in _gameData.savedSlots)
    {
      if (slot.gameCompleted)
        return true;
    }
    return false;
  }

  public bool AnySlotHasCheckpoint(out SavedSlotData chosenSlot)
  {
    EnsureInvariants();

    if (_gameData?.savedSlots == null)
    {
      chosenSlot = null;
      return false;
    }

    chosenSlot = _gameData
      .savedSlots.Where(slot =>
        slot.savedLevelDatas != null && slot.savedLevelDatas.Count > 0 && !slot.gameCompleted
      )
      .OrderByDescending(slot => slot.lastLevelSaveTime)
      .FirstOrDefault();

    return chosenSlot != null;
  }

  public void ResetRunTimeState()
  {
    Time.timeScale = 1;
    GameContext.IsPaused = false;

    var huds = FindObjectsByType<HudDirector>(
      FindObjectsInactive.Include,
      FindObjectsSortMode.None
    );
    foreach (var h in huds)
      Destroy(h.gameObject);

    var cams = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    foreach (var cam in cams)
    {
      if (cam.gameObject.scene.name == "DontDestroyOnLoad")
        Destroy(cam.gameObject);
    }

    var finalDialogue = FindObjectsByType<FinalSequenceDialogue>(
      FindObjectsInactive.Include,
      FindObjectsSortMode.None
    );
    foreach (var f in finalDialogue)
      Destroy(f.gameObject);
  }

  public void ClearSlot(int slotIndex)
  {
    slotIndex = NormalizeSlot(slotIndex);
    _gameData.savedSlots[slotIndex] = new SavedSlotData();
    Commit();
  }

  public void ClearGameData()
  {
    _gameData = NewGameData();
    Commit();
  }
}
