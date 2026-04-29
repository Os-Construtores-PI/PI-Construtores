using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collectible))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Collectible/Collectible Physics")]
	public class CollectiblePhysics : MonoBehaviour
	{
		[Header("Physics Settings")]
		[Tooltip("Radius of the collision sphere used for physics.")]
		public float collisionRadius = 0.5f;

		[Tooltip("Gravity applied to the collectible.")]
		public float gravity = 15f;

		[Tooltip("Bounciness factor applied on collision.")]
		public float bounciness = 0.98f;

		[Tooltip("Maximum vertical velocity after a bounce.")]
		public float maxBounceYVelocity = 10f;

		[Tooltip("Initial velocity applied to the collectible when spawned.")]
		public Vector3 initialVelocity = new(0, 12, 0);

		[Tooltip("Audio clip played on collision.")]
		public AudioClip collisionAudio;

		[Header("Ghosting Settings")]
		[Tooltip("If true, the collectible will pass through the player.")]
		public bool useGhosting = true;

		[Tooltip(
			"Duration of the ghosting effect in seconds. After this time, "
				+ "the collectible will collide with the player."
		)]
		public float ghostingDuration = 0.5f;

		protected float m_ghostingTimer;
		protected Vector3 m_velocity;
		protected Vector3 m_initialPosition;
		protected Quaternion m_initialRotation;

		protected AudioSource m_audio;
		protected Collectible m_collectible;
		protected static Transform m_container;

		/// <summary>
		/// The player currently in the scene.
		/// </summary>
		public Player currentPlayer
		{
			get
			{
				if (Level.instance && Level.instance.player)
					return Level.instance.player;

				return null;
			}
		}

		/// <summary>
		/// The container transform for all collectibles in the scene.
		/// </summary>
		public static Transform container
		{
			get
			{
				if (!m_container)
				{
					var go = GameObject.Find(k_containerName);

					if (go)
						m_container = go.transform;

					if (!m_container)
						m_container = new GameObject(k_containerName).transform;
				}

				return m_container;
			}
		}

		protected const string k_containerName = "__COLLECTIBLES_CONTAINER__";

		protected const int k_verticalMinRotation = 0;
		protected const int k_verticalMaxRotation = 30;
		protected const int k_horizontalMinRotation = 0;
		protected const int k_horizontalMaxRotation = 360;

		protected virtual void Awake()
		{
			InitializeCollectible();
			InitializeAudio();
		}

		protected virtual void OnEnable()
		{
			InitializeTransform();

			if (Level.instance)
				InitializeVelocity();
			else
				Level.instance.onPlayerChanged.AddListener(_ => InitializeVelocity());
		}

		protected virtual void Update()
		{
			if (!m_collectible.isVisible)
				return;

			HandleGhosting();
			HandleMovement();
		}

		protected virtual void InitializeCollectible()
		{
			m_collectible = GetComponent<Collectible>();
			m_collectible.canCollect = !useGhosting;
			m_collectible.onCollect.AddListener(_ =>
			{
				enabled = false;
				m_ghostingTimer = 0f;
			});
		}

		protected virtual void InitializeAudio()
		{
			if (!TryGetComponent(out m_audio))
			{
				m_audio = gameObject.AddComponent<AudioSource>();
			}
		}

		protected virtual void InitializeTransform()
		{
			var initialRotation = transform.rotation;

			if (
				transform.parent
				&& transform.parent.TryGetComponent<GravityHandler>(out var handler)
			)
			{
				var upDirection = -handler.gravityDirection;
				initialRotation = Quaternion.FromToRotation(transform.up, upDirection);
				initialRotation *= transform.rotation;
			}

			transform.parent = container;
			transform.rotation = initialRotation;
			m_initialPosition = transform.position;
			m_initialRotation = transform.rotation;
		}

		/// <summary>
		/// Restores the collectible physics to its initial spawned state.
		/// </summary>
		public virtual void Restore()
		{
			transform.SetPositionAndRotation(m_initialPosition, m_initialRotation);
			m_velocity = Vector3.zero;
			m_ghostingTimer = 0f;
			m_collectible.canCollect = !useGhosting;
			enabled = true;
			InitializeVelocity();
		}

		protected virtual void InitializeVelocity()
		{
			var direction = initialVelocity.normalized;
			var force = initialVelocity.magnitude;

			if (currentPlayer && currentPlayer.IsSideScroller)
			{
				var upward = currentPlayer.transform.up;
				var pathForward = currentPlayer.pathForward;
				var right = Vector3.Cross(upward, pathForward);
				var randomAngle = Random.Range(k_verticalMinRotation, k_verticalMaxRotation);
				direction = Quaternion.AngleAxis(randomAngle, right) * upward;
			}
			else
			{
				var randomZ = Random.Range(k_verticalMinRotation, k_verticalMaxRotation);
				var randomY = Random.Range(k_horizontalMinRotation, k_horizontalMaxRotation);
				direction = Quaternion.Euler(0, 0, randomZ) * direction;
				direction = Quaternion.Euler(0, randomY, 0) * direction;
				direction = transform.rotation * direction;
			}

			m_velocity = direction * force;
		}

		protected virtual bool SweepTest(out RaycastHit hit)
		{
			var direction = m_velocity.normalized;
			var magnitude = m_velocity.magnitude;
			var distance = magnitude * Time.deltaTime;

			return Physics.SphereCast(
				transform.position,
				collisionRadius,
				direction,
				out hit,
				distance,
				Physics.DefaultRaycastLayers,
				QueryTriggerInteraction.Ignore
			);
		}

		protected virtual void HandleGhosting()
		{
			if (!useGhosting)
				return;

			if (m_ghostingTimer < ghostingDuration)
			{
				m_ghostingTimer += Time.deltaTime;
				m_collectible.canCollect = false;
			}
			else
			{
				m_collectible.canCollect = true;
			}
		}

		protected virtual void HandleMovement()
		{
			HandleGravity();

			if (SweepTest(out var hit))
			{
				if (currentPlayer.IsSideScroller)
					HandleBounceSideScroller(
						m_velocity.normalized,
						m_velocity.magnitude,
						hit.normal
					);
				else
					HandleBounce(m_velocity.normalized, m_velocity.magnitude, hit.normal);

				PlayCollisionSound();
			}

			transform.position += m_velocity * Time.deltaTime;
		}

		protected virtual void HandleGravity()
		{
			m_velocity -= gravity * Time.deltaTime * transform.up;
		}

		protected virtual void HandleBounce(Vector3 direction, float magnitude, Vector3 normal)
		{
			var bounceDirection = Vector3.Reflect(direction, normal);
			m_velocity = bounciness * magnitude * bounceDirection;
			var currentYBounce = Vector3.Dot(transform.up, m_velocity);
			m_velocity -= transform.up * currentYBounce;
			m_velocity += transform.up * Mathf.Min(currentYBounce, maxBounceYVelocity);
		}

		protected virtual void HandleBounceSideScroller(
			Vector3 direction,
			float magnitude,
			Vector3 normal
		)
		{
			var pathForward = currentPlayer.pathForward;
			var right = Vector3.Cross(transform.up, pathForward);
			var bounceDirection = Vector3.Reflect(direction, normal);
			bounceDirection -= right * Vector3.Dot(right, bounceDirection);
			bounceDirection.Normalize();
			m_velocity = bounciness * magnitude * bounceDirection;
		}

		protected virtual void PlayCollisionSound()
		{
			if (collisionAudio && m_audio)
			{
				m_audio.PlayOneShot(collisionAudio);
			}
		}

		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position, collisionRadius);
		}
	}
}
