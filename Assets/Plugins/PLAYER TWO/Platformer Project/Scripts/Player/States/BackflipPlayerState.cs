using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Backflip Player State")]
	public class BackflipPlayerState : PlayerState
	{
		protected override void OnEnter(Player player)
		{
			player.SetJumps(1);
			player.playerEvents.OnJump.Invoke();

			if (player.stats.current.backflipLockMovement)
			{
				player.inputs.LockMovementDirection();
			}
		}

		protected override void OnExit(Player player) { }

		protected override void OnStep(Player player)
		{
			player.Gravity(player.stats.current.backflipGravity);
			player.BackflipAcceleration();

			if (player.isGrounded)
			{
				player.lateralVelocity = Vector3.zero;
				player.states.Change<IdlePlayerState>();
			}
			else if (player.verticalVelocity.y < 0)
			{
				player.Spin();
				player.AirDive();
				player.StompAttack();
				player.Glide();
			}
		}

		public override void OnContact(Player player, Collider other)
		{
			player.WallDrag(other);
			player.GrabPole(other);
		}
	}
}
