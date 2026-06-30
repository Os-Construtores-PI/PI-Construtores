using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	public class HomingDashTrickPlayerState : PlayerState
	{
		protected override void OnEnter(Player player)
		{
			player.canTakeDamage = false;
		}

		protected override void OnExit(Player player)
		{
			player.canTakeDamage = true;
		}

		protected override void OnStep(Player player)
		{
			if (timeSinceEntered > player.stats.current.homingTrickInvincibilityDuration)
				player.canTakeDamage = true;

			player.Gravity(player.stats.current.homingTrickGravity);
			player.SnapToGround();
			player.FaceDirectionSmooth(player.lateralVelocity);
			player.AccelerateToInputDirection();
			player.Roll();
			player.Jump();
			player.Spin();
			player.PickAndThrow();
			player.AirDive();
			player.StompAttack();
			player.LedgeGrab();
			player.Dash();
			player.Glide();
			player.HomingDash();

			if (player.isGrounded)
				player.states.Change<IdlePlayerState>();
		}

		public override void OnContact(Player player, Collider other) { }
	}
}
