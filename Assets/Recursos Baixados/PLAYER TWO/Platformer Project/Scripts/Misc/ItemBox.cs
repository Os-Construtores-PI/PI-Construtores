using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(BoxCollider))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Item Box")]
	public class ItemBox : MonoBehaviour, IEntityContact, ILevelTracked
	{
		public Collectible[] collectibles;
		public MeshRenderer itemBoxRenderer;
		public Material emptyItemBoxMaterial;

		[Space(15)]
		public UnityEvent onCollect;
		public UnityEvent onDisable;

		protected int m_index;
		protected int m_checkpointIndex;
		protected bool m_enabled = true;
		protected Vector3 m_initialScale;
		protected Material m_initialMaterial;

		protected BoxCollider m_collider;

		protected virtual void InitializeCollectibles()
		{
			foreach (var collectible in collectibles)
			{
				collectible.collectOnContact = false;
				collectible.gameObject.SetActive(false);
			}
		}

		public virtual void Collect(Player player)
		{
			if (m_enabled)
			{
				if (m_index < collectibles.Length)
				{
					collectibles[m_index].gameObject.SetActive(true);

					if (collectibles[m_index].TryGetComponent(out CollectibleHidden _))
						collectibles[m_index].Collect(player);
					else
						collectibles[m_index].collectOnContact = true;

					m_index = Mathf.Clamp(m_index + 1, 0, collectibles.Length);
					onCollect?.Invoke();
				}

				if (m_index == collectibles.Length)
				{
					Disable();
				}
			}
		}

		public virtual void Disable()
		{
			if (m_enabled)
			{
				m_enabled = false;
				itemBoxRenderer.sharedMaterial = emptyItemBoxMaterial;
				onDisable?.Invoke();
			}
		}

		protected virtual void Start()
		{
			m_collider = GetComponent<BoxCollider>();
			m_initialScale = transform.localScale;
			m_initialMaterial = itemBoxRenderer.sharedMaterial;
			InitializeCollectibles();
		}

		/// <summary>
		/// Registers a callback that fires when this item box is used by the player.
		/// </summary>
		/// <param name="listener">The action to invoke on use.</param>
		public virtual void AddInteractionListener(System.Action listener) =>
			onCollect.AddListener(listener.Invoke);

		/// <summary>
		/// Captures the current collectible index so the box can be partially restored after respawn.
		/// </summary>
		public virtual void OnCheckpointActivated() => m_checkpointIndex = m_index;

		/// <summary>
		/// Restores only the collectibles dispensed after the last checkpoint, preserving earlier ones.
		/// </summary>
		public virtual void RestoreToCheckpoint()
		{
			if (m_checkpointIndex == m_index)
				return;

			m_index = m_checkpointIndex;
			m_enabled = m_index < collectibles.Length;
			itemBoxRenderer.sharedMaterial = m_enabled ? m_initialMaterial : emptyItemBoxMaterial;

			for (int i = m_index; i < collectibles.Length; i++)
			{
				collectibles[i].Restore();
				collectibles[i].collectOnContact = false;
				collectibles[i].gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Restores the Item Box to its default state, making it ready to be collected again.
		/// </summary>
		public virtual void Restore()
		{
			m_enabled = true;
			m_index = 0;
			itemBoxRenderer.sharedMaterial = m_initialMaterial;

			foreach (var collectible in collectibles)
			{
				collectible.Restore();
				collectible.collectOnContact = false;
				collectible.gameObject.SetActive(false);
			}
		}

		public void OnEntityContact(Entity entity)
		{
			if (entity is Player player)
			{
				var offset = entity.height * 0.5f - entity.radius;
				var head =
					entity.position + entity.transform.up * (offset - Physics.defaultContactOffset);

				if (entity.verticalVelocity.y > 0 && BoundsHelper.IsAbovePoint(m_collider, head))
				{
					Collect(player);
					entity.verticalVelocity = Vector3.zero;
				}
			}
		}
	}
}
