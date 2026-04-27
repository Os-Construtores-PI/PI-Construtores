namespace PLAYERTWO.PlatformerProject
{
	public abstract class PlayerState : EntityState<Player>
	{
		/// <summary>
		/// Called when the player comes into contact with an enemy.
		/// </summary>
		/// <param name="player">A reference to the player.</param>
		/// <param name="enemy">A reference to the enemy.</param>
		public virtual void OnEnemyContact(Player player, Enemy enemy) { }
	}
}
