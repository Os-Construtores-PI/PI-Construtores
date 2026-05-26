using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Game/Game")]
	public class Game : Singleton<Game>
	{
		/// <summary>
		/// Called when the amount of retries change.
		/// </summary>
		public UnityEvent<int> OnRetriesSet;

		/// <summary>
		/// Called when a saving has been requested.
		/// </summary>
		public UnityEvent OnSavingRequested;

		/// <summary>
		/// Called when a Game Data has been loaded.
		/// </summary>
		public UnityEvent<int> onLoadState;

		[Tooltip("Amount of stars existing in each level.")]
		public int starsPerLevel = 3;

		[Tooltip("Amount of retries the player has when starting a new game.")]
		public int initialRetries = 3;

		[Tooltip("Name of the scene to load when the player exits any level.")]
		public string levelExitScene;

		[Tooltip("The list of collectible profiles available in the game.")]
		public List<CollectibleProfile> collectibles;

		[Tooltip("The list of playable levels.")]
		public List<GameLevel> levels;

		protected int m_retries;
		protected GameInventory m_inventory;
		protected DateTime m_createdAt;
		protected DateTime m_updatedAt;

		/// <summary>
		/// The index of the current loaded Game Data.
		/// If it's -1, it means no data is loaded.
		/// </summary>
		public int dataIndex { get; protected set; } = -1;

		/// <summary>
		/// The amount of Level retries.
		/// </summary>
		public int retries
		{
			get { return m_retries; }
			set
			{
				m_retries = value;
				OnRetriesSet?.Invoke(m_retries);
			}
		}

		/// <summary>
		/// Returns the Game Inventory.
		/// </summary>
		public GameInventory inventory
		{
			get
			{
				m_inventory ??= new GameInventory();
				return m_inventory;
			}
		}

		public bool dataLoaded => dataIndex >= 0;

		public static GameData CurrentData
		{
			get
			{
				if (instance.dataIndex >= 0)
					return GameSaver.instance.Load(instance.dataIndex);

				return null;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			DontDestroyOnLoad(gameObject);
			InitializeRetries();
			InitializeFrameRate();
		}

		protected virtual void InitializeRetries()
		{
			retries = initialRetries;
		}

		protected virtual void InitializeFrameRate()
		{
#if UNITY_ANDROID || UNITY_IOS
			Application.targetFrameRate = 60;
#endif
		}

		/// <summary>
		/// Sets the cursor lock and hide state.
		/// </summary>
		/// <param name="value">If true, the cursor will be hidden.</param>
		public static void LockCursor(bool value = true)
		{
#if UNITY_STANDALONE || UNITY_WEBGL
			Cursor.visible = !value;
			Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
#endif
		}

		/// <summary>
		/// Loads or create a Game Data from a given index.
		/// </summary>
		/// <param name="index">The index of the Game Data.</param>
		public virtual void LoadOrCreateState(int index)
		{
			var data = GameSaver.instance.Load(index);
			data ??= GameData.Create();
			LoadState(index, data);
		}

		/// <summary>
		/// Loads this Game state from a given Game Data.
		/// </summary>
		/// <param name="index">The index of the Game Data.</param>
		/// <param name="data">The Game Data to read the state from.</param>
		public virtual void LoadState(int index, GameData data)
		{
			dataIndex = index;
			m_retries = data.retries;
			m_inventory = new(data.inventory.ToArray());
			m_createdAt = DateTime.Parse(data.createdAt);
			m_updatedAt = DateTime.Parse(data.updatedAt);

			for (int i = 0; i < data.levels.Length; i++)
			{
				if (i >= levels.Count)
					break;

				levels[i].LoadState(data.levels[i]);
			}

			onLoadState.Invoke(index);
		}

		/// <summary>
		/// Returns the Game Level array as Level Data.
		/// </summary>
		public virtual LevelData[] LevelsData()
		{
			return levels.Select(level => level.ToData()).ToArray();
		}

		/// <summary>
		/// Returns a Game Level if the current scene is a Level. If its not, return null.
		/// </summary>
		public virtual GameLevel GetCurrentLevel()
		{
			var scene = GameLoader.instance.currentScene;
			return levels.Find((level) => level.scene == scene);
		}

		/// <summary>
		/// Returns the index from the levels list of the current scene.
		/// </summary>
		/// <returns></returns>
		public virtual int GetCurrentLevelIndex()
		{
			var scene = GameLoader.instance.currentScene;
			return levels.FindIndex((level) => level.scene == scene);
		}

		/// <summary>
		/// Returns the amount of collected stars from all levels.
		/// </summary>
		public virtual int GetTotalStars() =>
			levels.Aggregate(0, (acc, level) => acc + level.bestScore.stars);

		/// <summary>
		/// Save the Game Data to its current index.
		/// </summary>
		public virtual void RequestSaving()
		{
			GameSaver.instance.Save(ToData(), dataIndex);
			OnSavingRequested?.Invoke();
		}

		/// <summary>
		/// Unlocks a given Game Level by its scene name if it's not locked by stars counter.
		/// </summary>
		/// <param name="sceneName">The scene name of the level you want to unlock.</param>
		public virtual void UnlockLevelBySceneName(string sceneName)
		{
			var level = levels.Find((level) => level.scene == sceneName);

			if (level != null && level.requiredStars <= 0)
			{
				level.locked = false;
			}
		}

		/// <summary>
		/// Unlocks the next level from the levels list if it's not locked by stars counter.
		/// </summary>
		public virtual void UnlockNextLevel()
		{
			var index = GetCurrentLevelIndex() + 1;

			if (index >= 0 && index < levels.Count)
			{
				if (levels[index].requiredStars > 0)
					return;

				levels[index].locked = false;
			}
		}

		/// <summary>
		/// Resets the retries counter to its initial value.
		/// </summary>
		public virtual void ResetRetries() => m_retries = initialRetries;

		/// <summary>
		/// Returns true if the player has beaten at least one level.
		/// </summary>
		public virtual bool HasAnyBeatenLevel() => levels.Any((level) => level.beatenTimes > 0);

		/// <summary>
		/// Returns the Game Data of this Game to be used by the Data Layer.
		/// </summary>
		public virtual GameData ToData()
		{
			return new GameData()
			{
				retries = m_retries,
				levels = LevelsData(),
				inventory = inventory.ToData(),
				createdAt = m_createdAt.ToString(),
				updatedAt = DateTime.UtcNow.ToString(),
			};
		}
	}
}
