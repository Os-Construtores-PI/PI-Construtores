using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/States/Walk Player State")]
	public class WalkPlayerState : PlayerState
	{
		protected float? m_brakingFrame;
		protected bool m_isBraking;

		protected const int k_brakeFrameDelay = 1;

		protected override void OnEnter(Player player) { }

		protected override void OnExit(Player player) { }

		protected override void OnStep(Player player)
		{
			player.Gravity();
			player.SnapToGround();
			player.Jump();
			player.Fall();
			player.Roll();
			player.Spin();
			player.PickAndThrow();
			player.Dash();
			player.RegularSlopeFactor();
			player.DecelerateToTopSpeed();

			var inputDirection = player.inputs.GetMovementCameraDirection(out var magnitude);

			if (inputDirection.sqrMagnitude > 0)
			{
				HandleBrake(player, inputDirection);

				if (m_isBraking)
					player.states.Change<BrakePlayerState>();
				else
					player.Accelerate(inputDirection, magnitude);
			}
			else
			{
				player.Friction();

				if (player.lateralVelocity.sqrMagnitude <= 0)
				{
					player.states.Change<IdlePlayerState>();
				}
			}

			player.FaceDirectionSmooth(player.lateralVelocity);
			player.Crouch();
			player.RollCharge();
		}

		public override void OnContact(Player player, Collider other) { }

		// Fixes braking caused by sudden velocity changes when rotating upside down on certain slopes.
		// This sucks but I don't have a better solution right now.
		protected virtual void HandleBrake(Player player, Vector3 inputDirection)
		{
			var dot = Vector3.Dot(inputDirection, player.lateralVelocity);
			var tryingToBrake =
				dot < player.stats.current.brakeThreshold
				&& player.lateralVelocity.magnitude > player.stats.current.minSpeedToBrake;

			if (tryingToBrake)
			{
				if (m_brakingFrame == null)
					m_brakingFrame = Time.frameCount;
				else if (Time.frameCount - m_brakingFrame > k_brakeFrameDelay)
					m_isBraking = true;

				return;
			}

			m_brakingFrame = null;
			m_isBraking = false;
		}
	}
}
