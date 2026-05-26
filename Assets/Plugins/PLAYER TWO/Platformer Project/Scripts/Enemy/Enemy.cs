using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(EnemyStatsManager))]
	[RequireComponent(typeof(EnemyStateManager))]
	[RequireComponent(typeof(WaypointManager))]
	[RequireComponent(typeof(Health))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/Enemy")]
	public class Enemy : Entity<Enemy>
	{
		[Header("Enemy Settings")]
		public EnemyEvents enemyEvents;

		protected Player m_player;

		protected Collider[] m_sightOverlaps = new Collider[1024];
		protected Collider[] m_contactAttackOverlaps = new Collider[1024];

		/// <summary>
		/// Returns the Enemy Stats Manager instance.
		/// </summary>
		public EnemyStatsManager stats { get; protected set; }

		/// <summary>
		/// Returns the Waypoint Manager instance.
		/// </summary>
		public WaypointManager waypoints { get; protected set; }

		/// <summary>
		/// Returns the Health instance.
		/// </summary>
		public Health health { get; protected set; }

		/// <summary>
		/// Returns the instance of the Player on the Enemies sight.
		/// </summary>
		public Player player { get; protected set; }

		protected virtual void InitializeStatsManager() =>
			stats = GetComponent<EnemyStatsManager>();

		protected virtual void InitializeWaypointsManager() =>
			waypoints = GetComponent<WaypointManager>();

		protected virtual void InitializeHealth() => health = GetComponent<Health>();

		protected virtual void InitializeTag() => tag = GameTags.Enemy;

		/// <summary>
		/// Applies damage to this Enemy decreasing its health with proper reaction.
		/// </summary>
		/// <param name="amount">The amount of health you want to decrease.</param>
		public override void ApplyDamage(int amount, Vector3 origin)
		{
			if (!health.isEmpty && !health.recovering)
			{
				health.Damage(amount);
				enemyEvents.OnDamage?.Invoke();

				if (health.isEmpty)
				{
					controller.enabled = false;
					enemyEvents.OnDie?.Invoke();
				}
			}
		}

		/// <summary>
		/// Revives this enemy, restoring its health and reenabling its movements.
		/// </summary>
		public virtual void Revive()
		{
			if (!health.isEmpty)
				return;

			health.ResetHealth();
			controller.enabled = true;
			enemyEvents.OnRevive.Invoke();
		}

		/// <summary>
		/// Respawns this enemy to its original position and rotation, restoring its health.
		/// </summary>
		public virtual void Respawn()
		{
			gravityField = null;
			velocity = Vector3.zero;
			health.ResetHealth();
			transform.SetPositionAndRotation(originalPosition, originalRotation);
			controller.enabled = true;
			states.Reset();
			enemyEvents.OnRespawn.Invoke();
		}

		public virtual void Accelerate(Vector3 direction, float acceleration, float topSpeed) =>
			Accelerate(direction, stats.current.turningDrag, acceleration, topSpeed);

		/// <summary>
		/// Smoothly sets Lateral Velocity to zero by its deceleration stats.
		/// </summary>
		public virtual void Decelerate() => Decelerate(stats.current.deceleration);

		/// <summary>
		/// Smoothly sets Lateral Velocity to zero by its friction stats.
		/// </summary>
		public virtual void Friction() => Decelerate(stats.current.friction);

		/// <summary>
		/// Applies a downward force by its gravity stats.
		/// </summary>
		public virtual void Gravity()
		{
			if (stats.current.applyGravity)
				Gravity(stats.current.gravity);
		}

		/// <summary>
		/// Applies a downward force when ground by its snap stats.
		/// </summary>
		public virtual void SnapToGround() => SnapToGround(stats.current.snapForce);

		/// <summary>
		/// Rotate the Enemy forward to a given direction.
		/// </summary>
		/// <param name="direction">The direction you want it to face.</param>
		public virtual void FaceDirectionSmooth(Vector3 direction) =>
			FaceDirection(direction, stats.current.rotationSpeed);

		public virtual void ContactAttack(Collider other)
		{
			if (health.isEmpty || !other.CompareTag(GameTags.Player))
				return;

			if (!other.TryGetComponent(out Player player))
				return;

			var stepping =
				controller.bounds.max + Vector3.down * stats.current.contactSteppingTolerance;

			if (player.isGrounded || !BoundsHelper.IsBellowPoint(controller.collider, stepping))
			{
				if (stats.current.contactPushback)
					lateralVelocity = -localForward * stats.current.contactPushBackForce;

				player.ApplyDamage(stats.current.contactDamage, transform.position);
				enemyEvents.OnPlayerContact?.Invoke();
			}
		}

		/// <summary>
		/// Handles the view sight and Player detection behaviour.
		/// </summary>
		protected virtual void HandleSight()
		{
			if (!player)
			{
				var overlaps = Physics.OverlapSphereNonAlloc(
					position,
					stats.current.spotRange,
					m_sightOverlaps
				);

				for (int i = 0; i < overlaps; i++)
				{
					if (m_sightOverlaps[i].CompareTag(GameTags.Player))
					{
						if (m_sightOverlaps[i].TryGetComponent<Player>(out var player))
						{
							this.player = player;
							OnPlayerSpotted();
							enemyEvents.OnPlayerSpotted?.Invoke();
							return;
						}
					}
				}
			}
			else
			{
				var distance = Vector3.Distance(position, player.position);

				if ((player.health.current == 0) || (distance > stats.current.viewRange))
				{
					player = null;
					enemyEvents.OnPlayerScaped?.Invoke();
				}
			}
		}

		protected virtual void OnPlayerSpotted()
		{
			if (stats.current.followTargetOnSight)
				states.Change<FollowEnemyState>();
		}

		protected override void OnUpdate()
		{
			HandleSight();
		}

		protected override void Awake()
		{
			base.Awake();
			InitializeTag();
			InitializeStatsManager();
			InitializeWaypointsManager();
			InitializeHealth();
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			ContactAttack(other);
		}
	}
}
