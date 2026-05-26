using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Starter")]
	public class LevelStarter : Singleton<LevelStarter>
	{
		/// <summary>
		/// Called when the starter routine has ended.
		/// </summary>
		public UnityEvent OnStart;

		public float enablePlayerDelay = 1f;

		protected Level m_level => Level.instance;
		protected LevelScore m_score => LevelScore.instance;
		protected LevelPauser m_pauser => LevelPauser.instance;

		protected Fader m_fader => Fader.instance;

		protected virtual IEnumerator Routine(Player player)
		{
			Game.LockCursor();
			m_level.player.controller.enabled = false;
			m_level.player.inputs.enabled = false;
			yield return new WaitForSeconds(enablePlayerDelay);
			m_score.stopTime = false;
			m_level.player.controller.enabled = true;
			m_level.player.inputs.enabled = true;
			m_pauser.canPause = true;
			OnStart?.Invoke();
		}

		protected override void Awake()
		{
			base.Awake();
			m_level.onPlayerChanged.AddListener((player) => StartCoroutine(Routine(player)));
		}
	}
}
