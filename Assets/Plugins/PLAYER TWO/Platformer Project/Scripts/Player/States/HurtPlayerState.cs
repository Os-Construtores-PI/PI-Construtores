using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Hurt Player State")]
	public class HurtPlayerState : PlayerState
	{
		protected override void OnEnter(Player player)
		{
			FaceAwayDamageOrigin(player);
			ApplyDamagePushBack(player);
		}

		protected override void OnExit(Player player) { }

		protected override void OnStep(Player player)
		{
			ApplyGravity(player);
			ApplyWaterDrag(player);
			HandleGroundTransitions(player);
			HandleWaterTransitions(player);
			HandleStunRecover(player);
		}

		public override void OnContact(Player player, Collider other) { }

		protected virtual void FaceAwayDamageOrigin(Player player)
		{
			var up = player.transform.up;
			var head = player.lastDamageOrigin - player.position;
			var upOffset = Vector3.Dot(up, head);
			var damageDir = (head - up * upOffset).normalized;
			var localDamageDir = Quaternion.FromToRotation(up, Vector3.up) * damageDir;

			if (player.IsSideScroller)
			{
				var dot = Vector3.Dot(player.localForward, localDamageDir);
				localDamageDir = dot > 0 ? player.localForward : -player.localForward;
			}

			player.FaceDirection(localDamageDir);
		}

		protected virtual void ApplyDamagePushBack(Player player)
		{
			var verticalForce = player.stats.current.hurtUpwardForce;
			var lateralForce = player.onWater
				? player.stats.current.hurtBackwardsWaterForce
				: player.stats.current.hurtBackwardsForce;
			player.lateralVelocity = -player.localForward * lateralForce;

			if (!player.onWater)
				player.verticalVelocity = Vector3.up * verticalForce;
		}

		protected virtual void ApplyGravity(Player player)
		{
			if (player.onWater)
			{
				var force = player.stats.current.hurtDownwardsWaterForce;
				player.verticalVelocity = Vector3.down * force;
			}
			else
				player.Gravity();
		}

		protected virtual void ApplyWaterDrag(Player player)
		{
			if (!player.onWater)
				return;

			var delta = player.stats.current.hurtWaterDrag * Time.deltaTime;
			player.velocity = Vector3.MoveTowards(player.velocity, Vector3.zero, delta);
		}

		protected virtual void HandleGroundTransitions(Player player)
		{
			if (player.onWater || !player.isGrounded)
				return;

			if (player.health.current > 0)
				player.states.Change<IdlePlayerState>();
			else
				player.states.Change<DiePlayerState>();
		}

		protected virtual void HandleWaterTransitions(Player player)
		{
			if (!player.onWater)
				return;

			if (timeSinceEntered >= player.stats.current.hurtWaterCoolDown)
				player.states.Change<SwimPlayerState>();
		}

		protected virtual void HandleStunRecover(Player player)
		{
			if (timeSinceEntered >= player.stats.current.hurtStunRecoverTime && player.isAlive)
				player.states.Change<FallPlayerState>();
		}
	}
}
