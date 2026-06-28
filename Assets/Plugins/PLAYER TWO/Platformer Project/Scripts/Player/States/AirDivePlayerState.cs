using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Air Dive Player State")]
	public class AirDivePlayerState : PlayerState
	{
		protected override void OnEnter(Player player)
		{
			player.verticalVelocity = Vector3.zero;
			player.lateralVelocity = player.localForward * player.stats.current.airDiveForwardForce;
		}

		protected override void OnExit(Player player) { }

		protected override void OnStep(Player player)
		{
			player.Gravity();
			player.Jump();

			if (player.stats.current.applyDiveSlopeFactor)
				player.SlopeFactor(
					player.stats.current.slopeUpwardForce,
					player.stats.current.slopeDownwardForce
				);

			player.FaceDirection(player.lateralVelocity);

			if (player.isGrounded)
			{
				var inputDirection = player.inputs.GetMovementCameraDirection();
				var localInputDirection = player.transform.InverseTransformDirection(
					inputDirection
				);
				var rotation =
					localInputDirection.x
					* player.stats.current.airDiveRotationSpeed
					* Time.deltaTime;

				player.lateralVelocity = Quaternion.Euler(0, rotation, 0) * player.lateralVelocity;

				player.Decelerate(player.stats.current.airDiveFriction);

				if (player.lateralVelocity.sqrMagnitude == 0)
				{
					player.verticalVelocity =
						Vector3.up * player.stats.current.airDiveGroundLeapHeight;
					player.states.Change<FallPlayerState>();
				}
			}
		}

		public override void OnContact(Player player, Collider other)
		{
			if (!player.isGrounded)
			{
				player.WallDrag(other);
				player.GrabPole(other);
			}
		}
	}
}
