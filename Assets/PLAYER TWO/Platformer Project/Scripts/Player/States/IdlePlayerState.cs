using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Idle Player State")]
	public class IdlePlayerState : PlayerState
	{
		protected override void OnEnter(Player player) { }

		protected override void OnExit(Player player) { }

		protected override void OnStep(Player player)
		{
			player.Gravity();
			player.SnapToGround();
			player.Jump();
			player.Fall();
			player.Crouch();
			player.Spin();
			player.PickAndThrow();
			player.RegularSlopeFactor();
			player.Friction();

			var inputDirection = player.inputs.GetMovementCameraDirection();

			if (inputDirection.sqrMagnitude > 0 || player.lateralVelocity.sqrMagnitude > 0)
			{
				player.states.Change<WalkPlayerState>();
			}

			player.Dash();
			player.RollCharge();
		}

		public override void OnContact(Player player, Collider other) { }
	}
}
