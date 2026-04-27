using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	public class LedgeClimbingPlayerState : PlayerState
	{
		protected GameObject m_skinClimbSlot;

		protected const float k_groundDistance = 0.3f;
		protected const float k_verticalSmoothSpeed = 0.5f;

		protected override void OnEnter(Player player)
		{
			player.playerEvents.OnLedgeClimbing.Invoke();

			if (!m_skinClimbSlot)
				m_skinClimbSlot = new GameObject();

			m_skinClimbSlot.transform.position = player.transform.position;
			m_skinClimbSlot.transform.rotation = player.transform.rotation;
			player.platform?.Attach(m_skinClimbSlot.transform);
			player.velocity = Vector3.zero;
			player.SetSkinParent(m_skinClimbSlot.transform,
				player.stats.current.ledgeClimbingSkinOffset);
		}

		protected override void OnExit(Player player)
		{
			player.platform?.Detach(m_skinClimbSlot.transform);
			player.controller.IgnoreCollider(player.ledge, false);
			player.velocity = Vector3.zero;
			player.ResetSkinParent();
		}

		protected override void OnStep(Player player)
		{
			var totalDuration = player.stats.current.ledgeClimbingDuration;
			var halfDuration = totalDuration * 0.5f;
			var verticalSpeed = player.height / halfDuration;
			var lateralSpeed = player.radius * 2f / halfDuration;

			if (timeSinceEntered < halfDuration)
				player.velocity = player.transform.up * verticalSpeed;
			else
			{
				player.velocity = player.transform.forward * lateralSpeed;
				SmoothVerticalPosition(player);
			}

			if (timeSinceEntered >= totalDuration)
				player.states.Change<IdlePlayerState>();
		}

		public override void OnContact(Player player, Collider other) { }

		protected virtual void SmoothVerticalPosition(Player player)
		{
			var maxGroundDistance = player.height * 0.5f + k_groundDistance;

			if (Physics.Raycast(player.position, -player.transform.up, out var hit,
				maxGroundDistance, player.controller.collisionLayer, QueryTriggerInteraction.Ignore))
			{
				var targetPosition = hit.point + player.transform.up * player.height * 0.5f;
				var distanceDelta = Time.deltaTime * k_verticalSmoothSpeed;
				player.position = Vector3.MoveTowards(player.position, targetPosition, distanceDelta);
			}
		}
	}
}
