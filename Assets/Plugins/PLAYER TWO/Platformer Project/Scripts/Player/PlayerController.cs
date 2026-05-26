using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Controller")]
	public class PlayerController : MonoBehaviour
	{
		/// <summary>
		/// Increases the amount of Player Health by 1.
		/// </summary>
		/// <param name="player">The Player instance.</param>
		public void AddHealth(Player player) => AddHealth(player, 1);

		/// <summary>
		/// Increases the amount of Player Health by a given amount.
		/// </summary>
		/// <param name="player">The Player instance.</param>
		/// <param name="amount">The amount of health.</param>
		public void AddHealth(Player player, int amount) => player.health.Increase(amount);
	}
}
