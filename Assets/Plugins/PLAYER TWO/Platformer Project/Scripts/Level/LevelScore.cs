using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Score")]
	public class LevelScore : Singleton<LevelScore>
	{
		/// <summary>
		/// Called when the collectibles list is updated with a new collectible item.
		/// </summary>
		public UnityEvent<Collectible> OnCollectibleAdded;

		/// <summary>
		/// Called when the amount of a specific collectible item is set.
		/// </summary>
		public UnityEvent<string> OnCollectibleSet;

		/// <summary>
		/// Called after the level data is fully loaded.
		/// </summary>
		public UnityEvent OnScoreLoaded;

		/// <summary>
		/// Returns the time since the current Level started.
		/// </summary>
		public float time { get; protected set; }

		/// <summary>
		/// Returns true if the time counter should be updating.
		/// </summary>
		public bool stopTime { get; set; } = true;

		protected CollectibleInstanceList m_collectibles;

		/// <summary>
		/// Returns the list of collected collectible items on the current Level.
		/// </summary>
		public CollectibleInstanceList collectibles
		{
			get
			{
				if (m_collectibles == null)
				{
					m_collectibles = new();
					if (m_level)
						foreach (var instance in m_level.GetTrackedCollectibles())
							m_collectibles.Add(instance.Clone());
				}
				return m_collectibles;
			}
		}

		protected Level m_level => Level.instance;

		/// <summary>
		/// Resets the Level Score to its default values.
		/// </summary>
		public virtual void ResetScore()
		{
			time = 0;
		}

		/// <summary>
		/// Collect a given collectible item.
		/// </summary>
		/// <param name="collectible">The collectible item to collect.</param>
		public virtual void Collect(Collectible collectible)
		{
			if (!collectible)
				return;

			collectibles.AddOrStack(collectible);
			OnCollectibleAdded?.Invoke(collectible);
		}

		/// <summary>
		/// Resets all collected items this session by clearing the collectibles list.
		/// The list will be lazily re-initialized from save data on next access, so items
		/// collected in previous sessions are preserved while session items are cleared.
		/// Fires <see cref="OnCollectibleSet"/> for each affected reference.
		/// </summary>
		public virtual void ResetCollectibles()
		{
			if (m_collectibles == null)
				return;

			var references = new HashSet<string>();
			foreach (var instance in m_collectibles)
				references.Add(instance.reference);

			m_collectibles = null;

			foreach (var reference in references)
				OnCollectibleSet?.Invoke(reference);
		}

		/// <summary>
		/// Restores the collectibles list from a snapshot, such as one saved at a checkpoint.
		/// Fires <see cref="OnCollectibleSet"/> for every reference affected by the change.
		/// </summary>
		/// <param name="snapshot">The collectible snapshot to restore from.</param>
		public virtual void RestoreFromSnapshot(CollectibleInstanceList snapshot)
		{
			var allReferences = new HashSet<string>();
			if (m_collectibles != null)
				foreach (var instance in m_collectibles)
					allReferences.Add(instance.reference);

			m_collectibles = snapshot?.Clone() ?? new();

			foreach (var instance in m_collectibles)
				allReferences.Add(instance.reference);

			foreach (var reference in allReferences)
				OnCollectibleSet?.Invoke(reference);
		}

		/// <summary>
		/// Sends the current score to the Game Level to persist the data.
		/// </summary>
		public virtual void Consolidate()
		{
			if (!m_level)
				return;

			m_level.BeatLevel(collectibles, time);
		}

		protected virtual void Start()
		{
			OnScoreLoaded.Invoke();
		}

		protected virtual void Update()
		{
			if (!stopTime)
			{
				time += Time.deltaTime;
			}
		}
	}
}
