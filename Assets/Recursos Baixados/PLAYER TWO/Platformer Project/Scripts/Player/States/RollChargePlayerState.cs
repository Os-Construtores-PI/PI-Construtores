using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	public class RollChargePlayerState : PlayerState
	{
		protected override void OnEnter(Player player)
		{
			player.lateralVelocity = Vector3.zero;
			player.ResizeCollider(player.stats.current.crouchHeight);
		}

		protected override void OnExit(Player player)
		{
			player.ResizeCollider(player.originalHeight);
		}

		protected override void OnStep(Player player)
		{
			player.Gravity();
			player.SnapToGround();
			player.Fall();

			var inputDirection = player.inputs.GetMovementCameraDirection();

			player.FaceDirectionSmooth(inputDirection);

			var factor = Mathf.Clamp01(timeSinceEntered / player.stats.current.rollChargeDuration);
			var force = Mathf.Lerp(
				player.stats.current.minChargeForce,
				player.stats.current.maxChargeForce,
				factor
			);

			if (!player.inputs.GetRollCharge())
			{
				player.lateralVelocity = player.localForward * force;
				player.states.Change<RollingPlayerState>();
			}
		}

		public override void OnContact(Player player, Collider other) { }
	}
}
