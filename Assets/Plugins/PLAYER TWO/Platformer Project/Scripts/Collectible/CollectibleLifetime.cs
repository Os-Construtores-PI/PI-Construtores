using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collectible))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Collectible/Collectible Lifetime")]
	public class CollectibleLifetime : MonoBehaviour
	{
		[Header("Life Time Settings")]
		[Tooltip("If true, the collectible will disappear after a certain duration.")]
		public float lifeTimeDuration = 5f;

		protected float m_elapsedLifeTime;
		protected Collectible m_collectible;

		protected virtual void Awake()
		{
			InitializeCollectible();
		}

		protected virtual void Update()
		{
			HandleLifeTime();
		}

		protected virtual void InitializeCollectible()
		{
			m_collectible = GetComponent<Collectible>();
			m_collectible.onCollect.AddListener(_ => m_elapsedLifeTime = 0f);
		}

		/// <summary>
		/// Restores the collectible to its initial spawned state.
		/// </summary>
		public virtual void Restore()
		{
			m_elapsedLifeTime = 0f;
		}

		protected virtual void HandleLifeTime()
		{
			if (!m_collectible.isVisible)
				return;

			m_elapsedLifeTime += Time.deltaTime;

			if (m_elapsedLifeTime >= lifeTimeDuration)
			{
				m_elapsedLifeTime = 0f;
				m_collectible.HideDisplay();
				m_collectible.triggerCollider.enabled = false;
			}
		}
	}
}
