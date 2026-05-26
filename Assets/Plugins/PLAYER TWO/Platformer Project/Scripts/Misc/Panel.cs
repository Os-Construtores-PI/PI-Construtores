using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(AudioSource))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Panel")]
	public class Panel : MonoBehaviour, IEntityContact
	{
		public enum ActivationEntity
		{
			Player,
			Other,
			Any,
			None,
		}

		public enum ActivationCollider
		{
			Any,
			None,
		}

		[Header("Activation Settings")]
		[Tooltip("If true, the Panel will deactivate when no activator is in contact.")]
		public bool autoToggle;

		[Tooltip("The type of entity that can activate the Panel.")]
		public ActivationEntity activationEntity;

		[Tooltip("The type of collider that can activate the Panel.")]
		public ActivationCollider activationCollider;

		[Tooltip("If true, the Panel will only activate when the Player activator is stomping.")]
		public bool requireStomp;

		[Header("Audio Settings")]
		[Tooltip("The audio clip to play when the Panel is activated.")]
		public AudioClip activateClip;

		[Tooltip("The audio clip to play when the Panel is deactivated.")]
		public AudioClip deactivateClip;

		[Header("Events")]
		/// <summary>
		/// Called when the Panel is activated.
		/// </summary>
		public UnityEvent OnActivate;

		/// <summary>
		/// Called when the Panel is deactivated.
		/// </summary>
		public UnityEvent OnDeactivate;

		protected Collider m_collider;
		protected Collider m_entityActivator;
		protected Collider m_otherActivator;

		protected AudioSource m_audio;

		/// <summary>
		/// Return true if the Panel is activated.
		/// </summary>
		public bool activated { get; protected set; }

		/// <summary>
		/// Activate this Panel.
		/// </summary>
		public virtual void Activate()
		{
			if (!activated)
			{
				if (activateClip)
				{
					m_audio.PlayOneShot(activateClip);
				}

				activated = true;
				OnActivate?.Invoke();
			}
		}

		/// <summary>
		/// Deactivates this Panel.
		/// </summary>
		public virtual void Deactivate()
		{
			if (activated)
			{
				if (deactivateClip)
				{
					m_audio.PlayOneShot(deactivateClip);
				}

				activated = false;
				OnDeactivate?.Invoke();
			}
		}

		protected virtual void Start()
		{
			gameObject.tag = GameTags.Panel;
			m_collider = GetComponent<Collider>();
			m_audio = GetComponent<AudioSource>();
		}

		protected virtual void Update()
		{
			if (m_entityActivator || m_otherActivator)
			{
				var center = m_collider.bounds.center;
				var contactOffset = Physics.defaultContactOffset + 0.1f;
				var size = m_collider.bounds.size + Vector3.up * contactOffset;
				var bounds = new Bounds(center, size);

				var intersectsEntity =
					m_entityActivator && bounds.Intersects(m_entityActivator.bounds);
				var intersectsOther =
					m_otherActivator && bounds.Intersects(m_otherActivator.bounds);

				if (intersectsEntity || intersectsOther)
				{
					Activate();
				}
				else
				{
					m_entityActivator = intersectsEntity ? m_entityActivator : null;
					m_otherActivator = intersectsOther ? m_otherActivator : null;

					if (autoToggle)
					{
						Deactivate();
					}
				}
			}
		}

		public void OnEntityContact(Entity entity)
		{
			if (
				activationEntity == ActivationEntity.None
				|| entity.verticalVelocity.y > 0
				|| !BoundsHelper.IsBellowPoint(m_collider, entity.stepPosition)
			)
				return;

			switch (activationEntity)
			{
				default:
				case ActivationEntity.Any:
					m_entityActivator = entity.controller;
					break;
				case ActivationEntity.Player:
					if (
						entity is Player player
						&& (
							!requireStomp || player.states.IsCurrentOfType(typeof(StompPlayerState))
						)
					)
						m_entityActivator = entity.controller;
					break;
				case ActivationEntity.Other:
					if (entity is not Player)
						m_entityActivator = entity.controller;
					break;
			}
		}

		protected virtual void OnCollisionStay(Collision collision)
		{
			if (
				GameTags.IsEntity(collision.collider)
				|| activationCollider == ActivationCollider.None
			)
				return;

			m_otherActivator = collision.collider;
		}
	}
}
