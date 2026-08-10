using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Respawner")]
	public class LevelRespawner : Singleton<LevelRespawner>
	{
		public enum GameOverBehavior
		{
			ReloadCurrentScene,
			LoadScene,
			EnableGameObject,
		}

		[Header("Respawn Settings")]
		[Tooltip("The delay in seconds before the fade out effect starts when respawning.")]
		public float respawnFadeOutDelay = 1f;

		[Tooltip("The delay in seconds before the fade in effect starts when respawning.")]
		public float respawnFadeInDelay = 0.5f;

		[Header("Game Over Settings")]
		[Tooltip(
			"If true, the game over routine will be called when the retries counter reaches zero."
		)]
		public bool zeroRetriesToGameOver = true;

		[Tooltip("The delay in seconds before the game over behavior starts.")]
		public float gameOverDelay = 5f;

		[Tooltip("The behavior to execute when the game is over.")]
		public GameOverBehavior gameOverBehavior = GameOverBehavior.ReloadCurrentScene;

		[Tooltip(
			"The name of the scene to load when the game is over and the behavior is set to LoadScene."
		)]
		public string gameOverSceneName;

		[Tooltip(
			"The GameObject to enable when the game is over and the behavior is set to EnableGameObject."
		)]
		public GameObject gameOverGameObject;

		[Header("Restart Settings")]
		[Tooltip("The delay in seconds before the fade out effect starts when restarting.")]
		public float restartFadeOutDelay = 0.5f;

		[Header("Events")]
		/// <summary>
		/// Called after the Respawn routine ended.
		/// </summary>
		public UnityEvent OnRespawn;

		/// <summary>
		/// Called after the Game Over routine ended.
		/// </summary>
		public UnityEvent OnGameOver;

		protected WaitForSeconds m_respawnFadeOutDelay;
		protected WaitForSeconds m_respawnFadeInDelay;
		protected WaitForSeconds m_gameOverDelay;
		protected WaitForSeconds m_restartFadeOutDelay;
		protected List<PlayerCamera> m_cameras;

		protected Level m_level => Level.instance;
		protected LevelScore m_score => LevelScore.instance;
		protected LevelPauser m_pauser => LevelPauser.instance;
		protected Game m_game => Game.instance;
		protected Fader m_fader => Fader.instance;

		protected override void Awake()
		{
			base.Awake();
			InitializeCallbacks();
		}

		protected virtual void Start()
		{
			InitializePlayerCamera();
			InitializeYieldInstructions();
		}

		protected virtual void InitializePlayerCamera()
		{
			m_cameras = new List<PlayerCamera>(
#if UNITY_6000_0_OR_NEWER
				FindObjectsByType<PlayerCamera>(FindObjectsSortMode.None)
#else
				FindObjectsOfType<PlayerCamera>()
#endif
			);
		}

		protected virtual void InitializeCallbacks()
		{
			m_level.onPlayerChanged.AddListener(
				(player) => player.playerEvents.OnDie.AddListener(() => Respawn(true))
			);
		}

		protected virtual void InitializeYieldInstructions()
		{
			m_respawnFadeOutDelay = new WaitForSeconds(respawnFadeOutDelay);
			m_respawnFadeInDelay = new WaitForSeconds(respawnFadeInDelay);
			m_gameOverDelay = new WaitForSeconds(gameOverDelay);
			m_restartFadeOutDelay = new WaitForSeconds(restartFadeOutDelay);
		}

		protected virtual IEnumerator RespawnRoutine(bool consumeRetries)
		{
			if (consumeRetries)
			{
				m_game.retries--;
			}

			m_level.player.Respawn();
			FreezeCameras();
			ResetCameras();
			FreezeCameras(false);
			OnRespawn?.Invoke();

			yield return m_respawnFadeInDelay;

			m_fader.FadeIn(() =>
			{
				m_pauser.canPause = true;
				m_level.player.inputs.enabled = true;
			});
		}

		protected virtual IEnumerator GameOverRoutine()
		{
			m_score.stopTime = true;
			yield return m_gameOverDelay;
			HandleGameOver();
		}

		protected virtual IEnumerator RestartRoutine()
		{
			m_pauser.Pause(false);
			m_pauser.canPause = false;
			m_level.player.inputs.enabled = false;
			yield return m_restartFadeOutDelay;
			GameLoader.instance.Reload();
		}

		protected virtual IEnumerator Routine(bool consumeRetries)
		{
			m_pauser.Pause(false);
			m_pauser.canPause = false;
			m_level.player.inputs.enabled = false;
			FreezeCameras();

			var nextRetry = m_game.retries - 1;
			var canRetry = nextRetry > 0 || nextRetry == 0 && zeroRetriesToGameOver;

			if (consumeRetries && !canRetry)
			{
				StartCoroutine(GameOverRoutine());
				yield break;
			}

			yield return m_respawnFadeOutDelay;

			m_fader.FadeOut(() =>
			{
				if (LevelCheckpoint.instance)
					LevelCheckpoint.instance.RestoreAll();
				StartCoroutine(RespawnRoutine(consumeRetries));
			});
		}

		protected virtual void HandleGameOver()
		{
			m_game.ResetRetries();
			m_level.GameOver();
			Game.LockCursor(false);

			switch (gameOverBehavior)
			{
				default:
				case GameOverBehavior.ReloadCurrentScene:
					GameLoader.instance.Reload();
					break;
				case GameOverBehavior.LoadScene:
					GameLoader.instance.Load(gameOverSceneName);
					break;
				case GameOverBehavior.EnableGameObject:
					gameOverGameObject.SetActive(true);
					break;
			}

			OnGameOver?.Invoke();
		}

		protected virtual void ResetCameras()
		{
			foreach (var camera in m_cameras)
			{
				if (camera)
					camera.Reset();
			}
		}

		protected virtual void FreezeCameras(bool value = true)
		{
			foreach (var camera in m_cameras)
			{
				if (camera)
					camera.freeze = value;
			}
		}

		/// <summary>
		/// Invokes either Respawn or Game Over routine depending of the retries available.
		/// </summary>
		/// <param name="consumeRetries">If true, reduces the retries counter by one or call the game over routine.</param>
		public virtual void Respawn(bool consumeRetries)
		{
			StopAllCoroutines();
			StartCoroutine(Routine(consumeRetries));
		}

		/// <summary>
		/// Restarts the current Level loading the scene again.
		/// </summary>
		public virtual void Restart()
		{
			StopAllCoroutines();
			StartCoroutine(RestartRoutine());
		}
	}
}
