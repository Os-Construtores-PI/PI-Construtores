using UnityEngine;
using System.Collections;

namespace PLAYERTWO.PlatformerProject
{
	public class LedgeHangingPlayerState : PlayerState
	{
		protected bool m_keepPlatform;
		protected Coroutine m_exitPlatformRoutine;

		protected const float k_exitPlatformDelay = 0.25f;

		protected override void OnEnter(Player player)
		{
			if (m_exitPlatformRoutine != null)
				player.StopCoroutine(m_exitPlatformRoutine);

			m_keepPlatform = false;
			player.velocity = Vector3.zero;
			player.controller.IgnoreCollider(player.ledge);
			player.skin.position += player.transform.rotation * player.stats.current.ledgeHangingSkinOffset;
			player.ResetJumps();
			player.ResetAirSpins();
			player.ResetAirDash();
		}

		protected override void OnExit(Player player)
		{
			m_exitPlatformRoutine = player.StartCoroutine(ExitPlatformRoutine(player));
			player.skin.position -= player.transform.rotation * player.stats.current.ledgeHangingSkinOffset;
		}

		protected override void OnStep(Player player)
		{
			var playerTop = player.position + player.transform.up * player.height * 0.5f;
			var ledgeTopMaxDistance = player.radius + player.stats.current.ledgeMaxForwardDistance;
			var ledgeTopHeightOffset = player.height * 0.5f + player.stats.current.ledgeMaxDownwardDistance;
			var topOrigin = player.position + player.transform.up * ledgeTopHeightOffset + player.transform.forward * ledgeTopMaxDistance;
			var sideOrigin = playerTop - player.transform.up * player.stats.current.ledgeSideHeightOffset;
			var rayDistance = player.radius + player.stats.current.ledgeSideMaxDistance;
			var rayRadius = player.stats.current.ledgeSideCollisionRadius;

			var detectingBorder = Physics.Raycast(topOrigin, -player.transform.up, out var topHit, player.height,
				player.stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore);

			var detectingWall = Physics.SphereCast(sideOrigin, rayRadius, player.transform.forward, out var sideHit,
				rayDistance, player.stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore);

			if (!detectingBorder || !detectingWall)
			{
				player.states.Change<FallPlayerState>();
				return;
			}

			var inputDirection = player.inputs.GetMovementDirection();
			var ledgeSideOrigin = sideOrigin + player.transform.right * Mathf.Sign(inputDirection.x) * player.radius;
			var sideForward = -(sideHit.normal - player.transform.up * Vector3.Dot(player.transform.up, sideHit.normal)).normalized;
			var destinationHeight = player.height * 0.5f;
			var climbDestination = topHit.point + player.transform.up * destinationHeight +
				player.transform.forward * player.radius;

			player.FaceDirection(sideForward, Space.World);

			if (Physics.Raycast(ledgeSideOrigin, sideForward, rayDistance,
				player.stats.current.ledgeHangingLayers, QueryTriggerInteraction.Ignore))
				HandleHorizontalMovement(player, inputDirection);
			else
				player.lateralVelocity = Vector3.zero;

			var ledgeDistance = Vector3.Distance(playerTop, topHit.point);
			var verticalOffset = topHit.point - player.transform.up * player.height * 0.5f;
			var lateralOffset = sideForward * ledgeDistance;
			var finalPosition = verticalOffset - lateralOffset;

			player.position = finalPosition;

			if (player.inputs.GetReleaseLedgeDown())
			{
				player.FaceDirection(-sideForward, Space.World);
				player.states.Change<FallPlayerState>();
			}
			else if (player.inputs.GetJumpDown())
			{
				player.Jump(player.stats.current.maxJumpHeight);
				player.states.Change<FallPlayerState>();
			}
			else if (inputDirection.z > 0 && player.stats.current.canClimbLedges &&
					((1 << topHit.collider.gameObject.layer) & player.stats.current.ledgeClimbingLayers) != 0 &&
					player.FitsIntoPosition(climbDestination - player.transform.forward * player.radius * 0.5f))
			{
				m_keepPlatform = true;
				player.states.Change<LedgeClimbingPlayerState>();
			}
		}

		public override void OnContact(Player player, Collider other) { }

		protected virtual IEnumerator ExitPlatformRoutine(Player player)
		{
			if (m_keepPlatform) yield break;

			yield return new WaitForSeconds(k_exitPlatformDelay);

			player.ExitMovingPlatform();
			player.controller.IgnoreCollider(player.ledge, false);
		}

		protected virtual void HandleHorizontalMovement(Player player, Vector3 inputDirection)
		{
			var speed = Vector3.Dot(player.localRight, player.lateralVelocity);
			var accel = player.stats.current.ledgeMovementAcceleration;
			var topSpeed = player.stats.current.ledgeMovementTopSpeed;
			var friction = player.stats.current.ledgeMovementFriction;

			if (inputDirection.x > 0)
				speed += accel * Time.deltaTime;
			else if (inputDirection.x < 0)
				speed -= accel * Time.deltaTime;

			if (inputDirection.x == 0 || Mathf.Sign(speed) != Mathf.Sign(inputDirection.x))
				speed = Mathf.MoveTowards(speed, 0, friction * Time.deltaTime);

			speed = Mathf.Clamp(speed, -topSpeed, topSpeed);
			player.lateralVelocity = player.localRight * speed;
		}
	}
}
