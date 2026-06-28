using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(WaypointManager))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Platform/Moving Platform")]
	public class MovingPlatform : Platform
	{
		public enum RotationMode { Interpolate, LookAt }

		[Header("Movement Settings")]
		[Tooltip("The speed at which the platform moves between waypoints.")]
		public float speed = 3f;

		[Header("Rotation Settings")]
		[Tooltip("Whether or not the platform should rotate to face the next waypoint.")]
		public bool rotateToWaypoint;

		[Tooltip("The rotation mode to use when rotating to face the next waypoint.")]
		public RotationMode rotationMode;

		[Tooltip("The speed at which the platform rotates to face the next waypoint in the LookAt mode.")]
		public float lookAtSpeed = 360f;

		public WaypointManager waypoints { get; protected set; }

		protected const float k_minDistance = 0.001f;

		protected override void Awake()
		{
			base.Awake();
			waypoints = GetComponent<WaypointManager>();
		}

		public override void PlatformUpdate()
		{
			CacheTransform();
			HandleWaypoints();
			HandleAttachedTransforms();
		}

		protected virtual void HandleWaypoints()
		{
			var position = transform.position;
			var target = waypoints.current.position;

			position = Vector3.MoveTowards(position, target, speed * Time.deltaTime);
			transform.position = position;

			var distance = Vector3.Distance(transform.position, target);

			HandleRotation(distance);

			if (distance <= k_minDistance)
				waypoints.Next();
		}

		protected virtual void HandleRotation(float distance)
		{
			if (!rotateToWaypoint)
				return;

			var targetRotation = waypoints.current.rotation;
			var previousRotation = waypoints.previous.rotation;

			if (rotationMode == RotationMode.Interpolate)
			{
				var targetPosition = waypoints.current.position;
				var previousPosition = waypoints.previous.position;
				var t = 1f - (distance / Vector3.Distance(targetPosition, previousPosition));
				transform.rotation = Quaternion.Lerp(previousRotation, targetRotation, t);
			}
			else if (rotationMode == RotationMode.LookAt)
			{
				var rotation = transform.rotation;
				var rotationDelta = lookAtSpeed * Time.deltaTime;
				transform.rotation = Quaternion.RotateTowards(rotation, targetRotation, rotationDelta);
			}
		}
	}
}
