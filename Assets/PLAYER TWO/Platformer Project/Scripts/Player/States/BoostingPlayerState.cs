using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	public class BoostingPlayerState : PlayerState
	{
		protected readonly float m_minDurationToFall = 0.5f;
		protected readonly float m_minimumMagnitude = 0.01f;
		protected readonly float m_minBoostingSpeed = 1f;

		protected readonly float m_wallOffset = 0.1f;

		protected override void OnEnter(Player player)
		{
			player.SetSkinParent(null);
		}

		protected override void OnExit(Player player)
		{
			player.ResetSkinParent();
		}

		protected override void OnStep(Player player)
		{
			player.Gravity();
			player.SnapToGround();
			player.FaceDirectionSmooth(player.lateralVelocity);
			player.AccelerateToInputDirection();

			var speed = player.velocity.magnitude;
			var targetDirection = player.velocity / speed;
			var upwardFactor = Vector3.Dot(targetDirection, player.transform.up);
			var isFalling = upwardFactor < 0 && timeSinceEntered > m_minDurationToFall;

			if (isFalling)
			{
				targetDirection = player.transform.forward;
				HandleAirAbilities(player);
			}

			HandleSkinRotation(player, targetDirection);
			HandleSkinPosition(player);
			HandleGroundTransition(player);
			HandleFrontalCollision(player);

			if (speed < m_minBoostingSpeed)
				player.states.Change<FallPlayerState>();
		}

		public override void OnContact(Player player, Collider other)
		{
			player.WallRun();
		}

		protected virtual void HandleAirAbilities(Player player)
		{
			player.Jump();
			player.Spin();
			player.PickAndThrow();
			player.AirDive();
			player.StompAttack();
			player.LedgeGrab();
			player.Dash();
			player.Glide();
		}

		protected virtual void HandleSkinRotation(Player player, Vector3 targetDirection)
		{
			if (targetDirection.sqrMagnitude <= m_minimumMagnitude)
				return;

			var pitchAngle = Vector3.Dot(targetDirection, player.transform.up) * 90;
			var pitchRotation = Quaternion.AngleAxis(-pitchAngle, Vector3.right);
			var yawRotation = Quaternion.LookRotation(
				player.transform.forward,
				player.transform.up
			);

			player.skin.rotation = yawRotation * pitchRotation;
		}

		protected virtual void HandleSkinPosition(Player player)
		{
			player.skin.position = player.transform.position;
		}

		protected virtual void HandleGroundTransition(Player player)
		{
			if (player.isGrounded)
				player.states.Change<WalkPlayerState>();
		}

		protected virtual void HandleFrontalCollision(Player player)
		{
			var faceCollision = player.SphereCast(
				player.transform.forward,
				player.radius + m_wallOffset
			);

			if (faceCollision)
				player.states.Change<FallPlayerState>();
		}
	}
}
