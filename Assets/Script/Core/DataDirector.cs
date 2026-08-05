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
  private static string ActiveSceneName => SceneManager.GetActiveScene().name;

  public bool ShowStageIntro = true;

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

  public void RestartCurrentLevel()
  {
    SavedSlotData slot = GetSafeSlot(_currentSlot);

    string scene = ActiveSceneName;

    // Remove COMPLETAMENTE os dados da fase atual

    slot.savedLevelDatas.RemoveAll(level => level.levelName == scene);

    // Atualiza o último nível salve
    slot.lastLevelName = scene;

    Commit();
  }

  #region RAM / DISK
  private void LoadFromDisk()
  {
    _gameData = LoadOrCreate(GamePath, () => new SavedGameData(_maxSlots));
    _configData = LoadOrCreate(ConfigPath, () => new SavedConfigData());
    EnsureInvariants();
  }

  // Unifica o carregamento (decrypt + parse + fallback) usado tanto para
  // SavedGameData quanto para SavedConfigData, eliminando os dois blocos
  // try/catch quase idênticos que existiam antes.
  private static T LoadOrCreate<T>(string path, Func<T> factory)
    where T : class
  {
    if (!File.Exists(path))
      return factory();

    try
    {
      string enc = File.ReadAllText(path);
      string json = DataCryptography.Decrypt(enc);
      return JsonUtility.FromJson<T>(json) ?? factory();
    }
    catch
    {
      return factory();
    }
  }

  public void Commit()
  {
    EnsureInvariants();
    QualityOfLife.WriteJsonInDisk<SavedGameData>(_gameData, GamePath);
    QualityOfLife.WriteJsonInDisk<SavedConfigData>(_configData, ConfigPath);
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

  // Garante que exista um SavedPlayerData no índice pedido (crescendo a
  // lista se preciso) e o devolve. Usado por SaveLevelRecord e
  // SavePlayerStats, que antes repetiam esse mesmo while().
  private static SavedPlayerData EnsurePlayerSlot(SavedLevelData lvl, int playerIndex)
  {
    while (lvl.savedPlayers.Count <= playerIndex)
      lvl.savedPlayers.Add(new SavedPlayerData());
    return lvl.savedPlayers[playerIndex];
  }

  // "Toca" o slot (atualiza cena/hora do último save) e devolve o
  // SavedLevelData da cena ativa. SaveCheckpoint e SaveLastPath faziam
  // essas mesmas três linhas cada um.
  private SavedLevelData TouchSlotAndGetLevel(int slotIndex)
  {
    SavedSlotData slotData = GetSafeSlot(slotIndex);
    string scene = ActiveSceneName;
    slotData.lastLevelName = scene;
    slotData.lastLevelSaveTime = DateTime.Now;
    return GetSafeLevel(slotIndex, scene);
  }
  #endregion

  #region SCENE COLLECTION
  private static List<T> FindAll<T>(FindObjectsInactive inactive = FindObjectsInactive.Exclude)
    where T : UnityEngine.Object =>
    FindObjectsByType<T>(inactive, FindObjectsSortMode.None).ToList();

  public void CollectScene()
  {
    _players.Clear();
    _players.AddRange(FindAll<Player>());

    _drops.Clear();
    _drops.AddRange(FindAll<ItemDropZone>());
  }
  #endregion

  #region COLLECTORS
  private void CollectInto(Player p, SavedPlayerData d)
  {
    d.Position = p.transform.position;
    d.Health = p.Health;
    d.AmethystsCount = p.Amethysts;
    d.Score = p.CurrentScore;
    d.HighestComboIndex = p.HighestComboIndex;
    d.Lastsave = DateTime.Now;

    d.Inventory.Clear();
    foreach (var it in p.Inventory.GetItems())
      d.Inventory.Add(new SavedItemEntry(it.data.itemName, it.quantity));
  }
  #endregion

  #region SAVE (RAM)
  public void SaveCheckpoint(int slot)
  {
    CollectScene();
    SavedLevelData lvl = TouchSlotAndGetLevel(slot);

    for (int i = 0; i < _players.Count; i++)
    {
      SavedPlayerData pd = EnsurePlayerSlot(lvl, i);
      CollectInto(_players[i], pd);
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
    SavedLevelData lvl = TouchSlotAndGetLevel(slot);
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

  public void SavePreview(int slot, string scene, int playerIndex, int previewScore)
  {
    SavedLevelData lvl = GetSafeLevel(slot, scene);
    SavedPlayerData pd = EnsurePlayerSlot(lvl, playerIndex);
    pd.PreviewScore = previewScore;
    Commit();
  }

  public string SaveLevelRecord(
    int slot,
    string scene,
    int playerIndex,
    int score,
    float time,
    int comboIndex
  )
  {
    var lvl = GetSafeLevel(slot, scene);
    var finish = new SavedLevelFinish(playerIndex, score, time, comboIndex);
    lvl.savedFinishes.Add(finish);
    Commit();
    return finish.FinishUUID;
  }

  public void SavePlayerStats(int slot, string scene, int playerIndex, int health, int amethysts)
  {
    SavedLevelData lvl = GetSafeLevel(slot, scene);
    SavedPlayerData pd = EnsurePlayerSlot(lvl, playerIndex);

    pd.Health = health;
    pd.AmethystsCount = amethysts;

    Commit();
  }
  #endregion

  #region RESPAWN (CHECKPOINT RUNTIME)

  private SavedLevelData PrepareRespawn(int slot)
  {
    CollectScene();
    return FindLevel(slot, ActiveSceneName);
  }

  public void RespawnAllPlayers(int slot)
  {
    var lvl = PrepareRespawn(slot);
    if (lvl == null)
      return;

    int count = Mathf.Min(_players.Count, lvl.savedPlayers.Count);
    for (int i = 0; i < count; i++)
      StartCoroutine(RespawnRoutine(_players[i], lvl.savedPlayers[i]));
  }

  public void RespawnPlayer(int slot, int playerIndex)
  {
    var lvl = PrepareRespawn(slot);
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
    var lvl = FindLevel(slot, ActiveSceneName);
    if (lvl == null)
      return;

    foreach (var d in FindAll<ItemDropZone>())
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

  private TValue GetPlayerField<TValue>(
    int slot,
    string scene,
    int playerIndex,
    Func<SavedPlayerData, TValue> selector,
    TValue defaultValue = default
  )
  {
    var lvl = FindLevel(slot, scene);
    if (lvl == null || playerIndex < 0 || playerIndex >= lvl.savedPlayers.Count)
      return defaultValue;

    return selector(lvl.savedPlayers[playerIndex]);
  }

  public IReadOnlyList<SavedLevelFinish> GetLevelFinishes(int slot, string scene)
  {
    var lvl = FindLevel(slot, scene);
    return lvl?.savedFinishes ?? new List<SavedLevelFinish>();
  }

  public string GetLastFinishUUID(int slot, string scene, int playerIndex)
  {
    var lvl = FindLevel(slot, scene);
    return lvl
      ?.savedFinishes.Where(f => f.PlayerIndex == playerIndex)
      .OrderByDescending(f => f.When)
      .FirstOrDefault()
      ?.FinishUUID;
  }

  public int GetPlayerHighestComboIndex(int slot, string scene, int playerIndex = 0) =>
    GetPlayerField(slot, scene, playerIndex, p => p.HighestComboIndex);

  public int GetPlayerScore(int slot, string scene, int playerIndex = 0) =>
    GetPlayerField(slot, scene, playerIndex, p => p.Score);

  public int GetPlayerPreviewScore(int slot, string scene, int playerIndex = 0) =>
    GetPlayerField(slot, scene, playerIndex, p => p.PreviewScore);

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

    foreach (var h in FindAll<HudDirector>(FindObjectsInactive.Include))
      Destroy(h.gameObject);

    foreach (var cam in FindAll<Camera>(FindObjectsInactive.Include))
    {
      if (cam.gameObject.scene.name == "DontDestroyOnLoad")
        Destroy(cam.gameObject);
    }

    foreach (var f in FindAll<FinalSequenceDialogue>(FindObjectsInactive.Include))
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
    _gameData = new SavedGameData(_maxSlots);
    Commit();
  }
}
