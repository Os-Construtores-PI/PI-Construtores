using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Level/Level Rails Manager")]
	public class LevelRailsManager : Singleton<LevelRailsManager>
	{
		protected SplineContainer[] m_splineContainers;
		protected Dictionary<Collider, SplineContainer> m_colliderToSplineContainer = new();

		protected List<Collider> m_tempColliders = new();

		protected virtual void Start()
		{
			InitializeContainers();
			InitializeColliderMapping();
		}

		protected virtual void InitializeContainers()
		{
#if UNITY_6000
			m_splineContainers = FindObjectsByType<SplineContainer>(FindObjectsSortMode.None);
#else
			m_splineContainers = FindObjectsOfType<SplineContainer>(true);
#endif
		}

		protected virtual void InitializeColliderMapping()
		{
			m_colliderToSplineContainer.Clear();

			foreach (var container in m_splineContainers)
			{
				container.GetComponentsInChildren(true, m_tempColliders);

				for (int i = 0; i < m_tempColliders.Count; i++)
				{
					if (
						!m_colliderToSplineContainer.ContainsKey(m_tempColliders[i])
						&& GameTags.IsRail(m_tempColliders[i])
					)
						m_colliderToSplineContainer.Add(m_tempColliders[i], container);
				}
			}
		}

		public virtual bool TryGetSplineContainer(
			Collider collider,
			out SplineContainer container
		) => m_colliderToSplineContainer.TryGetValue(collider, out container);
	}
}
