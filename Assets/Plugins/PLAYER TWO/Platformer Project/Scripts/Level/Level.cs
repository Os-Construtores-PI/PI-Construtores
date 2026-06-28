using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level")]
	public class Level : Singleton<Level>
	{
		[Header("Level Settings")]
		[Tooltip("The sub-scenes that can be loaded from this Level.")]
		public string[] subScenes;

		[Space(10)]
		public UnityEvent<Player> onPlayerChanged;
		public UnityEvent onLevelInitialized;

		protected int m_currentSubSceneIndex = -1;

		protected Player m_player;
		protected PlayerCamera m_camera;
		protected GameLevel m_gameLevel;

		/// <summary>
		/// Returns the Player activated in the current Level.
		/// </summary>
		public Player player
		{
			get
			{
				if (!m_player)
#if UNITY_6000_0_OR_NEWER
					player = FindFirstObjectByType<Player>();
#else
					player = FindObjectOfType<Player>();
#endif

				return m_player;
			}
			set
			{
				if (m_player == value)
					return;

				m_player = value;
				onPlayerChanged.Invoke(m_player);
			}
		}

		/// <summary>
		/// Returns the Player Camera activated in the current Level.
		/// </summary>
		public new PlayerCamera camera
		{
			get
			{
				if (!m_camera)
#if UNITY_6000_0_OR_NEWER
					m_camera = FindFirstObjectByType<PlayerCamera>();
#else
					m_camera = FindObjectOfType<PlayerCamera>();
#endif

				return m_camera;
			}
		}

		/// <summary>
		/// Returns true if the Level has been finished.
		/// </summary>
		public bool isFinished { get; set; }

		/// <summary>
		/// Returns the Game Level corresponding to this Level's scene.
		/// </summary>
		public GameLevel gameLevel
		{
			get
			{
				if (m_gameLevel == null)
				{
					InitializeGame();
					InitializeLevel();
				}

				return m_gameLevel;
			}
			protected set => m_gameLevel = value;
		}

		/// <summary>
		/// Returns true if the Level has been completed at least once.
		/// </summary>
		public bool isCompleted => gameLevel?.wasCompletedOnce ?? false;

		protected Entity[] m_entities;
		protected Platform[] m_platforms;

		protected override void Awake()
		{
			base.Awake();
			InitializeGame();
			InitializeLevel();
			DontDestroyOnLoad(gameObject);
		}

		protected virtual void Start()
		{
			InitializeEntities();
			InitializePlatforms();
		}

		protected virtual void Update()
		{
			UpdateEntities();
			UpdatePlatforms();
		}

		protected virtual void InitializeGame()
		{
			if (!Game.instance.dataLoaded)
				Game.instance.LoadOrCreateState(0);
		}

		protected virtual void InitializeLevel()
		{
			m_gameLevel ??= Game.instance.GetCurrentLevel();

			if (m_gameLevel == null)
			{
				Debug.LogError(
					$"There are no Levels defined for the current scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}"
				);
				return;
			}

			onLevelInitialized?.Invoke();
		}

		protected virtual void InitializeEntities()
		{
#if UNITY_6000_0_OR_NEWER
			m_entities = FindObjectsByType<Entity>(FindObjectsSortMode.None);
#else
			m_entities = FindObjectsOfType<Entity>();
#endif

			foreach (var entity in m_entities)
				entity.manualUpdate = true;
		}

		protected virtual void InitializePlatforms()
		{
#if UNITY_6000_0_OR_NEWER
			m_platforms = FindObjectsByType<Platform>(FindObjectsSortMode.None);
#else
			m_platforms = FindObjectsOfType<Platform>();
#endif
		}

		protected virtual void UpdateEntities()
		{
			for (int i = 0; i < m_entities.Length; i++)
			{
				try
				{
					if (m_entities[i])
						m_entities[i].EntityUpdate();
				}
				catch (UnityException e)
				{
					Debug.LogError(
						$"Error updating entity {m_entities[i].name}: {e.Message}",
						m_entities[i]
					);
				}
			}
		}

		protected virtual void UpdatePlatforms()
		{
			for (int i = 0; i < m_platforms.Length; i++)
			{
				try
				{
					if (m_platforms[i])
						m_platforms[i].PlatformUpdate();
				}
				catch (UnityException e)
				{
					Debug.LogError(
						$"Error updating platform {m_platforms[i].name}: {e.Message}",
						m_platforms[i]
					);
				}
			}
		}

		/// <summary>
		/// Returns the array of tracked collectibles in the Level (collectOnce items).
		/// </summary>
		public virtual CollectibleInstance[] GetTrackedCollectibles() =>
			gameLevel.collectibles.Where((c) => c.profile.collectOnce).ToArray();

		/// <summary>
		/// Loads the finish scene of this Level.
		/// </summary>
		public virtual void FinishLevel()
		{
			gameLevel.FinishLevel();
			Destroy(gameObject);
		}

		/// <summary>
		/// Loads the exit scene of this Level.
		/// </summary>
		public virtual void ExitLevel()
		{
			gameLevel.ExitLevel();
			Destroy(gameObject);
		}

		/// <summary>
		/// Destroys the Level game object when the game is over.
		/// </summary>
		public virtual void GameOver() => Destroy(gameObject);

		/// <summary>
		/// Loads the next sub-scene in the Level.
		/// If no sub-scenes are defined, this method does nothing.
		/// </summary>
		public virtual void LoadNextSubScene()
		{
			if (subScenes == null || subScenes.Length == 0)
				return;

			m_currentSubSceneIndex = (m_currentSubSceneIndex + 1) % subScenes.Length;
			LoadSubScene(m_currentSubSceneIndex);
		}

		/// <summary>
		/// Loads the sub-scene at the given index in the Level.
		/// If the index is out of bounds, it will not load any scene.
		/// </summary>
		/// <param name="index">The index of the sub-scene to load.</param>
		public virtual void LoadSubScene(int index)
		{
			if (
				subScenes == null
				|| subScenes.Length == 0
				|| index < 0
				|| index >= subScenes.Length
			)
				return;

			string sceneToLoad = subScenes[m_currentSubSceneIndex];

			if (!string.IsNullOrEmpty(sceneToLoad))
				GameLoader.instance.Load(sceneToLoad);
		}

		/// <summary>
		/// Beats the Level with the given collectibles and time.
		/// </summary>
		/// <param name="collectibles">The collectibles collected in the Level.</param>
		/// <param name="time">The time it took to beat the Level.</param>
		public virtual void BeatLevel(CollectibleInstanceList collectibles, float time)
		{
			var coins = collectibles.SumAmount("coin");
			var stars = collectibles.SumAmount("star");

			gameLevel.beatenTimes++;
			gameLevel.collectibles.AddOrStackMany(collectibles);
			gameLevel.SetBestScore(coins, stars, time);
			Game.instance.inventory.AddMany(collectibles);
			Game.instance.RequestSaving();
		}
	}
}
