using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/States/Waypoint Enemy State")]
	public class WaypointEnemyState : EnemyState
	{
		protected override void OnEnter(Enemy enemy) { }

		protected override void OnExit(Enemy enemy) { }

		protected override void OnStep(Enemy enemy)
		{
			enemy.Gravity();
			enemy.SnapToGround();

			var destination = enemy.waypoints.current.position;
			var head = destination - enemy.position;
			var upOffset = Vector3.Dot(enemy.transform.up, head);

			head -= enemy.transform.up * upOffset;

			var distance = head.magnitude;
			var direction = head / distance;
			var localDirection = Quaternion.FromToRotation(enemy.transform.up, Vector3.up) * direction;

			if (distance <= enemy.stats.current.waypointMinDistance)
			{
				enemy.Decelerate();
				enemy.waypoints.Next();
			}
			else
			{
				enemy.Accelerate(localDirection, enemy.stats.current.waypointAcceleration, enemy.stats.current.waypointTopSpeed);

				if (enemy.stats.current.faceWaypoint)
					enemy.FaceDirectionSmooth(localDirection);
			}
		}

		public override void OnContact(Enemy enemy, Collider other) { }
	}
}
