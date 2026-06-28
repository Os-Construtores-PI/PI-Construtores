using UnityEngine;
using System.Collections;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collider))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Platform/Falling Platform")]
	public class FallingPlatform : Platform, IEntityContact
	{
		[Header("General Setting")]
		[Tooltip("If true, the platform will automatically reset its position.")]
		public bool autoReset = true;

		[Tooltip("Time in seconds before the platform falls.")]
		public float fallDelay = 2f;

		[Tooltip("Time in seconds before the platform resets its position after falling.")]
		public float resetDelay = 5f;

		[Tooltip("Gravity force applied to the platform when falling.")]
		public float fallGravity = 40f;

		[Header("Shake Setting")]
		[Tooltip("If true, the platform will shake before falling.")]
		public bool shake = true;

		[Tooltip("How fast the platform will shake.")]
		public float speed = 45f;

		[Tooltip("The maximum height the platform will reach when shaking.")]
		public float height = 0.1f;

		protected Collider m_collider;
		protected Vector3 m_initialPosition;
		protected float m_timer;

		protected Collider[] m_overlaps = new Collider[32];

		/// <summary>
		/// Returns true if the fall routine was activated.
		/// </summary>
		public bool activated { get; protected set; }

		/// <summary>
		/// Returns true if this platform is falling.
		/// </summary>
		public bool falling { get; protected set; }

		/// <summary>
		/// Returns true if the platform is shaking.
		/// </summary>
		public bool shaking { get; protected set; }

		/// <summary>
		/// Make the platform fall.
		/// </summary>
		public virtual void Fall()
		{
			falling = true;
			m_collider.isTrigger = true;
			shaking = false;
			DetachAll();

			if (autoReset)
				StartCoroutine(ResetRoutine());
		}

		/// <summary>
		/// Reset the platform to its original state.
		/// </summary>
		public virtual void Restart()
		{
			m_timer = 0;
			activated = falling = false;
			transform.position = m_initialPosition;
			m_collider.isTrigger = false;
			OffsetPlayer();
		}

		public void OnEntityContact(Entity entity)
		{
			if (!activated && entity is Player && BoundsHelper
				.IsBellowPoint(m_collider, entity.stepPosition))
			{
				activated = true;
				m_timer = fallDelay;
			}
		}

		protected virtual void OffsetPlayer()
		{
			var center = m_collider.bounds.center;
			var extents = m_collider.bounds.extents;
			var maxY = m_collider.bounds.max.y;
			var overlaps = Physics.OverlapBoxNonAlloc(center, extents, m_overlaps);

			for (int i = 0; i < overlaps; i++)
			{
				if (!m_overlaps[i].CompareTag(GameTags.Player))
					continue;

				var distance = maxY - m_overlaps[i].transform.position.y;
				var height = m_overlaps[i].GetComponent<Player>().height;
				var offset = transform.up * (distance + height * 0.5f);

				m_overlaps[i].transform.position += offset;
			}
		}

		protected IEnumerator ResetRoutine()
		{
			yield return new WaitForSeconds(resetDelay);
			Restart();
		}

		protected virtual void HandleShaking()
		{
			if (!activated || falling)
				return;

			if (shake && m_timer <= fallDelay * 0.5f)
			{
				var offset = Mathf.Sin(Time.time * speed) * height;
				transform.position = m_initialPosition + transform.up * offset;
				shaking = true;
			}

			m_timer -= Time.deltaTime;

			if (m_timer <= 0)
				Fall();
		}

		protected virtual void HandleFall()
		{
			if (falling)
				transform.position += fallGravity * -transform.up * Time.deltaTime;
		}

		protected override void HandleAttachedTransforms()
		{
			if (!falling)
				base.HandleAttachedTransforms();
		}

		protected virtual void Start()
		{
			m_collider = GetComponent<Collider>();
			m_initialPosition = transform.position;
		}

		public override void PlatformUpdate()
		{
			CacheTransform();
			HandleShaking();
			HandleFall();
			HandleAttachedTransforms();
		}
	}
}
