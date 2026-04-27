using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Pauser")]
	public class LevelPauser : Singleton<LevelPauser>
	{
		/// <summary>
		/// Called when the Level is Paused.
		/// </summary>
		public UnityEvent OnPause;

		/// <summary>
		/// Called when the Level is unpaused.
		/// </summary>
		public UnityEvent OnUnpause;

		[Tooltip("The UI Container that will be shown when the Level is paused.")]
		public UIContainer pauseScreen;

		protected int m_lastToggleFrame;

		/// <summary>
		/// Returns true if it's possible to pause the Level.
		/// </summary>
		public bool canPause { get; set; }

		/// <summary>
		/// Returns true if the Level is paused.
		/// </summary>
		public bool paused { get; protected set; }

		/// <summary>
		/// Sets the pause state based on a given value.
		/// </summary>
		/// <param name="value">The state you want to set the pause to.</param>
		public virtual void Pause(bool value)
		{
			if (paused == value || m_lastToggleFrame == Time.frameCount)
				return;

			if (!paused)
				Pause();
			else
				Unpause();

			m_lastToggleFrame = Time.frameCount;
		}

		protected virtual void Pause()
		{
			if (!canPause)
				return;

			Game.LockCursor(false);
			paused = true;
			Time.timeScale = 0;

			if (pauseScreen)
			{
				pauseScreen.SetActive(true);
				pauseScreen.Show();
			}

			OnPause?.Invoke();
		}

		protected virtual void Unpause()
		{
			Game.LockCursor();
			paused = false;
			Time.timeScale = 1;

			if (pauseScreen)
				pauseScreen.Hide();

			OnUnpause?.Invoke();
		}

		/// <summary>
		/// Returns true if the Level is paused.
		/// Also considers the last toggle time to avoid frame issues.
		/// </summary>
		/// <returns>True if the Level is paused, false otherwise.</returns>
		public static bool Paused()
		{
			if (instance)
				return instance.paused || instance.m_lastToggleFrame == Time.frameCount;

			return Time.timeScale == 0;
		}
	}
}
