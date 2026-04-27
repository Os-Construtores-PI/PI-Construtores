using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Brake Player State")]
	public class BrakePlayerState : PlayerState
	{
		protected override void OnEnter(Player player) { }

		protected override void OnExit(Player player) { }

		protected override void OnStep(Player player)
		{
			var inputDirection = player.inputs.GetMovementCameraDirection(false);

			if (
				player.stats.current.canBackflip
				&& player.stats.current.canBackflipWhileTurning
				&& Vector3.Dot(inputDirection, player.transform.forward) < 0
				&& player.inputs.GetJumpDown()
			)
			{
				player.Backflip(player.stats.current.backflipBackwardTurnForce);
			}
			else
			{
				player.SnapToGround();
				player.Jump();
				player.Fall();
				player.Decelerate();

				if (player.lateralVelocity.sqrMagnitude == 0)
				{
					player.states.Change<IdlePlayerState>();
				}
			}
		}

		public override void OnContact(Player player, Collider other) { }
	}
}
