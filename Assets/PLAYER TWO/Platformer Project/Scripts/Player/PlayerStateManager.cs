using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player State Manager")]
	public class PlayerStateManager : EntityStateManager<Player>
	{
		[ClassTypeName(typeof(PlayerState))]
		public string[] states;

		/// <summary>
		/// Called when the player comes into contact with an enemy.
		/// </summary>
		/// <param name="enemy">The enemy that the player collided with.</param>
		public virtual void OnEnemyContact(Enemy enemy)
		{
			(current as PlayerState)?.OnEnemyContact(entity, enemy);
		}

		protected override List<EntityState<Player>> GetStateList()
		{
			return PlayerState.CreateListFromStringArray(states);
		}
	}
}
