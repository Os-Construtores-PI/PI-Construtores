using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	public class WallRunPlayerState : PlayerState
	{
		protected Vector3 m_wallDirection;
		protected Vector3 m_skinOffset;

		protected readonly float m_collisionOffset = 0.1f;

		protected override void OnEnter(Player player)
		{
			var wallDot = Vector3.Dot(player.transform.right, player.lastWallNormal);
			var speed = Mathf.Max(player.velocity.magnitude, player.stats.current.wallRunBaseSpeed);
			var faceDirection = Vector3.ProjectOnPlane(
				player.transform.forward,
				player.lastWallNormal
			);

			faceDirection -= Vector3.Dot(faceDirection, player.transform.right) * faceDirection;
			faceDirection.Normalize();

			m_skinOffset = player.stats.current.wallRunSkinOffset;
			m_skinOffset.x *= wallDot < 0 ? 1 : -1;

			m_wallDirection = wallDot < 0 ? player.transform.right : -player.transform.right;

			player.skin.position += player.transform.rotation * m_skinOffset;
			player.FaceDirection(faceDirection, Space.World);
			player.lateralVelocity = player.verticalVelocity = Vector3.zero;
			player.velocity = faceDirection * speed;

			player.ResetJumps();
			player.ResetAirSpins();
			player.ResetAirDash();
		}

		protected override void OnExit(Player player)
		{
			player.skin.position -= player.transform.rotation * m_skinOffset;
		}

		protected override void OnStep(Player player)
		{
			var minGroundDistance =
				player.height * 0.5f + player.stats.current.wallRunMinGroundDistance;
			var collisionRadius = player.radius + m_collisionOffset;

			var onWall = player.SphereCast(
				m_wallDirection,
				collisionRadius,
				out var hit,
				player.stats.current.wallRunLayer
			);

			var faceCollision = player.SphereCast(player.transform.forward, collisionRadius);
			var detectingGround = player.DetectingGround(minGroundDistance, out _);

			if (
				!onWall
				|| faceCollision
				|| detectingGround
				|| player.verticalVelocity.y < player.stats.current.wallRunMaxFallSpeed
				|| player.velocity.magnitude < player.stats.current.wallRunMinSpeed
			)
			{
				player.states.Change<FallPlayerState>();
				return;
			}

			player.lastWallNormal = hit.normal;

			HandleWallVelocity(player, hit.normal);
			HandleFriction(player);
			HandleGravity(player);
			HandleWallJump(player, hit.normal);
			HandleGroundTransition(player);
		}

		public override void OnContact(Player player, Collider other) { }

		protected virtual void HandleWallVelocity(Player player, Vector3 wallNormal)
		{
			player.velocity = Vector3.ProjectOnPlane(player.velocity, wallNormal);
		}

		protected virtual void HandleFriction(Player player)
		{
			var deceleration = player.stats.current.wallRunFriction * Time.deltaTime;
			player.lateralVelocity = Vector3.MoveTowards(
				player.lateralVelocity,
				Vector3.zero,
				deceleration
			);
		}

		protected virtual void HandleGravity(Player player)
		{
			if (timeSinceEntered < player.stats.current.wallRunGravityDelay)
				return;

			player.Gravity(player.stats.current.wallRunGravity);
		}

		protected virtual void HandleWallJump(Player player, Vector3 wallNormal)
		{
			if (!player.inputs.GetJumpDown())
				return;

			var worldToLocal = Quaternion.FromToRotation(player.transform.up, Vector3.up);
			var localWallDirection = worldToLocal * wallNormal;
			var localForwardDirection = worldToLocal * player.transform.forward;
			var jumpDirection = (localWallDirection + localForwardDirection).normalized;

			var speed = player.velocity.magnitude;
			var jumpSpeed = Mathf.Max(speed, player.stats.current.wallRunJumpBaseForce);

			player.LockGravity(player.stats.current.wallRunJumpGravityDelay);
			player.inputs.LockMovementDirection();
			player.DirectionalJump(jumpDirection, 0, jumpSpeed);
			player.states.Change<BoostingPlayerState>();
		}

		protected virtual void HandleGroundTransition(Player player)
		{
			if (player.isGrounded)
				player.states.Change<IdlePlayerState>();
		}
	}
}
