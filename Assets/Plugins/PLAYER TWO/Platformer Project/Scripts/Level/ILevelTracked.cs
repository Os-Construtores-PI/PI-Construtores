namespace PLAYERTWO.PlatformerProject
{
	/// <summary>
	/// Marks a MonoBehaviour as trackable by the <see cref="LevelCheckpoint"/> system.
	/// Implementors must be MonoBehaviours so their instance IDs can be used for protection checks.
	/// </summary>
	public interface ILevelTracked
	{
		/// <summary>
		/// Restores the object to its initial state.
		/// </summary>
		void Restore();

		/// <summary>
		/// Registers a callback that fires when this object is interacted with by the player
		/// in a way that should be tracked by the checkpoint system.
		/// </summary>
		/// <param name="listener">The action to invoke on interaction.</param>
		void AddInteractionListener(System.Action listener);

		/// <summary>
		/// Called when a checkpoint is activated. Implementors should capture any internal
		/// state needed to restore to this point if the player dies after the checkpoint.
		/// </summary>
		void OnCheckpointActivated();

		/// <summary>
		/// Restores the object to the state it was in when the last checkpoint was activated,
		/// rather than its initial state. Called for objects that were interacted with before the checkpoint.
		/// </summary>
		void RestoreToCheckpoint();
	}
}
