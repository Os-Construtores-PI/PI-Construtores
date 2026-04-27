using UnityEngine;
using UnityEngine.Splines;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(PlayerInputManager))]
	[RequireComponent(typeof(PlayerStatsManager))]
	[RequireComponent(typeof(PlayerStateManager))]
	[RequireComponent(typeof(Health))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player")]
	public class Player : Entity<Player>
	{
		[SerializeField]
		[Header("Player Settings")]
		[Tooltip("The moving mode of this Entity.")]
		protected PlayerMovementMode m_movingMode = PlayerMovementMode.ThirdPerson;

		[Tooltip("List of events the Player can trigger.")]
		public PlayerEvents playerEvents;

		[Tooltip("References the Transform that represents the Player's skin.")]
		public Transform skin;

		protected Vector3 m_respawnPosition;
		protected Quaternion m_respawnRotation;
		protected Vector3 m_respawnPathForward;
		protected PlayerMovementMode m_respawnMovingMode;

		protected Vector3 m_skinInitialPosition;
		protected Quaternion m_skinInitialRotation;

		protected Vector3 m_pathForward;
		protected Vector3 m_closestPointOnPath;

		protected Collider[] m_homingResults = new Collider[128];
		protected float m_lastHomingUpdateTime;

		protected SplineContainer m_tempSpline;

		/// <summary>
		/// Returns the current homing target Collider, if any.
		/// </summary>
		public Collider homingTarget { get; protected set; }

		/// <summary>
		/// Returns the Spline Container of the current homing target, if any.
		/// </summary>
		public SplineContainer homingSpline { get; protected set; }

		/// <summary>
		/// Returns the initial position of the homing dash target.
		/// </summary>
		public Vector3 initialHomingTargetPosition { get; protected set; }

		/// <summary>
		/// Returns the Player Input Manager instance.
		/// </summary>
		public PlayerInputManager inputs { get; protected set; }

		/// <summary>
		/// Returns the Player Stats Manager instance.
		/// </summary>
		public PlayerStatsManager stats { get; protected set; }

		/// <summary>
		/// Returns the Health instance.
		/// </summary>
		public Health health { get; protected set; }

		/// <summary>
		/// Returns the Player Object Grabber instance.
		/// </summary>
		public PlayerObjectGrabber grabber { get; protected set; }

		/// <summary>
		/// Returns true if the Player is on water.
		/// </summary>
		public bool onWater { get; protected set; }

		/// <summary>
		/// If true, the Player can take damage.
		/// </summary>
		public bool canTakeDamage { get; set; } = true;

		/// <summary>
		/// If true, the last jump was performed from the ground.
		/// </summary>
		public bool jumpedFromGround { get; set; }

		/// <summary>
		/// Returns how many times the Player jumped.
		/// </summary>
		public int jumpCounter { get; protected set; }

		/// <summary>
		/// Returns how many times the Player performed an air spin.
		/// </summary>
		public int airSpinCounter { get; protected set; }

		/// <summary>
		/// Returns how many times the Player performed a Dash.
		/// </summary>
		/// <value></value>
		public int airDashCounter { get; protected set; }

		/// <summary>
		/// The last time the Player performed an dash.
		/// </summary>
		/// <value></value>
		public float lastDashTime { get; protected set; }

		/// <summary>
		/// Returns the normal of the last wall the Player touched.
		/// </summary>
		public Vector3 lastWallNormal { get; set; }

		/// <summary>
		/// Returns the last damage origin applied to the Player.
		/// </summary>
		public Vector3 lastDamageOrigin { get; protected set; }

		/// <summary>
		/// Returns the Pole instance in which the Player is colliding with.
		/// </summary>
		public Pole pole { get; protected set; }

		/// <summary>
		/// Returns the Collider of the water the Player is swimming.
		/// </summary>
		public Collider water { get; protected set; }

		/// <summary>
		/// Returns the Collider of the ledge the Player is hanging.
		/// </summary>
		public Collider ledge { get; protected set; }

		/// <summary>
		/// Returns the Spline Path instance in which the Player is following.
		/// </summary>
		public SplinePath splinePath { get; set; }

		/// <summary>
		/// Returns the current forward direction of the Player's path.
		/// </summary>
		public Vector3 pathForward
		{
			set { m_pathForward = value; }
			get
			{
				if (splinePath)
					return splinePath.GetPathForward(this, out m_closestPointOnPath);

				return m_pathForward;
			}
		}

		public PlayerMovementMode movingMode
		{
			get => m_movingMode;
			set
			{
				m_movingMode = value;
				playerEvents.OnMovementModeChanged.Invoke(m_movingMode);
			}
		}

		/// <summary>
		/// Returns true if the Player health is not empty.
		/// </summary>
		public virtual bool isAlive => !health.isEmpty;

		/// <summary>
		/// Returns true if the Player is holding an object.
		/// </summary>
		public bool holding => grabber?.isGrabbing ?? false;

		/// <summary>
		/// Returns true if the Player can stand up.
		/// </summary>
		public virtual bool canStandUp =>
			!SphereCast(transform.up, originalHeight * 0.5f + radius - controller.skinWidth);

		public virtual bool running =>
			inputs.GetRun() && stats.current.canRun && (stats.current.canRunOnAir || isGrounded);

		/// <summary>
		/// Returns true if the Player has valid homing dash targets.
		/// </summary>
		public virtual bool hasHomingTargets => homingTarget || homingSpline;

		/// <summary>
		/// Returns true if the moving mode is set to Third Person.
		/// </summary>
		public bool IsThirdPerson => m_movingMode == PlayerMovementMode.ThirdPerson;

		/// <summary>
		/// Returns true if the moving mode is set to Side Scroller.
		/// </summary>
		public bool IsSideScroller => m_movingMode == PlayerMovementMode.SideScroller;

		/// <summary>
		/// These states won't be affected by gravity field changes.
		/// </summary>
		protected readonly System.Type[] m_ignoreGravityFieldStates = new[]
		{
			typeof(PoleClimbingPlayerState),
			typeof(LedgeHangingPlayerState),
			typeof(LedgeClimbingPlayerState),
			typeof(SwimPlayerState),
		};

		protected const float k_waterExitOffset = 0.25f;

		protected virtual void InitializeInputs() => inputs = GetComponent<PlayerInputManager>();

		protected virtual void InitializeStats() => stats = GetComponent<PlayerStatsManager>();

		protected virtual void InitializeHealth() => health = GetComponent<Health>();

		protected virtual void InitializeGrabber() => grabber = GetComponent<PlayerObjectGrabber>();

		protected virtual void InitializeTag() => tag = GameTags.Player;

		protected virtual void InitializePathForward() =>
			m_pathForward = m_respawnPathForward = transform.forward;

		protected virtual void InitializeRespawn()
		{
			m_respawnPosition = transform.position;
			m_respawnRotation = transform.rotation;
			m_respawnMovingMode = movingMode;
		}

		protected virtual void InitializeSkin()
		{
			if (!skin)
				return;

			m_skinInitialPosition = skin.localPosition;
			m_skinInitialRotation = skin.localRotation;
		}

		/// <summary>
		/// Resets Player state, health, position, and rotation.
		/// </summary>
		public virtual void Respawn()
		{
			health.ResetHealth();
			velocity = Vector3.zero;
			gravityField = null;
			m_pathForward = m_respawnPathForward;
			movingMode = m_respawnMovingMode;
			transform.SetPositionAndRotation(m_respawnPosition, m_respawnRotation);
			states.Change<IdlePlayerState>();
		}

		/// <summary>
		/// Sets the position and rotation of the Player for the next respawn.
		/// </summary>
		public virtual void SetRespawn(Vector3 position, Quaternion rotation)
		{
			m_respawnPosition = position;
			m_respawnRotation = IsSideScroller
				? Quaternion.LookRotation(pathForward, Vector3.up)
				: rotation;
			m_respawnPathForward = pathForward;
			m_respawnMovingMode = movingMode;
		}

		/// <summary>
		/// Updates the homing dash target based on nearby valid targets.
		/// </summary>
		public virtual void UpdateHomingTargets()
		{
			if (Time.time < m_lastHomingUpdateTime + stats.current.homingDashRefreshRate)
				return;

			var radius = stats.current.homingDashRadius;
			var targets = Physics.OverlapSphereNonAlloc(
				position,
				radius,
				m_homingResults,
				stats.current.homingDashLayers
			);

			var closestTargetDistance = Mathf.Infinity;
			homingTarget = null;
			homingSpline = null;

			for (int i = 0; i < targets; i++)
			{
				var targetPos = m_homingResults[i].transform.position;
				var boundsRadius = m_homingResults[i].bounds.extents.magnitude;
				var isRail =
					GameTags.IsRail(m_homingResults[i])
					&& LevelRailsManager.instance.TryGetSplineContainer(
						m_homingResults[i],
						out m_tempSpline
					);

				if (isRail)
				{
					var origin =
						position + transform.forward * stats.current.homingDashSplineForwardOffset;
					origin = m_tempSpline.transform.InverseTransformPoint(origin);

					SplineUtility.GetNearestPoint(
						m_tempSpline.Spline,
						origin,
						out var nearest,
						out _,
						128
					);

					targetPos = m_tempSpline.transform.TransformPoint(nearest);
					boundsRadius = 0.25f;
				}

				var head = targetPos - position;
				var distance = head.magnitude;
				var direction = head / distance;
				var dot = Vector3.Dot(direction, transform.forward);

				var obstruction = Physics.SphereCast(
					position,
					controller.radius,
					direction,
					out _,
					distance - boundsRadius - controller.radius,
					controller.collisionLayer & ~stats.current.homingDashLayers,
					QueryTriggerInteraction.Ignore
				);

				var heightDiff = Vector3.Dot(head, transform.up);
				var validHeight = heightDiff < stats.current.homingDashMaxHeightDifference;

				if (!obstruction && distance < closestTargetDistance && dot > 0 && validHeight)
				{
					closestTargetDistance = distance;
					homingTarget = m_homingResults[i];
					initialHomingTargetPosition = targetPos;

					if (isRail)
						homingSpline = m_tempSpline;
				}
			}

			m_lastHomingUpdateTime = Time.time;
			playerEvents.OnHomingTargetUpdated.Invoke(homingTarget);
		}

		/// <summary>
		/// Clears the current homing dash target.
		/// </summary>
		public virtual void ClearHomingTarget()
		{
			homingTarget = null;
			homingSpline = null;
			playerEvents.OnHomingTargetUpdated.Invoke(homingTarget);
		}

		/// <summary>
		/// Returns the position of the current homing dash target.
		/// </summary>
		/// <returns>The position of the homing dash target.</returns>
		public virtual Vector3 GetHomingTargetPosition()
		{
			if (!hasHomingTargets)
				return Vector3.zero;

			if (homingSpline)
			{
				var origin =
					position + transform.forward * stats.current.homingDashSplineForwardOffset;
				origin = homingSpline.transform.InverseTransformPoint(origin);

				SplineUtility.GetNearestPoint(
					homingSpline.Spline,
					origin,
					out var nearestT,
					out _,
					128
				);

				return homingSpline.transform.TransformPoint(nearestT);
			}

			return homingTarget.transform.position;
		}

		/// <summary>
		/// Updates the homing dash target and handles the state transition.
		/// </summary>
		public virtual void HomingDash()
		{
			if (isGrounded || holding || !stats.current.canHomingDash || jumpCounter <= 0)
				return;

			UpdateHomingTargets();

			if (hasHomingTargets && inputs.GetHomingDashDown())
				states.Change<HomingDashPlayerState>();
		}

		protected override void HandleController()
		{
			if (IsSideScroller && !onRails)
			{
				var forward = Vector3.Cross(Vector3.up, pathForward).normalized;
				velocity -= forward * Vector3.Dot(velocity, forward);
			}

			base.HandleController();
		}

		protected override void HandleSpline()
		{
			base.HandleSpline();
			AdjustToPath();
		}

		/// <summary>
		/// Applies damage to this Player decreasing its health with proper reaction.
		/// </summary>
		/// <param name="amount">The amount of health you want to decrease.</param>
		public override void ApplyDamage(int amount, Vector3 origin)
		{
			if (health.isEmpty || health.recovering || !canTakeDamage)
				return;

			health.Damage(amount);
			lastDamageOrigin = origin;
			states.Change<HurtPlayerState>();
			playerEvents.OnHurt?.Invoke();

			if (health.isEmpty)
			{
				splinePath?.RemovePlayer(this);
				grabber?.Throw();
				playerEvents.OnDie?.Invoke();
			}
		}

		/// <summary>
		/// Kills the Player.
		/// </summary>
		public virtual void Die()
		{
			health.Set(0);
			playerEvents.OnDie?.Invoke();
		}

		/// <summary>
		/// Makes the Player transition to the Swim State.
		/// </summary>
		/// <param name="water">The instance of the water collider.</param>
		public virtual void EnterWater(Collider water)
		{
			var validWaterStates = states.IsCurrentOfType(
				typeof(SwimPlayerState),
				typeof(HurtPlayerState)
			);

			if ((!onWater || !validWaterStates) && !health.isEmpty)
			{
				grabber?.Throw();
				onWater = true;
				this.water = water;
				states.Change<SwimPlayerState>();
			}
		}

		/// <summary>
		/// Makes the Player exit the current water instance.
		/// </summary>
		public virtual void ExitWater()
		{
			if (onWater)
			{
				onWater = false;
			}
		}

		/// <summary>
		/// Attaches the Player to a given Pole.
		/// </summary>
		/// <param name="pole">The Pole you want to attach the Player to.</param>
		public virtual void GrabPole(Collider other)
		{
			if (!stats.current.canPoleClimb || !other.CompareTag(GameTags.Pole))
				return;

			if (verticalVelocity.y <= 0 && !holding && other.TryGetComponent(out Pole pole))
			{
				this.pole = pole;
				states.Change<PoleClimbingPlayerState>();
			}
		}

		protected override bool EvaluateLanding(RaycastHit hit)
		{
			return base.EvaluateLanding(hit) && !hit.collider.CompareTag(GameTags.Spring);
		}

		public override bool CanChangeToGravityField(GravityField field) =>
			base.CanChangeToGravityField(field) && !IgnoreGravityFieldChanges();

		protected override void UpdateGravityField()
		{
			if (IgnoreGravityFieldChanges())
				return;

			base.UpdateGravityField();
		}

		protected override void HandleAirToGround(float conversionFactor = 1f)
		{
			if (!holding && stats.current.canRoll && inputs.GetRoll())
				base.HandleAirToGround(stats.current.airToGroundRollFactor);
			else
				base.HandleAirToGround(conversionFactor);
		}

		protected virtual bool IgnoreGravityFieldChanges() =>
			states.IsCurrentOfType(m_ignoreGravityFieldStates);

		protected virtual float GetCurrentTopSpeed()
		{
			if (holding && stats.current.useHoldingTopSpeed)
				return stats.current.holdingTopSpeed;

			return running ? stats.current.runningTopSpeed : stats.current.topSpeed;
		}

		/// <summary>
		/// Moves the Player smoothly in a given direction.
		/// </summary>
		/// <param name="direction">The direction you want to move.</param>
		/// <param name="magnitude">The magnitude of the movement.</param>
		public virtual void Accelerate(Vector3 direction, float magnitude = 1f)
		{
			var turningDrag = running
				? stats.current.runningTurningDrag
				: stats.current.turningDrag;
			var acceleration = running
				? stats.current.runningAcceleration
				: stats.current.acceleration;
			var finalTurningDrag = isGrounded ? turningDrag : stats.current.airTurningDrag;
			var finalAcceleration = isGrounded ? acceleration : stats.current.airAcceleration;

			Accelerate(
				direction,
				finalTurningDrag,
				finalAcceleration,
				GetCurrentTopSpeed(),
				stats.current.minTopSpeed,
				stats.current.applyInputMagnitude && !running ? magnitude : 1f
			);
		}

		/// <summary>
		/// Moves the Player smoothly in the input direction relative to the camera.
		/// </summary>
		public virtual void AccelerateToInputDirection()
		{
			var inputDirection = inputs.GetMovementCameraDirection(out float magnitude);
			Accelerate(inputDirection, magnitude);
		}

		/// <summary>
		/// Handles the Roll action transition.
		/// </summary>
		public virtual void Roll()
		{
			if (
				stats.current.canRoll
				&& !holding
				&& inputs.GetRoll()
				&& (
					isGrounded && lateralVelocity.magnitude > stats.current.minSpeedToRoll
					|| stats.current.canRollOnAir
				)
			)
				states.Change<RollingPlayerState>();
		}

		/// <summary>
		/// Handles the Roll Charge action transition.
		/// </summary>
		public virtual void RollCharge()
		{
			if (
				stats.current.canRollCharge
				&& !holding
				&& isGrounded
				&& inputs.GetRollCharge()
				&& (
					!stats.current.rollChargeRequiresCrouch
					|| states.IsCurrentOfType(typeof(CrouchPlayerState))
				)
				&& lateralVelocity.magnitude < stats.current.maxSpeedToRollCharge
			)
				states.Change<RollChargePlayerState>();
		}

		/// <summary>
		/// Applies the standard slope factor to the Player.
		/// </summary>
		public virtual void RegularSlopeFactor()
		{
			if (stats.current.applySlopeFactor)
				SlopeFactor(stats.current.slopeUpwardForce, stats.current.slopeDownwardForce);
		}

		/// <summary>
		/// Moves the Player smoothly in a given direction with water stats.
		/// </summary>
		/// <param name="direction">The direction you want to move.</param>
		public virtual void WaterAcceleration(Vector3 direction) =>
			Accelerate(
				direction,
				stats.current.waterTurningDrag,
				stats.current.swimAcceleration,
				stats.current.swimTopSpeed
			);

		/// <summary>
		/// Moves the Player smoothly in a given direction with crawling stats.
		/// </summary>
		/// <param name="direction">The direction you want to move.</param>
		public virtual void CrawlingAccelerate(Vector3 direction) =>
			Accelerate(
				direction,
				stats.current.crawlingTurningSpeed,
				stats.current.crawlingAcceleration,
				stats.current.crawlingTopSpeed
			);

		/// <summary>
		/// Moves the Player smoothly using the backflip stats.
		/// </summary>
		public virtual void BackflipAcceleration()
		{
			var direction = inputs.GetMovementCameraDirection();
			Accelerate(
				direction,
				stats.current.backflipTurningDrag,
				stats.current.backflipAirAcceleration,
				stats.current.backflipTopSpeed
			);
		}

		/// <summary>
		/// Smoothly sets Lateral Velocity to zero by its deceleration stats.
		/// </summary>
		public virtual void Decelerate() => Decelerate(stats.current.deceleration);

		/// <summary>
		/// Smoothly decelerates the lateral velocity to the current target top speed.
		/// </summary>
		public virtual void DecelerateToTopSpeed()
		{
			if (!stats.current.decelerateWhenOverTopSpeed)
				return;

			var delta = stats.current.decelerationToTopSpeed * Time.deltaTime;
			var speed = lateralVelocity.magnitude;

			if (speed <= targetTopSpeed || !isGrounded)
				return;

			var moveDirection = lateralVelocity / speed;
			speed = Mathf.MoveTowards(speed, targetTopSpeed, delta);
			lateralVelocity = moveDirection * speed;
		}

		/// <summary>
		/// Smoothly sets Lateral Velocity to zero by its friction stats.
		/// </summary>
		public virtual void Friction()
		{
			if (!inputs.lockedMovementDirection)
				Decelerate(stats.current.friction);
		}

		/// <summary>
		/// Applies a downward force by its gravity stats.
		/// </summary>
		public virtual void Gravity()
		{
			if (!isGrounded && Time.time < m_lockGravityTime)
			{
				verticalVelocity = Vector3.zero;
				return;
			}

			if (!isGrounded && verticalVelocity.y > -stats.current.gravityTopSpeed)
			{
				var speed = verticalVelocity.y;
				var force =
					verticalVelocity.y > 0 ? stats.current.gravity : stats.current.fallGravity;
				speed -= force * gravityMultiplier * Time.deltaTime;
				speed = Mathf.Max(speed, -stats.current.gravityTopSpeed);
				verticalVelocity = new Vector3(0, speed, 0);
			}
		}

		/// <summary>
		/// Applies a downward force when ground by its snap stats.
		/// </summary>
		public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);

		/// <summary>
		/// Rotate the Player forward to a given direction.
		/// </summary>
		/// <param name="direction">The direction you want it to face.</param>
		public virtual void FaceDirectionSmooth(Vector3 direction) =>
			FaceDirection(direction, stats.current.rotationSpeed);

		/// <summary>
		/// Rotates the Player forward to a given direction with water stats.
		/// </summary>
		/// <param name="direction">The direction you want it to face.</param>
		public virtual void WaterFaceDirection(Vector3 direction) =>
			FaceDirection(direction, stats.current.waterRotationSpeed);

		/// <summary>
		/// Makes a transition to the Fall State if the Player is not grounded.
		/// </summary>
		public virtual void Fall()
		{
			if (
				!isGrounded
				&& (
					stats.current.unrollWhenFalling
					|| !states.IsCurrentOfType(typeof(RollingPlayerState))
				)
			)
			{
				states.Change<FallPlayerState>();
			}
		}

		/// <summary>
		/// Handles ground jump with proper evaluations and height control.
		/// </summary>
		public virtual void Jump()
		{
			if (!stats.current.canJump)
				return;

			var canMultiJump = (jumpCounter > 0) && (jumpCounter < stats.current.multiJumps);
			var canCoyoteJump =
				(jumpCounter == 0)
				&& (Time.time < lastGroundTime + stats.current.coyoteJumpThreshold);
			var holdJump = !holding || stats.current.canJumpWhileHolding;

			if ((isGrounded || onRails || canMultiJump || canCoyoteJump) && holdJump)
			{
				if (inputs.GetJumpDown())
				{
					jumpedFromGround = isGrounded || onRails || canCoyoteJump;
					Jump(stats.current.maxJumpHeight);
				}
			}

			if (
				inputs.GetJumpUp()
				&& jumpedFromGround
				&& (verticalVelocity.y > stats.current.minJumpHeight)
			)
			{
				verticalVelocity = Vector3.up * stats.current.minJumpHeight;
			}
		}

		/// <summary>
		/// Applies an upward force to the Player.
		/// </summary>
		/// <param name="height">The force you want to apply.</param>
		public virtual void Jump(float height)
		{
			jumpCounter++;
			verticalVelocity = Vector3.up * height;
			states.Change<FallPlayerState>();
			playerEvents.OnJump?.Invoke();
		}

		/// <summary>
		/// Applies jump force to the Player in a given direction.
		/// </summary>
		/// <param name="direction">The direction that you want to jump.</param>
		/// <param name="height">The upward force that you want to apply.</param>
		/// <param name="distance">The force towards the direction that you want to apply.</param>
		public virtual void DirectionalJump(Vector3 direction, float height, float distance)
		{
			jumpCounter++;
			verticalVelocity = Vector3.up * height;
			lateralVelocity = direction * distance;
			playerEvents.OnJump?.Invoke();
		}

		/// <summary>
		/// Sets the air dash counter to zero.
		/// </summary>
		public virtual void ResetAirDash() => airDashCounter = 0;

		/// <summary>
		/// Resets the jump counter to zero.
		/// </summary>
		public virtual void ResetJumps()
		{
			jumpCounter = 0;
			jumpedFromGround = false;
		}

		/// <summary>
		/// Sets the jump counter to a specific value.
		/// </summary>
		/// <param name="amount">The amount of jumps.</param>
		public virtual void SetJumps(int amount) => jumpCounter = amount;

		/// <summary>
		/// Sets the air spin counter back to zero.
		/// </summary>
		public virtual void ResetAirSpins() => airSpinCounter = 0;

		/// <summary>
		/// Uncurls the Player from the Rolling state if the Roll input is not being pressed.
		/// </summary>
		/// <param name="forced">If true, forces the Uncurl action regardless of input.</param>
		public virtual void Uncurl(bool forced = false)
		{
			if (states.IsCurrentOfType(typeof(RollingPlayerState)) && (forced || !inputs.GetRoll()))
			{
				if (isGrounded)
					states.Change<IdlePlayerState>();
				else
					states.Change<FallPlayerState>();
			}
		}

		/// <summary>
		/// Handles the Crouch state transition.
		/// </summary>
		public virtual void Crouch()
		{
			if (
				stats.current.canCrouch
				&& !holding
				&& isGrounded
				&& inputs.GetCrouch()
				&& (stats.current.canCrouchSlide || lateralVelocity.sqrMagnitude.Equals(0))
			)
				states.Change<CrouchPlayerState>();
		}

		public virtual void Spin()
		{
			var canAirSpin =
				(isGrounded || stats.current.canAirSpin)
				&& airSpinCounter < stats.current.allowedAirSpins;

			if (stats.current.canSpin && canAirSpin && !holding && inputs.GetSpinDown())
			{
				if (!isGrounded)
				{
					airSpinCounter++;
				}

				states.Change<SpinPlayerState>();
				playerEvents.OnSpin?.Invoke();
			}
		}

		public virtual void PickAndThrow() => grabber?.HandleGrabbing();

		public virtual void AirDive()
		{
			if (stats.current.canAirDive && !isGrounded && !holding && inputs.GetAirDiveDown())
			{
				states.Change<AirDivePlayerState>();
				playerEvents.OnAirDive?.Invoke();
			}
		}

		public virtual void StompAttack()
		{
			if (!isGrounded && !holding && stats.current.canStompAttack && inputs.GetStompDown())
			{
				states.Change<StompPlayerState>();
			}
		}

		public virtual void LedgeGrab()
		{
			if (
				stats.current.canLedgeHang
				&& verticalVelocity.y < 0
				&& !holding
				&& states.ContainsStateOfType(typeof(LedgeHangingPlayerState))
				&& DetectingLedge(
					stats.current.ledgeMaxForwardDistance,
					stats.current.ledgeMaxDownwardDistance,
					out var hit
				)
			)
			{
				if (Vector3.Angle(hit.normal, transform.up) > 0)
					return;
				if (hit.collider is CapsuleCollider || hit.collider is SphereCollider)
					return;

				var ledgeDistance = radius + stats.current.ledgeMaxForwardDistance;
				var lateralOffset = transform.forward * ledgeDistance;
				var verticalOffset = -transform.up * height * 0.5f;
				ledge = hit.collider;
				velocity = Vector3.zero;
				position = hit.point - lateralOffset + verticalOffset;
				HandlePlatform(hit.collider);
				states.Change<LedgeHangingPlayerState>();
				playerEvents.OnLedgeGrabbed?.Invoke();
			}
		}

		public virtual void Backflip(float force)
		{
			if (stats.current.canBackflip && !holding)
			{
				verticalVelocity = Vector3.up * stats.current.backflipJumpHeight;
				lateralVelocity = -localForward * force;
				states.Change<BackflipPlayerState>();
				playerEvents.OnBackflip.Invoke();
			}
		}

		public virtual void Dash()
		{
			var canAirDash =
				stats.current.canAirDash
				&& !isGrounded
				&& airDashCounter < stats.current.allowedAirDashes;
			var canGroundDash =
				stats.current.canGroundDash
				&& isGrounded
				&& Time.time - lastDashTime > stats.current.groundDashCoolDown;

			if (inputs.GetDashDown() && (canAirDash || canGroundDash))
			{
				if (!isGrounded)
					airDashCounter++;

				lastDashTime = Time.time;
				states.Change<DashPlayerState>();
			}
		}

		public virtual void Glide()
		{
			if (
				!isGrounded
				&& !holding
				&& inputs.GetGlide()
				&& verticalVelocity.y <= 0
				&& stats.current.canGlide
			)
				states.Change<GlidingPlayerState>();
		}

		/// <summary>
		/// Sets the Skin parent to a given transform.
		/// </summary>
		/// <param name="parent">The transform you want to parent the skin to.</param>
		public virtual void SetSkinParent(Transform parent, Vector3 offset = default)
		{
			if (!skin)
				return;

			skin.parent = parent;
			skin.position += transform.rotation * offset;
		}

		/// <summary>
		/// Resets the Skin parenting to its initial one, with original position and rotation.
		/// </summary>
		public virtual void ResetSkinParent()
		{
			if (!skin)
				return;

			skin.parent = transform;
			skin.localPosition = m_skinInitialPosition;
			skin.localRotation = m_skinInitialRotation;
		}

		/// <summary>
		/// Makes the Player face the current lateral velocity direction when on a Spline Path.
		/// </summary>
		public virtual void FaceVelocityInPaths()
		{
			if (splinePath)
				FaceDirection(lateralVelocity);
		}

		protected virtual void AdjustToPath()
		{
			if (!IsSideScroller || !splinePath || onRails)
				return;

			var pathDirection = m_closestPointOnPath - position;
			var adjustDelta = stats.current.snapToPathForce * Time.deltaTime;

			pathDirection -= Vector3.Dot(pathDirection, transform.up) * transform.up;
			pathDirection -= Vector3.Dot(pathDirection, pathForward) * pathForward;
			position = Vector3.MoveTowards(position, position + pathDirection, adjustDelta);
		}

		protected virtual void ComputeFallDamage()
		{
			if (!isAlive || !stats.current.canTakeFallDamage)
				return;

			var minFallDuration = stats.current.minFallDurationToTakeDamage;
			var minFallSpeed = stats.current.minFallSpeedToTakeDamage;
			var damageIncreaseInterval = stats.current.fallDamageInterval;
			var landingSpeed = Mathf.Abs(verticalVelocity.y);

			if (fallDuration <= minFallDuration || landingSpeed < minFallSpeed)
				return;

			var exceedingTime = fallDuration - minFallDuration;
			var damageMultiplier = Mathf.FloorToInt(exceedingTime / damageIncreaseInterval);
			var additionalDamage = stats.current.additionalFallDamage * damageMultiplier;
			var damage = stats.current.baseFallDamage + additionalDamage;

			lateralVelocity = Vector3.zero;
			ApplyDamage(damage, position);
		}

		public virtual void WallDrag(Collider other)
		{
			if (holding || !stats.current.canWallDrag || verticalVelocity.y > 0)
				return;

			var maxWallDistance = radius + stats.current.ledgeMaxForwardDistance;
			var minGroundDistance = height * 0.5f + stats.current.minGroundDistanceToDrag;

			var detectingLedge = DetectingLedge(maxWallDistance, height, out _);
			var detectingWall = SphereCast(
				transform.forward,
				maxWallDistance,
				out var hit,
				stats.current.wallDragLayers
			);
			var detectingGround = DetectingGround(minGroundDistance, out var groundHit);
			var wallAngle = Vector3.Angle(transform.up, hit.normal);
			var groundAngle = Vector3.Angle(transform.up, groundHit.normal);

			if (
				!detectingWall
				|| (detectingGround && groundAngle < controller.slopeLimit)
				|| detectingLedge
				|| wallAngle < stats.current.minWallAngleToDrag
			)
				return;

			HandlePlatform(hit.collider);
			lastWallNormal = hit.normal;
			states.Change<WallDragPlayerState>();
		}

		public virtual void WallRun()
		{
			if (
				holding
				|| !stats.current.canWallRun
				|| verticalVelocity.y > 0
				|| verticalVelocity.y < stats.current.wallRunMaxFallSpeed
				|| lateralVelocity.magnitude < stats.current.wallRunMinSpeed
			)
				return;

			var maxWallDistance = radius + 1f;
			var rightWall = SphereCast(
				transform.right,
				maxWallDistance,
				out var rightHit,
				stats.current.wallRunLayer
			);
			var leftWall = SphereCast(
				-transform.right,
				maxWallDistance,
				out var leftHit,
				stats.current.wallRunLayer
			);

			if (!rightWall && !leftWall)
				return;

			var worldUp = CurrentWorldUp();
			var normal = rightWall ? rightHit.normal : leftHit.normal;
			var wallAngle = Vector3.Angle(worldUp, normal);

			var minGroundDistance = height * 0.5f + stats.current.wallRunMinGroundDistance;
			var detectingGround = DetectingGround(minGroundDistance, out _);

			if (detectingGround || wallAngle < 60 || wallAngle > 120)
				return;

			lastWallNormal = normal;
			states.Change<WallRunPlayerState>();
		}

		public virtual bool DetectingLedge(
			float forwardDistance,
			float downwardDistance,
			out RaycastHit ledgeHit
		)
		{
			var contactOffset = Physics.defaultContactOffset + positionDelta;
			var ledgeMaxDistance = radius + forwardDistance;
			var ledgeHeightOffset = height * 0.5f + contactOffset;
			var upwardOffset = transform.up * ledgeHeightOffset;
			var forwardOffset = transform.forward * ledgeMaxDistance;

			if (
				Physics.Raycast(
					position + upwardOffset,
					transform.forward,
					ledgeMaxDistance,
					Physics.DefaultRaycastLayers,
					QueryTriggerInteraction.Ignore
				)
				|| Physics.Raycast(
					position + forwardOffset * .01f,
					transform.up,
					ledgeHeightOffset,
					Physics.DefaultRaycastLayers,
					QueryTriggerInteraction.Ignore
				)
			)
			{
				ledgeHit = new RaycastHit();
				return false;
			}

			var origin = position + upwardOffset + forwardOffset;
			var distance = downwardDistance + contactOffset;

			return Physics.Raycast(
				origin,
				-transform.up,
				out ledgeHit,
				distance,
				stats.current.ledgeHangingLayers,
				QueryTriggerInteraction.Ignore
			);
		}

		public virtual bool DetectingGround(float minGroundDistance, out RaycastHit hit)
		{
			return Physics.Raycast(
					position,
					-transform.up,
					out hit,
					minGroundDistance,
					controller.collisionLayer,
					QueryTriggerInteraction.Ignore
				) || SphereCast(-transform.up, minGroundDistance, out hit);
		}

		public virtual void HandleRail()
		{
			if (stats.current.canRailGrind)
				states.Change<RailGrindPlayerState>();
		}

		protected virtual void HandleWaterCollision(Collider other)
		{
			if (!other.CompareTag(GameTags.VolumeWater))
				return;

			if (GravityHelper.IsPointInsideField(other, unsizedPosition))
				EnterWater(other);
			else if (onWater)
			{
				var exitPoint = position - transform.up * k_waterExitOffset;

				if (!GravityHelper.IsPointInsideField(other, exitPoint))
					ExitWater();
			}
		}

		protected virtual void HandleWaterExitCollision(Collider other)
		{
			if (other.CompareTag(GameTags.VolumeWater))
				ExitWater();
		}

		protected virtual void HandleEnemyCollision(Collider other)
		{
			if (!GameTags.IsEnemy(other))
				return;

			if (other.TryGetComponent(out Enemy enemy))
				(states as PlayerStateManager).OnEnemyContact(enemy);
		}

		protected override void Awake()
		{
			base.Awake();
			InitializeInputs();
			InitializeStats();
			InitializeHealth();
			InitializeGrabber();
			InitializeTag();
			InitializeRespawn();
			InitializePathForward();

			entityEvents.OnGroundEnter.AddListener(() =>
			{
				ResetJumps();
				ResetAirSpins();
				ResetAirDash();
				Uncurl();
				ComputeFallDamage();
				ClearHomingTarget();
			});

			entityEvents.OnRailsEnter.AddListener(() =>
			{
				ResetJumps();
				ResetAirSpins();
				ResetAirDash();
				ClearHomingTarget();
				HandleRail();
			});

			entityEvents.OnGroundFall.AddListener(() =>
			{
				inputs.LockMovementDirection();
			});
		}

		protected virtual void OnTriggerStay(Collider other)
		{
			HandleWaterCollision(other);
		}

		protected virtual void OnTriggerExit(Collider other)
		{
			HandleWaterExitCollision(other);
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			HandleEnemyCollision(other);
		}
	}
}
