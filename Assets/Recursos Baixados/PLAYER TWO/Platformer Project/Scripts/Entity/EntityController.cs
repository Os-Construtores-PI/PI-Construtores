using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Entity/Entity Controller")]
	public class EntityController : MonoBehaviour
	{
		[Range(0, 180f)]
		[Tooltip(
			"The maximum angle of the ground the controller can walk on. "
				+ "Any angle above this value will be considered a wall."
		)]
		public float slopeLimit = 45f;

		[Min(0)]
		[Tooltip("The distance the controller will step up when it encounters a step.")]
		public float stepOffset = 0.3f;

		[Min(0.0001f)]
		[Tooltip(
			"An additional skin width to prevent the controller from getting stuck on walls and jitter."
		)]
		public float skinWidth = 0.01f;

		[Tooltip("The center of the entity collider.")]
		public Vector3 center;

		[Min(0)]
		[SerializeField]
		[Tooltip("The radius of the entity collider.")]
		protected float m_radius = 0.5f;

		[Min(0)]
		[SerializeField]
		[Tooltip("The height of the entity collider.")]
		protected float m_height = 2f;

		[Tooltip("The layer mask used to detect collisions.")]
		public LayerMask collisionLayer = -5;

		protected const int k_maxCollisionSteps = 3;
		protected const int k_gapCheckAngle = 35;

		protected Rigidbody m_rigidbody;
		protected Collider[] m_overlaps = new Collider[128];
		protected List<Collider> m_ignoredColliders = new();

		/// <summary>
		/// The list of contacts detected during the last movement.
		/// </summary>
		public List<EntityControllerContact> contacts { get; protected set; } = new();

		public bool handleCollision { get; set; } = true;
		public bool handleSteps { get; set; } = true;

		/// <summary>
		/// The radius of the entity collider.
		/// </summary>
		public float radius
		{
			get { return Mathf.Max(m_radius, skinWidth); }
			set { m_radius = value; }
		}

		/// <summary>
		/// The height of the entity collider.
		/// </summary>
		public float height
		{
			get { return Mathf.Max(m_height, radius * 2); }
			set { m_height = value; }
		}

		public new CapsuleCollider collider { get; protected set; }

		public Bounds bounds => collider.bounds;
		public Vector3 capsuleOffset => transform.up * (height * 0.5f - radius);

		/// <summary>
		/// Returns true if the entity collider is spherical.
		/// </summary>
		public bool isSpherical => height <= radius * 2f;

		protected virtual void Awake()
		{
			InitializeCollider();
			InitializeRigidbody();
			RefreshCollider();
		}

		protected virtual void OnDisable() => collider.enabled = false;

		protected virtual void OnEnable() => collider.enabled = true;

		protected virtual void InitializeCollider()
		{
			collider = gameObject.AddComponent<CapsuleCollider>();
			collider.isTrigger = true;
		}

		protected virtual void InitializeRigidbody()
		{
			if (!TryGetComponent(out m_rigidbody))
			{
				m_rigidbody = gameObject.AddComponent<Rigidbody>();
			}

			m_rigidbody.isKinematic = true;
		}

		public virtual void Move(Vector3 motion)
		{
			if (!enabled)
				return;

			var position = transform.position;

			contacts.Clear();

			if (handleCollision)
			{
				var localMotion = transform.InverseTransformDirection(motion);
				var lateralMotion = new Vector3(localMotion.x, 0, localMotion.z);
				var verticalMotion = new Vector3(0, localMotion.y, 0);

				lateralMotion = transform.TransformDirection(lateralMotion);
				verticalMotion = transform.TransformDirection(verticalMotion);

				position = MoveAndSlide(position, lateralMotion);
				position = MoveAndSlide(position, verticalMotion, true);
				position = HandlePenetration(position);
			}
			else
			{
				position += motion;
			}

			transform.position = position;
		}

		public virtual void Resize(float height)
		{
			var originalHeight = this.height;
			this.height = height;
			var delta = this.height - originalHeight;
			center += delta * 0.5f * Vector3.up;
			RefreshCollider();
		}

		/// <summary>
		/// Add or remove a collider to the ignore list, making it so the controller will not collide with it.
		/// </summary>
		/// <param name="collider">The collider you want to add or remove.</param>
		/// <param name="ignore">If true, the collider will be added to the ignore list.</param>
		public virtual void IgnoreCollider(Collider collider, bool ignore = true)
		{
			if (ignore)
			{
				if (!m_ignoredColliders.Contains(collider))
					m_ignoredColliders.Add(collider);
			}
			else if (m_ignoredColliders.Contains(collider))
				m_ignoredColliders.Remove(collider);
		}

		protected virtual void RefreshCollider()
		{
			collider.radius = radius - skinWidth;
			collider.height = height - skinWidth;
			collider.center = center;
		}

		protected virtual Vector3 MoveAndSlide(
			Vector3 position,
			Vector3 motion,
			bool verticalPass = false
		)
		{
			for (int i = 0; i < k_maxCollisionSteps - 1; i++)
			{
				var moveDistance = motion.magnitude;
				var moveDirection = motion / moveDistance;
				var movingTowardsGap = !verticalPass && GapCheck(position);

				if (moveDistance <= Mathf.Epsilon || movingTowardsGap)
					break;

				var distance = moveDistance + radius - skinWidth;
				var origin = position + transform.rotation * center - moveDirection * radius;
				var point1 = origin - capsuleOffset;
				var point2 = origin + capsuleOffset;

				if (!verticalPass)
				{
					distance += 0.01f;

					if (handleSteps && !isSpherical)
						point1 += transform.up * stepOffset;
					else
						origin += 0.5f * stepOffset * transform.up;
				}

				var colliding = SweepTest(
					origin,
					point1,
					point2,
					moveDirection,
					distance,
					out var hit
				);

				if (colliding && !m_ignoredColliders.Contains(hit.collider))
				{
					var safeDistance = hit.distance - skinWidth - radius;
					var offset = moveDirection * safeDistance;
					var normal =
						verticalPass && handleSteps && !isSpherical
							? RaycastHelpers.GetRealNormal(hit, collisionLayer)
							: hit.normal;
					var angle = Vector3.Angle(transform.up, normal);

					position += offset;
					contacts.Add(new EntityControllerContact(hit, verticalPass));

					if (angle <= slopeLimit && verticalPass)
						continue;

					var leftover = motion - offset;
					motion = Vector3.ProjectOnPlane(leftover, normal);

					if (
						!verticalPass
						&& angle >= slopeLimit
						&& Vector3.Dot(transform.up, normal) > 0
					)
						motion -= transform.up * Vector3.Dot(motion, transform.up);
				}
				else
				{
					position += motion;
					break;
				}
			}

			return position;
		}

		protected virtual Vector3 HandlePenetration(Vector3 position)
		{
			var origin = position + transform.rotation * center;
			var penetrations = isSpherical ? OverlapSphere(origin) : OverlapCapsule(origin);

			for (int i = 0; i < penetrations; i++)
			{
				if (m_ignoredColliders.Contains(m_overlaps[i]))
					continue;

				if (
					Physics.ComputePenetration(
						collider,
						origin,
						transform.rotation,
						m_overlaps[i],
						m_overlaps[i].transform.position,
						m_overlaps[i].transform.rotation,
						out var direction,
						out var distance
					)
				)
				{
					if (m_overlaps[i].transform == transform)
						continue;

					if (HandlePlatform(m_overlaps[i], origin, direction, distance, ref position))
						continue;

					HandleCrunch(origin, ref direction);

					position += direction * distance;
				}
			}

			return position;
		}

		protected virtual bool HandlePlatform(
			Collider collider,
			Vector3 worldCenter,
			Vector3 direction,
			float distance,
			ref Vector3 position
		)
		{
			if (
				GameTags.IsPlatform(collider)
				&& SweepTest(
					worldCenter,
					worldCenter - capsuleOffset,
					worldCenter + capsuleOffset,
					direction,
					distance,
					out _
				)
			)
			{
				contacts.Add(
					new EntityControllerContact
					{
						collider = collider,
						point = worldCenter - direction * distance,
						normal = direction,
					}
				);

				position += 0.5f * height * transform.up;
				return true;
			}

			return false;
		}

		protected virtual void HandleCrunch(Vector3 worldCenter, ref Vector3 direction)
		{
			var groundHit = new RaycastHit();
			var maxGroundDistance = height * 0.5f + radius - skinWidth;
			var groundCrunch =
				Vector3.Dot(direction, transform.up) < 0
				&& Physics.Raycast(
					worldCenter,
					-transform.up,
					out groundHit,
					maxGroundDistance,
					collisionLayer,
					QueryTriggerInteraction.Ignore
				);

			if (groundCrunch)
				direction = Vector3.ProjectOnPlane(direction, groundHit.normal);
		}

		protected virtual bool SweepTest(
			Vector3 position,
			Vector3 point1,
			Vector3 point2,
			Vector3 direction,
			float distance,
			out RaycastHit hit
		)
		{
			var sphereColliding = Physics.SphereCast(
				position,
				radius,
				direction,
				out var sphereHit,
				distance,
				collisionLayer,
				QueryTriggerInteraction.Ignore
			);

			var capsuleColliding = Physics.CapsuleCast(
				point1,
				point2,
				radius,
				direction,
				out var capsuleHit,
				distance,
				collisionLayer,
				QueryTriggerInteraction.Ignore
			);

			hit = isSpherical ? sphereHit : capsuleHit;

			var colliding = isSpherical ? sphereColliding : capsuleColliding;

			return colliding
				|| Physics.Raycast(
					position,
					direction,
					out hit,
					distance,
					collisionLayer,
					QueryTriggerInteraction.Ignore
				);
		}

		protected virtual bool GapCheck(Vector3 position)
		{
			var radius = this.radius * 2f;
			var direction = transform.forward;
			var origin = position + transform.rotation * center;
			var firstDirection = Quaternion.AngleAxis(k_gapCheckAngle, transform.up) * direction;
			var secondDirection = Quaternion.AngleAxis(-k_gapCheckAngle, transform.up) * direction;

			var firstCollision = Physics.Raycast(
				origin,
				firstDirection,
				out var firstHit,
				radius,
				collisionLayer,
				QueryTriggerInteraction.Ignore
			);

			if (!firstCollision || m_ignoredColliders.Contains(firstHit.collider))
				return false;

			var secondCollision = Physics.Raycast(
				origin,
				secondDirection,
				out var secondHit,
				radius,
				collisionLayer,
				QueryTriggerInteraction.Ignore
			);

			if (!secondCollision || m_ignoredColliders.Contains(secondHit.collider))
				return false;

			return firstHit.normal != secondHit.normal;
		}

		protected virtual int OverlapCapsule(Vector3 origin)
		{
			var point1 = origin - capsuleOffset;
			var point2 = origin + capsuleOffset;
			return Physics.OverlapCapsuleNonAlloc(
				point1,
				point2,
				radius,
				m_overlaps,
				collisionLayer,
				QueryTriggerInteraction.Ignore
			);
		}

		protected virtual int OverlapSphere(Vector3 origin)
		{
			return Physics.OverlapSphereNonAlloc(
				origin,
				radius,
				m_overlaps,
				collisionLayer,
				QueryTriggerInteraction.Ignore
			);
		}

		public static implicit operator Collider(EntityController controller) =>
			controller.collider;
	}
}
