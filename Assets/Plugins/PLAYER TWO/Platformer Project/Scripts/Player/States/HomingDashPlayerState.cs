using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	public class HomingDashPlayerState : PlayerState
	{
		protected Vector3 m_initialTargetPosition;

		protected override void OnEnter(Player player)
		{
			player.canTakeDamage = false;
			m_initialTargetPosition = player.initialHomingTargetPosition;
		}

		protected override void OnExit(Player player)
		{
			player.canTakeDamage = true;
		}

		protected override void OnStep(Player player)
		{
			if (timeSinceEntered > player.stats.current.homingDashMaxDuration)
			{
				player.states.Change<FallPlayerState>();
				return;
			}

			var destination = GetHomingTargetPosition(player);
			var head = destination - player.position;
			var distance = head.magnitude;
			var direction = head / distance;

			player.velocity = direction * player.stats.current.homingDashForce;
			player.FaceDirection(player.lateralVelocity);

			if (player.homingSpline)
			{
				if (distance <= player.radius + 0.5f)
					player.EnterRail(player.homingSpline);
			}
			else if (distance <= 0.1f)
			{
				HandleRecover(player);
			}
		}

		public override void OnContact(Player player, Collider other) { }

		public override void OnEnemyContact(Player player, Enemy enemy)
		{
			player.transform.position -= player.transform.forward * Physics.defaultContactOffset;
			enemy.ApplyDamage(player.stats.current.homingDashDamage, player.position);
			HandleRecover(player);
		}

		protected virtual Vector3 GetHomingTargetPosition(Player player)
		{
			if (player.homingSpline)
				return m_initialTargetPosition;

			return player.GetHomingTargetPosition();
		}

		protected virtual void HandleRecover(Player player)
		{
			player.lateralVelocity = Vector3.zero;
			player.verticalVelocity = Vector3.up * player.stats.current.homingDashRecoverForce;
			player.states.Change<HomingDashTrickPlayerState>();
		}
	}
}
