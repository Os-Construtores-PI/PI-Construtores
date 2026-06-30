using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/UI/HUD")]
	public class HUD : MonoBehaviour
	{
		[Header("Format Settings")]
		[Tooltip("The format to display the retries counter.")]
		public string retriesFormat = "00";

		[Tooltip("The format to display the health counter.")]
		public string healthFormat = "0";

		[Header("UI Elements")]
		[Tooltip("The text to display the retries counter.")]
		public TMP_Text retries;

		[Tooltip("The text to display the health counter.")]
		public TMP_Text health;

		[Tooltip("The text to display the timer.")]
		public TMP_Text timer;

		[Header("Timer Settings")]
		[Tooltip("The separator between the minutes and the seconds.")]
		public string minutesSeparator = "'";

		[Tooltip("The separator between the seconds and the milliseconds.")]
		public string secondsSeparator = "\"";

		protected Game m_game;
		protected LevelScore m_score;

		protected float timerStep;
		protected static float timerRefreshRate = .1f;

		protected Player m_player => Level.instance.player;

		protected virtual void Awake()
		{
			m_game = Game.instance;
			m_score = LevelScore.instance;
			m_score.OnScoreLoaded.AddListener(() =>
			{
				m_game.OnRetriesSet.AddListener(UpdateRetries);

				if (m_player != null)
					m_player.health.onChange.AddListener(UpdateHealth);

				Refresh();
			});

			Level.instance.onPlayerChanged.AddListener(
				(player) =>
				{
					player.health.onChange.AddListener(UpdateHealth);
					Refresh();
				}
			);
		}

		protected virtual void Start()
		{
			StartCoroutine(TimerRoutine());
		}

		/// <summary>
		/// Set the retries counter to a given value.
		/// </summary>
		protected virtual void UpdateRetries(int value)
		{
			retries.text = value.ToString(retriesFormat);
		}

		/// <summary>
		/// Called when the Player Health changed.
		/// </summary>
		protected virtual void UpdateHealth()
		{
			if (!m_player)
				return;

			health.text = m_player.health.current.ToString(healthFormat);
		}

		/// <summary>
		/// Set the timer text to the Level Score time.
		/// </summary>
		protected virtual void UpdateTimer()
		{
			timer.text = GameLevel.FormattedTime(m_score.time, minutesSeparator, secondsSeparator);
		}

		protected IEnumerator TimerRoutine()
		{
			while (true)
			{
				UpdateTimer();

				yield return new WaitForSeconds(timerRefreshRate);
			}
		}

		/// <summary>
		/// Called to force an updated on the HUD.
		/// </summary>
		public virtual void Refresh()
		{
			UpdateRetries(m_game.retries);
			UpdateHealth();
		}
	}
}
