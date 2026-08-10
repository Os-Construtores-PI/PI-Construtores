using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Level Pause")]
	public class PlayerLevelPause : MonoBehaviour
	{
		protected Player m_player;
		protected LevelPauser m_pauser;

		protected virtual void Start()
		{
			m_player = GetComponent<Player>();
			m_pauser = LevelPauser.instance;
		}

		protected virtual void Update()
		{
			var pausing = m_player.inputs.GetPauseDown() || m_player.inputs.EscPressed();

			if (pausing)
			{
				var value = m_pauser.paused;
				m_pauser.Pause(!value);
			}
		}
	}
}
