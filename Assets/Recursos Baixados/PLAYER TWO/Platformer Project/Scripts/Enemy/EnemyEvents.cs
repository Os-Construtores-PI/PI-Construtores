using System;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[Serializable]
	public class EnemyEvents
	{
		/// <summary>
		/// Called when the Player enters this Enemy sight.
		/// </summary>
		public UnityEvent OnPlayerSpotted;

		/// <summary>
		/// Called when the Player leaves this Enemy sight.
		/// </summary>
		public UnityEvent OnPlayerScaped;

		/// <summary>
		/// Called when this Enemy touches a Player.
		/// </summary>
		public UnityEvent OnPlayerContact;

		/// <summary>
		/// Called when this Enemy takes damage.
		/// </summary>
		public UnityEvent OnDamage;

		/// <summary>
		/// Called when this Enemy loses all health.
		/// </summary>
		public UnityEvent OnDie;

		/// <summary>
		/// Called when the Enemy was revived.
		/// </summary>
		public UnityEvent OnRevive;

		/// <summary>
		/// Called when the Enemy respawns.
		/// </summary>
		public UnityEvent OnRespawn;
	}
}
