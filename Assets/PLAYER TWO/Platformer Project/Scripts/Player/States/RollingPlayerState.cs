using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	public class RollingPlayerState : PlayerState
	{
		protected bool m_uncurl;

		protected readonly float m_minimumDuration = 0.5f;

		protected override void OnEnter(Player player)
		{
			m_uncurl = false;
			player.ResizeCollider(player.stats.current.crouchHeight);
			player.playerEvents.OnRollStarted?.Invoke();
		}

		protected override void OnExit(Player player)
		{
			player.ResizeCollider(player.originalHeight);
			player.playerEvents.OnRollEnded?.Invoke();
		}

		protected override void OnStep(Player player)
		{
			player.Gravity();
			player.SnapToGround();
			player.Jump();
			player.Fall();
			player.SlopeFactor(
				player.stats.current.rollingSlopeUpwardForce,
				player.stats.current.rollingSlopeDownwardForce
			);
			player.FaceDirectionSmooth(player.lateralVelocity);

			var inputDirection = player.inputs.GetMovementCameraDirection(out var magnitude);
			var forwardDot = Vector3.Dot(player.lateralVelocity, inputDirection);

			HandleGroundTurning(player, inputDirection, magnitude, forwardDot);
			HandleDeceleration(player, inputDirection, magnitude, forwardDot);
			HandleAirAcceleration(player, inputDirection, magnitude);
			HandleFriction(player);
			HandleUncurl(player);
		}

		public override void OnContact(Player player, Collider other) { }

		protected virtual void HandleGroundTurning(
			Player player,
			Vector3 inputDirection,
			float inputMagnitude,
			float forwardDot
		)
		{
			if (
				!player.isGrounded
				|| inputMagnitude <= 0
				|| forwardDot < player.stats.current.brakeThreshold
			)
				return;

			var targetVelocity = inputDirection * player.lateralVelocity.magnitude;
			var turningDelta =
				player.stats.current.rollingTurningDrag
				* player.turningDragMultiplier
				* Time.deltaTime;

			player.lateralVelocity = Vector3.MoveTowards(
				player.lateralVelocity,
				targetVelocity,
				turningDelta
			);
		}

		protected virtual void HandleDeceleration(
			Player player,
			Vector3 inputDirection,
			float inputMagnitude,
			float forwardDot
		)
		{
			if (
				!player.isGrounded
				|| inputMagnitude <= 0
				|| forwardDot >= player.stats.current.brakeThreshold
			)
				return;

			player.Decelerate(player.stats.current.rollingDeceleration);
		}

		protected virtual void HandleAirAcceleration(
			Player player,
			Vector3 inputDirection,
			float inputMagnitude
		)
		{
			if (player.isGrounded || inputMagnitude <= 0)
				return;

			player.Accelerate(inputDirection, inputMagnitude);
		}

		protected virtual void HandleFriction(Player player)
		{
			if (!player.isGrounded)
				return;

			player.Decelerate(player.stats.current.rollingFriction);
		}

		protected virtual void HandleUncurl(Player player)
		{
			if (player.inputs.GetCancelDown())
				m_uncurl = !m_uncurl;

			if (player.isGrounded)
			{
				if (
					player.lateralVelocity.magnitude <= player.stats.current.minSpeedToUnroll
					|| (m_uncurl && timeSinceEntered > m_minimumDuration)
				)
					player.states.Change<WalkPlayerState>();

				return;
			}

			if (
				player.verticalVelocity.y < 0
				&& (m_uncurl || player.stats.current.unrollWhenFalling)
			)
				player.states.Change<FallPlayerState>();
		}
	}
}
