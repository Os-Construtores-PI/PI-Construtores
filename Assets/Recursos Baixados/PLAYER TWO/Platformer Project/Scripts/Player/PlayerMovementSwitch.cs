using System.Collections;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collider))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Movement Switch")]
	public class PlayerMovementSwitch : MonoBehaviour
	{
		[Tooltip("The movement mode to switch to when the player enters this trigger.")]
		public PlayerMovementMode targetMode;

		protected Collider m_collider;
		protected Player m_tempPlayer;

		protected const float k_centralizeDuration = 0.5f;

		protected virtual void Start()
		{
			InitializeCollider();
		}

		protected virtual void InitializeCollider()
		{
			m_collider = GetComponent<Collider>();
			m_collider.isTrigger = true;
		}

		protected virtual void SwitchMovementMode(Player player)
		{
			if (!player || player.movingMode == targetMode)
				return;

			player.pathForward = transform.forward;
			player.movingMode = targetMode;
			CentralizePlayer(player);
		}

		protected virtual void CentralizePlayer(Player player)
		{
			if (player.movingMode != PlayerMovementMode.SideScroller)
				return;

			StartCoroutine(CentralizePlayerRoutine(player));
		}

		protected IEnumerator CentralizePlayerRoutine(Player player)
		{
			var t = 0f;
			var duration = k_centralizeDuration;
			var initialPosition = player.transform.position;

			while (t < duration)
			{
				var xzPosition = new Vector3(
					transform.position.x,
					player.transform.position.y,
					transform.position.z
				);

				t = Mathf.Min(t + Time.deltaTime, duration);
				var newPosition = Vector3.Lerp(initialPosition, xzPosition, t / duration);
				player.transform.position = newPosition;

				yield return null;
			}
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			if (GameTags.IsPlayer(other) && other.TryGetComponent(out m_tempPlayer))
			{
				SwitchMovementMode(m_tempPlayer);
			}
		}
	}
}
