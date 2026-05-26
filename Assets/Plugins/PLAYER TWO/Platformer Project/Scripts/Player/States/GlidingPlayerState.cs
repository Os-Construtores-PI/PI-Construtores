using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	public class GlidingPlayerState : PlayerState
	{
		protected override void OnEnter(Player player)
		{
			player.verticalVelocity = Vector3.zero;
			player.FaceDirection(player.lateralVelocity);
			player.playerEvents.OnGlidingStart.Invoke();
		}

		protected override void OnExit(Player player) => player.playerEvents.OnGlidingStop.Invoke();

		protected override void OnStep(Player player)
		{
			HandleGlideAcceleration(player);
			HandleGlidingGravity(player);
			player.LedgeGrab();
			player.FaceVelocityInPaths();

			if (player.isGrounded)
			{
				player.states.Change<IdlePlayerState>();
			}
			else if (!player.inputs.GetGlide())
			{
				player.states.Change<FallPlayerState>();
			}
		}

		public override void OnContact(Player player, Collider other)
		{
			player.WallDrag(other);
			player.GrabPole(other);
		}

		protected virtual void HandleGlideAcceleration(Player player)
		{
			var inputDirection = player.inputs.GetMovementCameraDirection(out var inputMagnitude);

			player.FaceDirection(inputDirection, player.stats.current.glidingRotationSpeed);

			if (inputMagnitude > 0)
			{
				var acceleration = player.stats.current.airAcceleration * inputMagnitude;
				player.Accelerate(
					player.localForward,
					acceleration,
					player.stats.current.glidingTurningDrag,
					player.stats.current.topSpeed
				);
			}
		}

		protected virtual void HandleGlidingGravity(Player player)
		{
			var yVelocity = player.verticalVelocity.y;
			yVelocity -= player.stats.current.glidingGravity * Time.deltaTime;
			yVelocity = Mathf.Max(yVelocity, -player.stats.current.glidingMaxFallSpeed);
			player.verticalVelocity = new Vector3(0, yVelocity, 0);
		}
	}
}
