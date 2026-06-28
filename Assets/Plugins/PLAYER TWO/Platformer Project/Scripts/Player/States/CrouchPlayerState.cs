using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	public class CrouchPlayerState : PlayerState
	{
		protected bool m_sliding;

		protected override void OnEnter(Player player)
		{
			HandleSlideStart(player);
			player.ResizeCollider(player.stats.current.crouchHeight);
			player.playerEvents.OnCrouchStarted.Invoke();
		}

		protected override void OnExit(Player player)
		{
			if (m_sliding)
			{
				m_sliding = false;
				player.playerEvents.OnSlideEnded.Invoke();
			}

			player.ResizeCollider(player.originalHeight);
			player.playerEvents.OnCrouchEnded.Invoke();
		}

		protected override void OnStep(Player player)
		{
			player.Gravity();
			player.SnapToGround();

			if (!player.stats.current.crouchJumpBackflip)
				player.Jump();

			player.Fall();
			player.RegularSlopeFactor();
			player.Decelerate(player.stats.current.crouchFriction);
			player.FaceVelocityInPaths();
			player.FaceDirection(player.lateralVelocity);

			HandleSlideEnd(player);

			var inputDirection = player.inputs.GetMovementDirection();

			if (
				player.inputs.GetCrouch()
				|| !player.canStandUp
				|| (m_sliding && timeSinceEntered <= player.stats.current.minCrouchSlideDuration)
			)
			{
				if (
					player.stats.current.canCrawl
					&& !player.holding
					&& inputDirection.sqrMagnitude > 0
				)
				{
					if (!m_sliding)
						player.states.Change<CrawlingPlayerState>();
				}
				else if (player.inputs.GetJumpDown())
				{
					player.Backflip(player.stats.current.backflipBackwardForce);
				}
			}
			else
			{
				player.states.Change<IdlePlayerState>();
			}

			player.RollCharge();
		}

		public override void OnContact(Player player, Collider other) { }

		protected virtual void HandleSlideStart(Player player)
		{
			if (player.lateralVelocity.magnitude >= player.stats.current.minSpeedToSlide)
			{
				m_sliding = true;
				player.playerEvents.OnSlideStarted.Invoke();
				return;
			}

			m_sliding = false;
			player.lateralVelocity = Vector3.zero;
		}

		protected virtual void HandleSlideEnd(Player player)
		{
			if (m_sliding && player.lateralVelocity.sqrMagnitude <= 0.1f)
			{
				m_sliding = false;
				player.playerEvents.OnSlideEnded.Invoke();
			}
		}
	}
}
