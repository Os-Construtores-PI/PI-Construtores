using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace PLAYERTWO.PlatformerProject
{
	public abstract class Entity : MonoBehaviour
	{
		[Header("Entity Settings")]
		public EntityEvents entityEvents;

		[Tooltip(
			"If true, the Entity will rotate to the ground surface. Note that gravity fields overrides this rotation."
		)]
		public bool rotateToGround;

		[Tooltip(
			"If true, the Entity will rotate to the moving platform surface. Note that gravity fields overrides this rotation."
		)]
		public bool rotateToPlatforms = true;

		[Tooltip(
			"If the lateral velocity is bellow this value, the Entity will fall from the ground."
		)]
		public float minSpeedToFall = 5;

		protected Collider[] m_contactBuffer = new Collider[10];
		protected List<IEntityContact> m_entityContactsBuffer = new();

		protected Platform m_tempPlatform;

		protected readonly float m_groundOffset = 0.1f;
		protected readonly float m_wallOffset = 0.025f;
		protected readonly float m_ceilingOffset = 0.1f;

		protected readonly float m_slopingGroundAngle = 20f;
		protected readonly float m_minCeilingAngle = 120f;
		protected readonly float m_minWallAngle = 90f;

		protected readonly float m_minGroundRotationSpeed = 45f;
		protected readonly float m_maxGroundRotationSpeed = 180f;

		protected float m_lockGravityTime;
		protected float m_lockGravityFieldTime;

		/// <summary>
		/// Returns the Character Controller of this Entity.
		/// </summary>
		public EntityController controller { get; protected set; }

		/// <summary>
		/// The current velocity of this Entity.
		/// </summary>
		public Vector3 velocity { get; set; }

		/// <summary>
		/// The current velocity of this Entity in the local space.
		/// </summary>
		/// <value></value>
		public Vector3 localVelocity
		{
			get { return Quaternion.FromToRotation(transform.up, Vector3.up) * velocity; }
			set { velocity = Quaternion.FromToRotation(Vector3.up, transform.up) * value; }
		}

		/// <summary>
		/// The current XZ velocity of this Entity.
		/// </summary>
		public Vector3 lateralVelocity
		{
			get
			{
				var value = new Vector3(localVelocity.x, 0, localVelocity.z);

				if (value.sqrMagnitude < 0.0001f)
					return Vector3.zero;

				return value;
			}
			set { localVelocity = new Vector3(value.x, localVelocity.y, value.z); }
		}

		/// <summary>
		/// The current Y velocity of this Entity.
		/// </summary>
		public Vector3 verticalVelocity
		{
			get { return new Vector3(0, localVelocity.y, 0); }
			set
			{
				localVelocity = new Vector3(localVelocity.x, value.y, localVelocity.z);

				if (!isGrounded && localVelocity.y < 0)
					fallDuration += Time.deltaTime;
				else
					fallDuration = 0;
			}
		}

		/// <summary>
		/// Returns the Entity position in the previous frame.
		/// </summary>
		public Vector3 lastPosition { get; set; }

		/// <summary>
		/// Return the Entity center position in world space.
		/// </summary>
		public Vector3 position
		{
			get { return transform.position + transform.rotation * center; }
			set { transform.position = value - transform.rotation * center; }
		}

		/// <summary>
		/// Returns the Entity position ignoring any collision resizing.
		/// </summary>
		public Vector3 unsizedPosition =>
			position - transform.up * height * 0.5f + 0.5f * originalHeight * transform.up;

		/// <summary>
		/// Returns the bottom position of this Entity considering the stepOffset.
		/// </summary>
		public Vector3 stepPosition =>
			position - transform.up * (height * 0.5f - controller.stepOffset);

		/// <summary>
		/// Returns true if this entity Update is called manually by another script.
		/// </summary>
		public bool manualUpdate { get; set; }

		/// <summary>
		/// The distance between the last and current Entity position.
		/// </summary>
		public float positionDelta { get; protected set; }

		/// <summary>
		/// Returns the last frame this Entity was grounded.
		/// </summary>
		public float lastGroundTime { get; protected set; }

		/// <summary>
		/// Returns for the time in seconds of how long this Entity is moving downwards in the air.
		/// </summary>
		public float fallDuration { get; protected set; }

		/// <summary>
		/// Returns true if the Entity is on the ground.
		/// </summary>
		public bool isGrounded { get; protected set; } = true;

		/// <summary>
		/// Returns true if the Entity is on Rail.
		/// </summary>
		public bool onRails { get; set; }

		public float accelerationMultiplier { get; set; } = 1f;

		public float gravityMultiplier { get; set; } = 1f;

		public float topSpeedMultiplier { get; set; } = 1f;

		public float turningDragMultiplier { get; set; } = 1f;

		public float decelerationMultiplier { get; set; } = 1f;

		/// <summary>
		/// Returns the current top speed the Entity is trying to reach.
		/// </summary>
		public float targetTopSpeed { get; set; }

		/// <summary>
		/// Returns the move direction the Entity is trying to reach in local space.
		/// </summary>
		public Vector3 targetMoveDirection { get; set; }

		/// <summary>
		/// Returns the hit info of the ground.
		/// </summary>
		public RaycastHit groundHit;

		/// <summary>
		/// Returns the current moving platform this Entity is attached to.
		/// </summary>
		public Platform platform { get; protected set; }

		/// <summary>
		/// Returns the current rails this Entity is attached to.
		/// </summary>
		public SplineContainer rails { get; protected set; }

		/// <summary>
		/// Returns the current Gravity Field of this Entity.
		/// </summary>
		public GravityField gravityField { get; set; }

		/// <summary>
		/// Returns the angle of the current ground.
		/// </summary>
		public float groundAngle { get; protected set; }

		/// <summary>
		/// Returns the ground normal of the current ground.
		/// </summary>
		public Vector3 groundNormal { get; protected set; }

		/// <summary>
		/// Returns the ground normal used for applying rotation.
		/// </summary>
		public Vector3 rotationNormal { get; protected set; }

		/// <summary>
		/// Returns the local slope direction of the current ground.
		/// </summary>
		public Vector3 localSlopeDirection { get; protected set; }

		/// <summary>
		/// Returns the original height of this Entity.
		/// </summary>
		public float originalHeight { get; protected set; }

		/// <summary>
		/// Returns the original forward direction of this Entity.
		/// </summary>
		public Vector3 originalForward { get; protected set; }

		/// <summary>
		/// Returns the original position of this Entity.
		/// </summary>
		public Vector3 originalPosition { get; protected set; }

		/// <summary>
		/// Returns the original rotation of this Entity.
		/// </summary>
		public Quaternion originalRotation { get; protected set; }

		/// <summary>
		/// Returns the collider height of this Entity.
		/// </summary>
		public float height => controller.height;

		/// <summary>
		/// Returns the collider radius of this Entity.
		/// </summary>
		public float radius => controller.radius;

		/// <summary>
		/// The center of the Character Controller collider.
		/// </summary>
		public Vector3 center => controller.center;

		public virtual Vector3 localForward =>
			Quaternion.FromToRotation(transform.up, Vector3.up) * transform.forward;

		public virtual Vector3 localRight =>
			Quaternion.FromToRotation(transform.up, Vector3.up) * transform.right;

		protected BoxCollider m_penetratorCollider;

		protected Rigidbody m_rigidbody;

		/// <summary>
		/// Returns the current up direction of the world.
		/// </summary>
		public virtual Vector3 CurrentWorldUp() =>
			gravityField ? -gravityField.GetGravityDirectionFrom(position) : Vector3.up;

		/// <summary>
		/// Returns true if the Player is on a sloping ground.
		/// </summary>
		/// <returns></returns>
		public virtual bool OnSlopingGround() =>
			isGrounded
			&& !platform
			&& Vector3.Angle(rotationNormal, CurrentWorldUp()) > m_slopingGroundAngle;

		/// <summary>
		/// Returns true if this Entity can change to a given gravity field.
		/// </summary>
		/// <param name="field">The field you want to validate.</param>
		public virtual bool CanChangeToGravityField(GravityField field) => gravityField != field;

		/// <summary>
		/// Resizes the Character Controller to a given height.
		/// </summary>
		/// <param name="height">The desired height.</param>
		public virtual void ResizeCollider(float height) => controller.Resize(height);

		public virtual bool CapsuleCast(
			Vector3 direction,
			float distance,
			int layer = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore
		)
		{
			return CapsuleCast(direction, distance, out _, layer, queryTriggerInteraction);
		}

		public virtual bool CapsuleCast(
			Vector3 direction,
			float distance,
			out RaycastHit hit,
			int layer = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore
		)
		{
			var origin = position - direction * radius;
			var offset = transform.up * (height * 0.5f - radius);
			var top = origin + offset;
			var bottom = origin - offset;
			return Physics.CapsuleCast(
				top,
				bottom,
				radius,
				direction,
				out hit,
				distance + radius,
				layer,
				queryTriggerInteraction
			);
		}

		public virtual bool SphereCast(
			Vector3 direction,
			float distance,
			int layer = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore
		)
		{
			return SphereCast(direction, distance, out _, layer, queryTriggerInteraction);
		}

		public virtual bool SphereCast(
			Vector3 direction,
			float distance,
			out RaycastHit hit,
			int layer = Physics.DefaultRaycastLayers,
			QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore
		)
		{
			var castDistance = Mathf.Abs(distance - radius);
			return Physics.SphereCast(
				position,
				radius,
				direction,
				out hit,
				castDistance,
				layer,
				queryTriggerInteraction
			);
		}

		public virtual int OverlapEntity(Collider[] result, float skinOffset = 0) =>
			OverlapEntity(position, result, skinOffset);

		public virtual int OverlapEntity(Vector3 position, Collider[] result, float skinOffset = 0)
		{
			var overlapRadius = radius + skinOffset;
			var offset = (height + skinOffset) * 0.5f - radius;
			var top = position + transform.up * offset;
			var bottom = position - transform.up * offset;

			return Physics.OverlapCapsuleNonAlloc(
				top,
				bottom,
				overlapRadius,
				result,
				Physics.DefaultRaycastLayers,
				QueryTriggerInteraction.Ignore
			);
		}

		/// <summary>
		/// Returns true if there is ground below this Entity.
		/// </summary>
		/// <param name="hit">The RaycastHit information of the ground below.</param>
		/// <returns>True if there is ground below.</returns>
		public virtual bool HasGroundBelow(out RaycastHit hit) =>
			Physics.Raycast(
				position,
				-transform.up,
				out hit,
				height,
				controller.collisionLayer,
				QueryTriggerInteraction.Ignore
			);

		public virtual void ApplyDamage(int damage, Vector3 origin) { }

		public abstract void EntityUpdate();
	}

	public abstract class Entity<T> : Entity
		where T : Entity<T>
	{
		/// <summary>
		/// Returns the State Manager of this Entity.
		/// </summary>
		public EntityStateManager<T> states { get; protected set; }

		protected virtual void InitializeController()
		{
			controller = GetComponent<EntityController>();

			if (!controller)
			{
				controller = gameObject.AddComponent<EntityController>();
			}

			originalHeight = controller.height;
		}

		protected virtual void InitializeRigidbody()
		{
			m_rigidbody = gameObject.AddComponent<Rigidbody>();
			m_rigidbody.isKinematic = true;
		}

		protected virtual void InitializeStateManager() =>
			states = GetComponent<EntityStateManager<T>>();

		protected virtual void CacheInitialStates()
		{
			originalPosition = transform.position;
			originalRotation = transform.rotation;
			originalForward = transform.forward;
		}

		protected virtual void HandleStates() => states.Step();

		protected virtual void HandleController()
		{
			if (controller.enabled)
			{
				controller.Move(velocity * Time.deltaTime);
				return;
			}

			transform.position += velocity * Time.deltaTime;
		}

		protected virtual void HandleSpline()
		{
			var distance = (height * 0.5f) + height * 0.5f;

			if (
				SphereCast(-transform.up, distance, out var hit)
				&& hit.collider.CompareTag(GameTags.InteractiveRail)
			)
			{
				if (!onRails && verticalVelocity.y <= 0)
				{
					EnterRail(hit.collider.GetComponent<SplineContainer>());
				}
			}
			else
			{
				ExitRail();
			}
		}

		protected virtual bool GroundRaycast(float distance, out RaycastHit hit) =>
			IndividualGroundSpherecast(distance, out hit)
			|| IndividualGroundRaycast(-transform.forward * radius, distance, out hit)
			|| IndividualGroundRaycast(transform.forward * radius, distance, out hit)
			|| IndividualGroundRaycast(transform.right * radius, distance, out hit)
			|| IndividualGroundRaycast(-transform.right * radius, distance, out hit);

		protected virtual bool IndividualGroundSpherecast(float distance, out RaycastHit hit) =>
			SphereCast(-transform.up, distance, out hit, controller.collisionLayer)
			&& ValidGroundHit(hit);

		protected virtual bool IndividualGroundRaycast(
			Vector3 offset,
			float distance,
			out RaycastHit hit
		) =>
			Physics.Raycast(
				position + offset,
				-transform.up,
				out hit,
				distance,
				controller.collisionLayer,
				QueryTriggerInteraction.Ignore
			) && ValidGroundHit(hit);

		protected virtual bool ValidGroundHit(RaycastHit hit)
		{
			var up = rotateToGround ? transform.up : CurrentWorldUp();
			var angle = Vector3.Angle(hit.normal, up);
			return angle <= controller.slopeLimit;
		}

		protected virtual void HandleGround()
		{
			if (onRails)
				return;

			var groundOffset = isGrounded ? controller.stepOffset * 2f : m_groundOffset;
			var distance = (height * 0.5f) + groundOffset;
			var colliding = GroundRaycast(distance, out var hit);
			var movingTowardGround = colliding && Vector3.Dot(velocity, hit.normal) <= 0;
			var landing = colliding && movingTowardGround;

			if (landing)
			{
				if (!isGrounded && EvaluateLanding(hit))
					EnterGround(hit);

				UpdateGround(hit);
				HandleGroundFall();
			}
			else
				ExitGround();
		}

		protected virtual void HandleContacts()
		{
			foreach (var contact in controller.contacts)
			{
				OnContact(contact.collider);
				InvokeContacts(contact.collider);
			}
		}

		protected virtual void HandleCeiling()
		{
			if (verticalVelocity.y <= 0)
				return;

			foreach (var contact in controller.contacts)
			{
				if (!contact.enabled)
					continue;

				var worldUp = CurrentWorldUp();

				if (Vector3.Angle(contact.normal, worldUp) >= m_minCeilingAngle)
					verticalVelocity = Vector3.zero;
			}
		}

		protected virtual void HandleWall()
		{
			if (lateralVelocity.sqrMagnitude < 0.01f)
				return;

			foreach (var contact in controller.contacts)
			{
				if (!contact.enabled || contact.verticalPass)
					continue;

				var angle = Vector3.Angle(contact.normal, transform.up);

				if (angle > controller.slopeLimit && angle < m_minCeilingAngle)
				{
					if (isGrounded)
					{
						var worldToLocal = Quaternion.FromToRotation(transform.up, Vector3.up);
						var localNormal = worldToLocal * contact.normal;
						var newVelocity = Vector3.ProjectOnPlane(lateralVelocity, localNormal);
						lateralVelocity = newVelocity;
						targetMoveDirection = newVelocity.normalized;
						return;
					}

					var localToWorld = Quaternion.FromToRotation(Vector3.up, transform.up);
					var direction = (localToWorld * lateralVelocity).normalized;
					var factor = Mathf.Clamp01(Vector3.Dot(-direction, contact.normal));
					var seed = Mathf.Max(lateralVelocity.magnitude, targetTopSpeed);
					var newSpeed = Mathf.Lerp(seed, 0.05f, factor);
					lateralVelocity = Vector3.ClampMagnitude(lateralVelocity, newSpeed);
				}
			}
		}

		protected virtual void HandlePosition()
		{
			positionDelta = (position - lastPosition).magnitude;
			lastPosition = position;
		}

		protected virtual void InvokeContacts(Collider collider)
		{
			if (!collider)
				return;

			collider.GetComponents(m_entityContactsBuffer);

			foreach (var listener in m_entityContactsBuffer)
				listener.OnEntityContact((T)this);
		}

		protected virtual void RotateTo(Vector3 normal, bool adjustVelocity = true)
		{
			var rotation = Quaternion.FromToRotation(transform.up, normal);
			transform.rotation = rotation * transform.rotation;

			if (adjustVelocity)
				velocity = rotation * velocity;
		}

		protected virtual void HandleAirToGround(float conversionFactor = 1f)
		{
			if (!rotateToGround)
				return;

			var normal = RaycastHelpers.GetRealNormal(groundHit, controller.collisionLayer);
			var groundAngle = Vector3.Angle(CurrentWorldUp(), normal);

			if (groundAngle < m_slopingGroundAngle || !HasGroundBelow(out _))
				return;

			var verticalSpeed = verticalVelocity.magnitude;
			var localSlopeDirection = new Vector3(normal.x, 0, normal.z).normalized;
			lateralVelocity += conversionFactor * verticalSpeed * localSlopeDirection;

			RotateTo(normal);
		}

		protected virtual void HandleGroundToAir()
		{
			verticalVelocity = Vector3.Max(verticalVelocity, Vector3.zero);

			if (!rotateToGround)
				return;

			var worldUp = CurrentWorldUp();
			var rotation = Quaternion.FromToRotation(transform.up, worldUp);
			transform.rotation = rotation * transform.rotation;
		}

		protected virtual void EnterGround(RaycastHit hit)
		{
			if (isGrounded)
				return;

			groundHit = hit;
			isGrounded = true;
			controller.handleSteps = true;
			m_lockGravityTime = Time.time;
			HandleAirToGround();
			entityEvents.OnGroundEnter?.Invoke();
		}

		protected virtual void ExitGround()
		{
			if (!isGrounded)
				return;

			isGrounded = false;
			lastGroundTime = Time.time;
			controller.handleSteps = false;
			HandleGroundToAir();
			ExitMovingPlatform();
			entityEvents.OnGroundExit?.Invoke();
		}

		public virtual void EnterRail(SplineContainer rails)
		{
			if (!onRails)
			{
				onRails = true;
				this.rails = rails;
				entityEvents.OnRailsEnter.Invoke();
			}
		}

		public virtual void ExitRail()
		{
			if (onRails)
			{
				onRails = false;
				entityEvents.OnRailsExit.Invoke();
			}
		}

		public virtual void EnterMovingPlatform(Collider other)
		{
			if (!other || (platform && platform.transform == other.transform))
				return;

			if (other.TryGetComponent(out m_tempPlatform))
			{
				if (platform)
					platform.Detach(transform);

				platform = m_tempPlatform;
				platform.Attach(transform);
			}
		}

		public virtual void ExitMovingPlatform()
		{
			if (!platform)
				return;

			var worldRotation = Quaternion.FromToRotation(transform.up, CurrentWorldUp());
			transform.rotation = worldRotation * transform.rotation;
			platform.Detach(transform);
			platform = null;
		}

		public virtual void HandlePlatform(Collider other)
		{
			if (GameTags.IsPlatform(other))
				EnterMovingPlatform(other);
			else
				ExitMovingPlatform();
		}

		/// <summary>
		/// Locks the gravity for a given duration.
		/// </summary>
		/// <param name="duration">The duration in seconds to lock the gravity.</param>
		public virtual void LockGravity(float duration = 0.5f) =>
			m_lockGravityTime = Time.time + duration;

		/// <summary>
		/// Locks the gravity field rotation for a given duration.
		/// </summary>
		/// <param name="duration">The duration in seconds to lock the gravity field rotation.</param>
		public virtual void LockGravityField(float duration = 0.1f) =>
			m_lockGravityFieldTime = Time.time + duration;

		protected virtual void UpdateGravityField()
		{
			if (!gravityField || onRails || Time.time <= m_lockGravityFieldTime)
				return;

			var point = isGrounded ? position : stepPosition;
			var direction = gravityField.GetGravityDirectionFrom(point);

			RotateTo(-direction, gravityField.rotateVelocity);
		}

		protected virtual void UpdateGroundRotation()
		{
			var rotateToPlatforms = this.rotateToPlatforms && platform;

			if (gravityField || onRails || (!rotateToGround && !rotateToPlatforms))
				return;

			var speed = lateralVelocity.magnitude;
			var normal = isGrounded ? rotationNormal : CurrentWorldUp();
			var rotation = Quaternion.FromToRotation(transform.up, normal);
			var targetRotation = rotation * transform.rotation;

			if (speed <= targetTopSpeed)
			{
				var speedFactor = Mathf.Clamp01(speed / targetTopSpeed);
				var rotationSpeed = Mathf.Lerp(
					m_minGroundRotationSpeed,
					m_maxGroundRotationSpeed,
					speedFactor
				);
				targetRotation = Quaternion.RotateTowards(
					transform.rotation,
					rotation * transform.rotation,
					rotationSpeed * Time.deltaTime
				);
			}

			var rotationDelta = targetRotation * Quaternion.Inverse(transform.rotation);
			transform.rotation = targetRotation;
			velocity = rotationDelta * velocity;
		}

		protected virtual void UpdateGround(RaycastHit hit)
		{
			if (!isGrounded)
				return;

			groundHit = hit;
			groundNormal = groundHit.normal;

			if (HasGroundBelow(out var nearHit))
				rotationNormal = nearHit.normal;

			groundAngle = Vector3.Angle(CurrentWorldUp(), groundHit.normal);
			localSlopeDirection = new Vector3(rotationNormal.x, 0, rotationNormal.z).normalized;
			HandlePlatform(groundHit.collider);
		}

		protected virtual void HandleGroundFall()
		{
			if (!isGrounded || gravityField || platform)
				return;

			var worldUp = CurrentWorldUp();
			var rotationAngle = Vector3.Angle(transform.up, worldUp);
			var steepGround = rotationAngle >= m_slopingGroundAngle;
			var falling = steepGround && lateralVelocity.magnitude < minSpeedToFall;

			if (falling)
			{
				if (rotationAngle >= m_minWallAngle)
					ExitGround();

				entityEvents.OnGroundFall.Invoke();
			}
		}

		protected virtual bool EvaluateLanding(RaycastHit hit) =>
			Vector3.Angle(hit.normal, transform.up) < controller.slopeLimit;

		protected virtual void OnUpdate() { }

		protected virtual void OnContact(Collider other)
		{
			if (other)
			{
				states.OnContact(other);
			}
		}

		/// <summary>
		/// Moves the Player smoothly in a given direction.
		/// </summary>
		/// <param name="direction">The direction you want to move.</param>
		/// <param name="turningDrag">How fast it will turn towards the new direction.</param>
		/// <param name="acceleration">How fast it will move over time.</param>
		/// <param name="maxTopSpeed">The maximum magnitude the speed can reach through acceleration.</param>
		/// <param name="minTopSpeed">The minimum magnitude the speed can reach through acceleration. Only used when magnitude is less than 1.</param>
		/// <param name="magnitude">The magnitude of the direction.</param>
		public virtual void Accelerate(
			Vector3 direction,
			float turningDrag,
			float acceleration,
			float maxTopSpeed,
			float minTopSpeed = 0,
			float magnitude = 1
		)
		{
			if (direction.sqrMagnitude <= 0)
				return;

			var speed = Vector3.Dot(direction, lateralVelocity);
			var velocity = direction * speed;
			var turningVelocity = lateralVelocity - velocity;
			var turningDelta = turningDrag * turningDragMultiplier * Time.deltaTime;
			var topSpeed = Mathf.Max(minTopSpeed, maxTopSpeed * magnitude);

			targetMoveDirection = direction;
			targetTopSpeed = topSpeed * topSpeedMultiplier;

			if (speed < topSpeed)
			{
				speed += acceleration * accelerationMultiplier * Time.deltaTime;
				speed = Mathf.Clamp(speed, -targetTopSpeed, targetTopSpeed);
			}

			velocity = direction * speed;
			turningVelocity = Vector3.MoveTowards(turningVelocity, Vector3.zero, turningDelta);
			lateralVelocity = velocity + turningVelocity;
		}

		/// <summary>
		/// Smoothly moves Lateral Velocity to zero.
		/// </summary>
		/// <param name="deceleration">How fast it will decelerate over time.</param>
		public virtual void Decelerate(float deceleration)
		{
			var delta = deceleration * decelerationMultiplier * Time.deltaTime;
			lateralVelocity = Vector3.MoveTowards(lateralVelocity, Vector3.zero, delta);
			targetMoveDirection = Vector3.zero;
		}

		/// <summary>
		/// Smoothly moves vertical velocity to zero.
		/// </summary>
		/// <param name="gravity">How fast it will move over time.</param>
		public virtual void Gravity(float gravity)
		{
			if (!isGrounded)
			{
				if (Time.time < m_lockGravityTime)
					verticalVelocity = Vector3.zero;
				else
					verticalVelocity += gravity * gravityMultiplier * Time.deltaTime * Vector3.down;
			}
		}

		/// <summary>
		/// Increases the lateral velocity based on the slope angle.
		/// </summary>
		/// <param name="upwardForce">The force applied when moving upwards.</param>
		/// <param name="downwardForce">The force applied when moving downwards.</param>
		public virtual void SlopeFactor(float upwardForce, float downwardForce)
		{
			if (!isGrounded || !OnSlopingGround())
				return;

			var worldUp = CurrentWorldUp();
			var horizontalNormal = Vector3.ProjectOnPlane(rotationNormal, worldUp);
			var factor = horizontalNormal.magnitude;
			var downwards = Vector3.Dot(localSlopeDirection, lateralVelocity) > 0;
			var multiplier = downwards ? downwardForce : upwardForce;
			lateralVelocity += factor * multiplier * Time.deltaTime * localSlopeDirection;
		}

		/// <summary>
		/// Applies a force towards the ground.
		/// </summary>
		/// <param name="force">The force you want to apply.</param>
		public virtual void SnapToGround(float force)
		{
			if (isGrounded && (verticalVelocity.y <= 0))
			{
				verticalVelocity = Vector3.down * force;
			}
		}

		/// <summary>
		/// Rotate the Player towards to a given direction.
		/// </summary>
		/// <param name="direction">The direction you want to face.</param>
		public virtual void FaceDirection(Vector3 direction, Space space = Space.Self)
		{
			if (direction != Vector3.zero)
			{
				if (space == Space.Self)
					direction = Quaternion.FromToRotation(Vector3.up, transform.up) * direction;

				var rotation = Quaternion.LookRotation(direction, transform.up);
				transform.rotation = rotation;
			}
		}

		/// <summary>
		/// Rotate the Player towards to a given direction.
		/// </summary>
		/// <param name="direction">The direction you want to face.</param>
		/// <param name="degreesPerSecond">How fast it should rotate over time.</param>
		public virtual void FaceDirection(
			Vector3 direction,
			float degreesPerSecond,
			bool adjustUp = true
		)
		{
			if (direction != Vector3.zero)
			{
				if (adjustUp)
					direction = Quaternion.FromToRotation(Vector3.up, transform.up) * direction;

				var rotation = transform.rotation;
				var rotationDelta = degreesPerSecond * Time.deltaTime;
				var target = Quaternion.LookRotation(direction, transform.up);
				transform.rotation = Quaternion.RotateTowards(rotation, target, rotationDelta);
			}
		}

		/// <summary>
		/// Returns true if this Entity collider fits into a given position.
		/// </summary>
		/// <param name="position">The position you want to test if the Entity collider fits.</param>
		public virtual bool FitsIntoPosition(Vector3 position)
		{
			var skinOffset = controller.skinWidth + Physics.defaultContactOffset;
			var overlaps = OverlapEntity(position, m_contactBuffer, -skinOffset);

			for (int i = 0; i < overlaps; i++)
			{
				if (
					m_contactBuffer[i].gameObject.isStatic && !GameTags.IsHazard(m_contactBuffer[i])
				)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Enables or disables the custom collision. Disabling the Character Controller.
		/// </summary>
		/// <param name="value">If true, enables the custom collision.</param>
		public virtual void UseCustomCollision(bool value) => controller.handleCollision = !value;

		public override void EntityUpdate()
		{
			if (!controller.enabled)
				return;

			HandleGround();
			UpdateGroundRotation();
			HandleStates();
			UpdateGravityField();
			HandleController();
			HandleContacts();
			HandleWall();
			HandleCeiling();
			HandleSpline();
			OnUpdate();
		}

		protected virtual void Awake()
		{
			InitializeController();
			InitializeStateManager();
			CacheInitialStates();
		}

		protected virtual void Update()
		{
			if (!manualUpdate)
				EntityUpdate();
		}

		protected virtual void LateUpdate()
		{
			if (!controller.enabled)
				return;

			HandlePosition();
		}
	}
}
