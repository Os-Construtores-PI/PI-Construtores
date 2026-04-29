using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Finisher")]
	public class LevelFinisher : Singleton<LevelFinisher>
	{
		/// <summary>
		/// Called when the Level has been finished.
		/// </summary>
		public UnityEvent OnFinish;

		/// <summary>
		/// Called when the Level has exited.
		/// </summary>
		public UnityEvent OnExit;

		[Tooltip("The delay in seconds applied when exiting or finishing the level.")]
		public float loadingDelay = 1f;

		protected Game m_game => Game.instance;
		protected Level m_level => Level.instance;
		protected LevelScore m_score => LevelScore.instance;
		protected LevelPauser m_pauser => LevelPauser.instance;
		protected GameLoader m_loader => GameLoader.instance;
		protected Fader m_fader => Fader.instance;

		protected virtual IEnumerator FinishRoutine()
		{
			m_pauser.Pause(false);
			m_pauser.canPause = false;
			m_score.stopTime = true;
			m_level.isFinished = true;
			m_level.player.canTakeDamage = false;
			m_level.player.inputs.enabled = false;

			yield return new WaitForSeconds(loadingDelay);

			Game.LockCursor(false);
			m_score.Consolidate();
			m_level.FinishLevel();
			OnFinish?.Invoke();
		}

		protected virtual IEnumerator ExitRoutine()
		{
			m_pauser.Pause(false);
			m_pauser.canPause = false;
			m_level.player.inputs.enabled = false;
			yield return new WaitForSeconds(loadingDelay);
			Game.LockCursor(false);
			m_level.ExitLevel();
			OnExit?.Invoke();
		}

		/// <summary>
		/// Invokes the Level finishing routine to consolidate the score and load the next scene.
		/// </summary>
		public virtual void Finish()
		{
			StopAllCoroutines();
			StartCoroutine(FinishRoutine());
		}

		/// <summary>
		/// Invokes the Level exit routine, Level Score is not saved.
		/// </summary>
		public virtual void Exit()
		{
			StopAllCoroutines();
			StartCoroutine(ExitRoutine());
		}
	}
}
