using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Checkpoint")]
	public class LevelCheckpoint : Singleton<LevelCheckpoint>
	{
		[Header("Events")]
		/// <summary>
		/// Called when any checkpoint is activated.
		/// </summary>
		public UnityEvent OnCheckpointActivated;

		protected List<ILevelTracked> m_trackedObjects;

		protected HashSet<int> m_trackedInstanceIDs = new();
		protected HashSet<int> m_snapshotInstanceIDs;
		protected CollectibleInstanceList m_snapshotCollectibles;

		protected LevelScore m_score => LevelScore.instance;

		/// <summary>
		/// Returns true if at least one checkpoint has been activated this session.
		/// </summary>
		public bool hasActiveCheckpoint => m_snapshotInstanceIDs != null;

		/// <summary>
		/// Returns the collectibles list snapshot taken at the last activated checkpoint.
		/// </summary>
		public CollectibleInstanceList snapshotCollectibles => m_snapshotCollectibles;

		/// <summary>
		/// Activates a checkpoint: saves the player's respawn transform and snapshots
		/// the current collected and item box state so it can be preserved after a respawn.
		/// </summary>
		/// <param name="player">The player to update the respawn for.</param>
		/// <param name="respawn">The transform to use as the respawn point.</param>
		public virtual void Activate(Player player, Transform respawn)
		{
			player.SetRespawn(respawn.position, respawn.rotation);
			m_snapshotInstanceIDs = new HashSet<int>(m_trackedInstanceIDs);
			m_snapshotCollectibles = m_score.collectibles.Clone();

			foreach (var tracked in m_trackedObjects)
				tracked.OnCheckpointActivated();

			OnCheckpointActivated?.Invoke();
		}

		/// <summary>
		/// Returns true if the object with the given instance ID was interacted with
		/// at or before the last activated checkpoint, meaning it should not be restored on respawn.
		/// </summary>
		/// <param name="instanceID">The Unity instance ID of the object to check.</param>
		public virtual bool IsProtected(int instanceID) =>
			hasActiveCheckpoint && m_snapshotInstanceIDs.Contains(instanceID);

		/// <summary>
		/// Resets the score and restores all tracked objects based on checkpoint state.
		/// </summary>
		public virtual void RestoreAll()
		{
			ResetScore();
			RestoreTrackedObjects();

			m_trackedInstanceIDs = hasActiveCheckpoint
				? new HashSet<int>(m_snapshotInstanceIDs)
				: new HashSet<int>();
		}

		/// <summary>
		/// Restores the score to the checkpoint snapshot, or clears it fully if no checkpoint is active.
		/// </summary>
		public virtual void ResetScore()
		{
			if (hasActiveCheckpoint)
				m_score.RestoreFromSnapshot(m_snapshotCollectibles);
			else
				m_score.ResetCollectibles();
		}

		/// <summary>
		/// Restores all tracked objects that were not protected by the active checkpoint.
		/// </summary>
		public virtual void RestoreTrackedObjects()
		{
			foreach (var tracked in m_trackedObjects)
			{
				var mb = (MonoBehaviour)tracked;
				if (!mb)
					continue;

				if (IsProtected(mb.GetInstanceID()))
					tracked.RestoreToCheckpoint();
				else
					tracked.Restore();
			}
		}

		protected virtual void Start()
		{
			InitializeTrackedObjects();
		}

		/// <summary>
		/// Adds an instance ID to the tracking list if it is not already present.
		/// </summary>
		/// <param name="instanceID">The Unity instance ID of the object to track.</param>
		protected virtual void AddToTrackingList(int instanceID)
		{
			if (!m_trackedInstanceIDs.Contains(instanceID))
				m_trackedInstanceIDs.Add(instanceID);
		}

		/// <summary>
		/// Registers a tracked object, subscribing to its interaction event for checkpoint tracking.
		/// </summary>
		/// <param name="tracked">The object to register.</param>
		protected virtual void RegisterTracked(ILevelTracked tracked)
		{
			m_trackedObjects.Add(tracked);
			var mb = (MonoBehaviour)tracked;
			tracked.AddInteractionListener(() => AddToTrackingList(mb.GetInstanceID()));
		}

		protected virtual void InitializeTrackedObjects()
		{
			m_trackedObjects = new List<ILevelTracked>();

#if UNITY_6000_0_OR_NEWER
			foreach (var collectible in FindObjectsByType<Collectible>(FindObjectsSortMode.None))
				RegisterTracked(collectible);

			foreach (var itemBox in FindObjectsByType<ItemBox>(FindObjectsSortMode.None))
				RegisterTracked(itemBox);

			foreach (var breakable in FindObjectsByType<Breakable>(FindObjectsSortMode.None))
				RegisterTracked(breakable);

			foreach (var pickable in FindObjectsByType<Pickable>(FindObjectsSortMode.None))
				RegisterTracked(pickable);
#else
			foreach (var collectible in FindObjectsOfType<Collectible>())
				RegisterTracked(collectible);

			foreach (var itemBox in FindObjectsOfType<ItemBox>())
				RegisterTracked(itemBox);

			foreach (var breakable in FindObjectsOfType<Breakable>())
				RegisterTracked(breakable);

			foreach (var pickable in FindObjectsOfType<Pickable>())
				RegisterTracked(pickable);
#endif
		}
	}
}
