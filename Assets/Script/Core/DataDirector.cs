using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
  private readonly List<Player> _players = new();
  private readonly List<ItemDropZone> _drops = new();

  private string SavePath => Constants.PersistentNames.DataPath;

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
    if (!File.Exists(SavePath))
    {
      _gameData = NewGameData();
      return;
    }

    try
    {
      var enc = File.ReadAllText(SavePath);
      var json = DataCryptography.Decrypt(enc);
      _gameData = JsonUtility.FromJson<SavedGameData>(json) ?? NewGameData();
    }
    catch
    {
      _gameData = NewGameData();
    }

    EnsureInvariants();
  }

  public void Commit()
  {
    EnsureInvariants();
    var json = JsonUtility.ToJson(_gameData, true);
    File.WriteAllText(SavePath, DataCryptography.Encrypt(json));
  }

  private SavedGameData NewGameData()
  {
    var gd = new SavedGameData(_maxSlots);
    EnsureInvariants(gd);
    return gd;
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

    gd.savedConfig ??= new SavedConfigData();
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

  private void EnsurePlayerIndex(List<SavedPlayerData> list, int idx)
  {
    if (idx < 0)
      return;
    while (list.Count <= idx)
      list.Add(new SavedPlayerData());
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
      position = p.transform.position,
      health = p.Health,
      amethystsCount = p.Amethysts,
    };

    foreach (var it in p.Inventory.GetItems())
      d.inventory.Add(new SavedItemEntry(it.data.itemName, it.quantity));

    return d;
  }
  #endregion

  #region SAVE (RAM)
  public void SaveCheckpoint(int slot)
  {
    CollectScene();
    var slotData = GetSafeSlot(slot);
    slotData.lastLevelName = SceneManager.GetActiveScene().name;

    var lvl = GetSafeLevel(slot, SceneManager.GetActiveScene().name);

    lvl.savedPlayers.Clear();
    foreach (var p in _players)
      lvl.savedPlayers.Add(Collect(p));

    lvl.savedDroppedItems.Clear();
    foreach (var d in _drops)
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
  #endregion

  #region RESPAWN (CHECKPOINT RUNTIME)
  public void RespawnAllPlayers(int slot)
  {
    CollectScene();
    var lvl = GetSafeLevel(slot, SceneManager.GetActiveScene().name);

    int count = Mathf.Min(_players.Count, lvl.savedPlayers.Count);
    for (int i = 0; i < count; i++)
      StartCoroutine(RespawnRoutine(_players[i], lvl.savedPlayers[i]));
  }

  public void RespawnPlayer(int slot, int playerIndex)
  {
    CollectScene();
    var lvl = GetSafeLevel(slot, SceneManager.GetActiveScene().name);

    if (playerIndex < 0 || playerIndex >= _players.Count || playerIndex >= lvl.savedPlayers.Count)
      return;

    StartCoroutine(RespawnRoutine(_players[playerIndex], lvl.savedPlayers[playerIndex]));
  }

  private IEnumerator RespawnRoutine(Player player, SavedPlayerData data)
  {
    yield return null;

    if (player.TryGetComponent<CharacterController>(out var cc))
      cc.enabled = false;

    var behaviours = player.GetComponents<MonoBehaviour>();
    foreach (var b in behaviours)
      b.enabled = false;

    // estado
    player.transform.position = data.position;
    player.Health = data.health;
    player.SetAmethysts(data.amethystsCount, null);

    // inventário
    player.Inventory.ClearItems();
    foreach (var it in data.inventory)
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
    var lvl = GetSafeLevel(slot, SceneManager.GetActiveScene().name);

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
  public bool HasCheckpoint(int slot, string scene) =>
    GetSafeSlot(slot).savedLevelDatas.Exists(l => l.levelName == scene);

  public IReadOnlyList<SavedPlayerData> GetPlayersData(int slot, string scene) =>
    GetSafeLevel(slot, scene).savedPlayers;

  public GameMode GetGameMode() => _gameData.savedConfig.GameMode;

  public SavedGameData GetGameData() => _gameData;

  public int GetMaxSlots() => _maxSlots;

  public int GetCurrentSlot() => _currentSlot;

  public string GetLastLevelName(int index) => GetSafeSlot(index).lastLevelName;
  #endregion

  #region  WRITE API
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

  public void SetSlotsCompleted(int slotIndex, bool value)
  {
    var slot = GetSafeSlot(slotIndex);
    slot.gameCompleted = value;
    Commit();
  }

  public bool AnySlotCompleted()
  {
    foreach(var slot in _gameData.savedSlots)
    {
      if(slot.gameCompleted)
      return true;
    }
    return false;
  }

  public bool AnySlotHasCheckpoint()
  {
    foreach (var slot in _gameData.savedSlots)
    {
      if(slot.savedLevelDatas != null && slot.savedLevelDatas.Count > 0)
        return true;
    }

    return false;
  }

  public void ResetRunTimeState()
  {
    //_currentSlot = -1;

    Time.timeScale = -1;
    GameContext.IsPaused = false;

    var huds = FindObjectsByType<HudDirector>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    foreach (var h in huds)
        Destroy(h.gameObject);
    
    var cams = FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    foreach (var cam in cams)
    {
      if (cam.gameObject.scene.name == "DontDestroyOnLoad")
          Destroy(cam.gameObject);
    }

    var finalDialogue = FindObjectsByType<FinalSequenceDialogue>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    foreach(var f in finalDialogue)
       Destroy(f.gameObject);
  }

  public void ClearSlot(int slotIndex)
  {
    slotIndex = NormalizeSlot(slotIndex);

    _gameData.savedSlots[slotIndex] = new SavedSlotData();
    Commit();
  }
}
