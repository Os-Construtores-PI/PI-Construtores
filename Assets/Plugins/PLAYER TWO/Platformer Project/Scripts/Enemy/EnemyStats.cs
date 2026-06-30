using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[CreateAssetMenu(
		fileName = "NewEnemyStats",
		menuName = "PLAYER TWO/Platformer Project/Enemy/New Enemy Stats"
	)]
	public class EnemyStats : EntityStats<EnemyStats>
	{
		[Header("General Stats")]
		[Tooltip("If true, the Enemy will be affected by gravity.")]
		public bool applyGravity = true;

		[Tooltip("Downward force applied to the Enemy when in the air.")]
		public float gravity = 35f;

		[Tooltip("Downward force applied to the Enemy when in on the ground.")]
		public float snapForce = 15f;

		[Tooltip(
			"The speed in the degrees per second that the Enemy rotates towards its movement direction."
		)]
		public float rotationSpeed = 970f;

		[Tooltip(
			"Deceleration speed in units per second applied to the Enemy when it's stopping its movement."
		)]
		public float deceleration = 28f;

		[Tooltip("The speed in units per second the Enemy decelerates when it's not moving.")]
		public float friction = 16f;

		[Tooltip(
			"Drag in units per second applied to the Enemy when redirecting its velocity towards the movement direction."
		)]
		public float turningDrag = 28f;

		[Header("Idle Stats")]
		[Tooltip("If true, the Enemy will face the target when idle.")]
		public bool idleFaceTarget;

		[Tooltip(
			"The speed in degrees per second that the Enemy rotates towards the target when idle."
		)]
		public float idleFacingRotationSpeed = 90f;

		[Header("Contact Attack Stats")]
		[Tooltip("If true, the Enemy will attack the Player on contact.")]
		public bool canAttackOnContact = true;

		[Tooltip("If true, the Enemy will be pushed back when it contacts the Player.")]
		public bool contactPushback = true;

		[Tooltip("The damage the Enemy deals to the Player on contact.")]
		public int contactDamage = 1;

		[Tooltip("The force applied backwards to the Enemy when it contacts the Player.")]
		public float contactPushBackForce = 18f;

		[Tooltip(
			"If the Player is not above the enemy, but higher than the enemy's head based on this value, the enemy will not deal damage."
		)]
		public float contactSteppingTolerance = 0.1f;

		[Header("View Stats")]
		[Tooltip("The distance the Enemy can detect the Player.")]
		public float spotRange = 5f;

		[Tooltip("The distance the Enemy can keep the Player in sight after detecting it.")]
		public float viewRange = 8f;

		[Header("Follow Stats")]
		[Tooltip("If true, the Enemy will follow the Player when it's in sight.")]
		public bool followTargetOnSight = true;

		[Tooltip(
			"If true, the Enemy will return to its last state when it loses sight of the Player."
		)]
		public bool returnToLastStateWhenLostTarget = true;

		[Tooltip("The acceleration of the Enemy when following the Player.")]
		public float followAcceleration = 10f;

		[Tooltip("The maximum speed the Enemy can reach when following the Player.")]
		public float followTopSpeed = 4f;

		[Tooltip("The delay before the Enemy returns to its last state.")]
		public float returnToLastStateDelay = 2f;

		[Header("Waypoint Stats")]
		[Tooltip(
			"If true, the Enemy will face towards the next waypoint, when it's following a path."
		)]
		public bool faceWaypoint = true;

		[Tooltip(
			"The minimum distance the Enemy needs to reach to consider it has reached a waypoint."
		)]
		public float waypointMinDistance = 0.5f;

		[Tooltip("The acceleration of the Enemy when following a path.")]
		public float waypointAcceleration = 10f;

		[Tooltip("The maximum speed the Enemy can reach when following a path.")]
		public float waypointTopSpeed = 2f;
	}
}
