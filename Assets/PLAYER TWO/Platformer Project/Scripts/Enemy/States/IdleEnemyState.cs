using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/States/Idle Enemy State")]
	public class IdleEnemyState : EnemyState
	{
		protected override void OnEnter(Enemy enemy) { }

		protected override void OnExit(Enemy enemy) { }

		protected override void OnStep(Enemy enemy)
		{
			enemy.Gravity();
			enemy.SnapToGround();
			enemy.Friction();

			HandleFacing(enemy);
		}

		public override void OnContact(Enemy enemy, Collider other) { }

		protected virtual void HandleFacing(Enemy enemy)
		{
			if (!enemy.stats.current.idleFaceTarget)
				return;

			if (!enemy.player)
				FaceInitialDirection(enemy);
			else
				FaceTarget(enemy);
		}

		protected virtual void FaceInitialDirection(Enemy enemy)
		{
			enemy.FaceDirection(enemy.originalForward, enemy.stats.current.idleFacingRotationSpeed);
		}

		protected virtual void FaceTarget(Enemy enemy)
		{
			var direction = enemy.player.position - enemy.position;
			direction -= enemy.transform.up * Vector3.Dot(direction, enemy.transform.up);
			enemy.FaceDirection(direction.normalized, enemy.stats.current.idleFacingRotationSpeed);
		}
	}
}
