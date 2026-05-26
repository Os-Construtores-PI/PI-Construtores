using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	public class WallDragPlayerState : PlayerState
	{
		protected override void OnEnter(Player player)
		{
			player.ResetJumps();
			player.ResetAirSpins();
			player.ResetAirDash();
			player.velocity = Vector3.zero;
			player.skin.position += player.transform.rotation * player.stats.current.wallDragSkinOffset;
			var faceDirection = player.lastWallNormal - player.transform.up * Vector3.Dot(player.lastWallNormal, player.transform.up);
			player.FaceDirection(faceDirection, Space.World);
		}

		protected override void OnExit(Player player)
		{
			player.skin.position -= player.transform.rotation * player.stats.current.wallDragSkinOffset;

			if (!player.isGrounded && player.platform)
				player.platform.Detach(player.skin);
		}

		protected override void OnStep(Player player)
		{
			HandleGravity(player);
			HandleGroundDetection(player);
			HandleWallJump(player);
		}

		public override void OnContact(Player player, Collider other) { }

		protected virtual void HandleGravity(Player player)
		{
			if (timeSinceEntered < player.stats.current.wallDragGravityDelay)
				return;

			var gravity = player.stats.current.wallDragGravity * Time.deltaTime;
			player.verticalVelocity += Vector3.down * gravity;
		}

		protected virtual void HandleGroundDetection(Player player)
		{
			var centerDistance = Vector3.Distance(player.position, player.transform.position);
			var maxWallDistance = player.radius + centerDistance + player.stats.current.ledgeMaxForwardDistance;
			var detectingWall = player.SphereCast(-player.transform.forward, maxWallDistance,
				player.stats.current.wallDragLayers);

			if (player.isGrounded || !detectingWall)
				player.states.Change<IdlePlayerState>();
		}

		protected virtual void HandleWallJump(Player player)
		{
			var jumpHeight = player.stats.current.wallJumpHeight;
			var jumpDistance = player.stats.current.wallJumpDistance;

			if (player.inputs.GetJumpDown())
			{
				if (player.stats.current.wallJumpLockMovement)
					player.inputs.LockMovementDirection();

				player.DirectionalJump(player.localForward, jumpHeight, jumpDistance);
				player.states.Change<FallPlayerState>();
			}
		}
	}
}
