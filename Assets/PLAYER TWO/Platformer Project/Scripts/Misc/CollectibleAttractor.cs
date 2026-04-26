using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Collectible Attractor")]
	public class CollectibleAttractor : MonoBehaviour
	{
		[Header("Attractor Settings")]
		public float attractionRadius = 1f;
		public float attractionForce = 50f;
		public LayerMask layerMask = 1 << 0;

		protected Dictionary<Collider, Collectible> m_collectibleCache = new();
		protected List<Collectible> m_attractedCollectibles = new();
		protected Dictionary<Collectible, UnityAction<Player>> m_collectibleActions = new();

		protected Collider[] m_overlapResults = new Collider[128];

		protected virtual void Update()
		{
			HandleOverlapping();
			HandleAttraction();
		}

		protected virtual void AddCollectible(Collectible collectible)
		{
			if (m_attractedCollectibles.Contains(collectible))
				return;

			m_attractedCollectibles.Add(collectible);

			var action = new UnityAction<Player>((_) => RemoveCollectible(collectible));
			m_collectibleActions.Add(collectible, action);
			collectible.onCollect.AddListener(action);
		}

		protected virtual void RemoveCollectible(Collectible collectible)
		{
			if (!m_attractedCollectibles.Contains(collectible))
				return;

			m_attractedCollectibles.Remove(collectible);

			if (m_collectibleActions.TryGetValue(collectible, out var action))
			{
				collectible.onCollect.RemoveListener(action);
				m_collectibleActions.Remove(collectible);
			}
		}

		protected virtual bool TryGetOrCacheCollectible(
			Collider collider,
			out Collectible collectible
		)
		{
			if (m_collectibleCache.TryGetValue(collider, out collectible))
				return true;

			if (collider.TryGetComponent(out collectible))
			{
				m_collectibleCache.Add(collider, collectible);
				return true;
			}

			return false;
		}

		protected virtual void HandleOverlapping()
		{
			var results = Physics.OverlapSphereNonAlloc(
				transform.position,
				attractionRadius,
				m_overlapResults,
				layerMask
			);

			for (var i = 0; i < results; i++)
			{
				if (
					!GameTags.IsCollectible(m_overlapResults[i])
					|| Physics.Linecast(
						transform.position,
						m_overlapResults[i].transform.position,
						Physics.DefaultRaycastLayers,
						QueryTriggerInteraction.Ignore
					)
				)
					continue;

				if (TryGetOrCacheCollectible(m_overlapResults[i], out var collectible))
					AddCollectible(collectible);
			}
		}

		protected virtual void HandleAttraction()
		{
			var maxDelta = Time.deltaTime * attractionForce;
			var targetPosition = transform.position;

			for (int i = 0; i < m_attractedCollectibles.Count; i++)
			{
				if (!m_attractedCollectibles[i])
					continue;

				var position = m_attractedCollectibles[i].transform.position;
				position = Vector3.MoveTowards(position, targetPosition, maxDelta);
				m_attractedCollectibles[i].transform.position = position;
			}
		}

		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position, attractionRadius);
		}
	}
}
