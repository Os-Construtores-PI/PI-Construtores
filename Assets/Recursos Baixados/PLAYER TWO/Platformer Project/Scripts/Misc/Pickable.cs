using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(Rigidbody))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Pickable")]
	public class Pickable : MonoBehaviour, IEntityContact, ILevelTracked
	{
		[Header("General Settings")]
		public Vector3 offset;
		public float releaseOffset = 0.5f;

		[Header("Respawn Settings")]
		public bool autoRespawn;
		public bool respawnOnHitHazards;
		public float respawnHeightLimit = -100;

		[Header("Attack Settings")]
		public bool attackEnemies = true;
		public int damage = 1;
		public float minDamageSpeed = 5f;

		[Space(15)]
		/// <summary>
		/// Called when this object is Picked.
		/// </summary>
		public UnityEvent onPicked;

		/// <summary>
		/// Called when this object is Released.
		/// </summary>
		public UnityEvent onReleased;

		/// <summary>
		/// Called when this object is respawned.
		/// </summary>
		public UnityEvent onRespawn;

		protected Collider m_collider;
		protected Rigidbody m_rigidBody;
		protected PlayerObjectGrabber m_currentGrabber;

		protected Vector3 m_initialPosition;
		protected Quaternion m_initialRotation;
		protected Transform m_initialParent;

		protected bool m_checkpointBeingHeld;
		protected PlayerObjectGrabber m_checkpointGrabber;
		protected Vector3 m_checkpointPosition;
		protected Quaternion m_checkpointRotation;
		protected Transform m_checkpointParent;

		protected RigidbodyInterpolation m_interpolation;

		public bool beingHold { get; protected set; }

		/// <summary>
		/// Picks up this object, attaching it to the grabber's slot.
		/// </summary>
		/// <param name="grabber">The grabber picking up this object.</param>
		public virtual void PickUp(PlayerObjectGrabber grabber)
		{
			if (!beingHold)
			{
				m_currentGrabber = grabber;
				beingHold = true;
				transform.parent = grabber.grabberSlot;
				transform.localPosition = Vector3.zero + offset;
				transform.localRotation = Quaternion.identity;
				m_rigidBody.isKinematic = true;
				m_collider.isTrigger = true;
				m_interpolation = m_rigidBody.interpolation;
				m_rigidBody.interpolation = RigidbodyInterpolation.None;
				onPicked?.Invoke();
			}
		}

		/// <summary>
		/// Releases this object from the grabber applying a directional force.
		/// </summary>
		public virtual void Release(Vector3 direction, float force)
		{
			if (beingHold)
			{
				m_currentGrabber = null;
				transform.parent = m_initialParent;
				transform.position += direction * (releaseOffset + 0.01f);
				m_collider.isTrigger = m_rigidBody.isKinematic = beingHold = false;
				m_rigidBody.interpolation = m_interpolation;
#if UNITY_6000_0_OR_NEWER
				m_rigidBody.linearVelocity = direction * force;
#else
				m_rigidBody.velocity = direction * force;
#endif
				onReleased?.Invoke();
			}
		}

		/// <summary>
		/// Resets this object to its initial position and rotation.
		/// </summary>
		public virtual void Respawn()
		{
			m_rigidBody.isKinematic = m_collider.isTrigger = beingHold = false;
#if UNITY_6000_0_OR_NEWER
			m_rigidBody.linearVelocity = Vector3.zero;
#else
			m_rigidBody.velocity = Vector3.zero;
#endif
			transform.parent = m_initialParent;
			transform.SetLocalPositionAndRotation(m_initialPosition, m_initialRotation);
			onRespawn?.Invoke();
		}

		public void OnEntityContact(Entity entity)
		{
			if (
				attackEnemies
				&& entity is Enemy
#if UNITY_6000_0_OR_NEWER
				&& m_rigidBody.linearVelocity.magnitude > minDamageSpeed
#else
				&& m_rigidBody.velocity.magnitude > minDamageSpeed
#endif
			)
			{
				entity.ApplyDamage(damage, transform.position);
			}
		}

		protected virtual void ReleaseFromGrabber()
		{
			if (m_currentGrabber)
			{
				m_currentGrabber.Release();
				m_currentGrabber = null;
			}
		}

		protected virtual void EvaluateHazardRespawn(Collider other)
		{
			if (autoRespawn && respawnOnHitHazards && other.CompareTag(GameTags.Hazard))
			{
				ReleaseFromGrabber();
				Respawn();
			}
		}

		protected virtual void Start()
		{
			m_collider = GetComponent<Collider>();
			m_rigidBody = GetComponent<Rigidbody>();
			m_initialPosition = transform.localPosition;
			m_initialRotation = transform.localRotation;
			m_initialParent = transform.parent;
		}

		protected virtual void Update()
		{
			if (autoRespawn && transform.position.y <= respawnHeightLimit)
			{
				ReleaseFromGrabber();
				Respawn();
			}
		}

		/// <summary>
		/// Restores the pickable to its initial position and rotation.
		/// </summary>
		public virtual void Restore() => Respawn();

		/// <summary>
		/// Registers a callback that fires when this pickable is picked up by the player.
		/// </summary>
		public virtual void AddInteractionListener(System.Action listener) =>
			onPicked.AddListener(() => listener());

		/// <summary>
		/// Snapshots whether the pickable was being held and by which grabber,
		/// as well as its current position, rotation, and parent.
		/// </summary>
		public virtual void OnCheckpointActivated()
		{
			m_checkpointBeingHeld = beingHold;
			m_checkpointGrabber = beingHold ? m_currentGrabber : null;
			m_checkpointPosition = transform.localPosition;
			m_checkpointRotation = transform.localRotation;
			m_checkpointParent = transform.parent;
		}

		/// <summary>
		/// Restores the pickable to the grabber's hands if it was held at the checkpoint,
		/// otherwise restores it to the position, rotation, and parent it had at the checkpoint.
		/// </summary>
		public virtual void RestoreToCheckpoint()
		{
			if (m_checkpointBeingHeld && m_checkpointGrabber)
			{
				m_checkpointGrabber.RestoreGrab(this);
				return;
			}

			m_rigidBody.isKinematic = m_collider.isTrigger = beingHold = false;
#if UNITY_6000_0_OR_NEWER
			m_rigidBody.linearVelocity = Vector3.zero;
#else
			m_rigidBody.velocity = Vector3.zero;
#endif
			transform.parent = m_checkpointParent;
			transform.SetLocalPositionAndRotation(m_checkpointPosition, m_checkpointRotation);
		}

		protected virtual void OnTriggerEnter(Collider other) => EvaluateHazardRespawn(other);

		protected virtual void OnCollisionEnter(Collision collision) =>
			EvaluateHazardRespawn(collision.collider);
	}
}
