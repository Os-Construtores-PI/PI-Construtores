using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(TrailRenderer))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Spin Trail")]
	public class PlayerSpinTrail : MonoBehaviour
	{
		[Tooltip("The transform that represents the hand for the trail to follow.")]
		public Transform hand;

		protected Player m_player;
		protected TrailRenderer m_trail;
		protected Transform m_tempHand;

		protected const int k_findHandMaxDepth = 100;
		protected const string k_handName = "hand";

		protected virtual void InitializeTrail()
		{
			m_trail = GetComponent<TrailRenderer>();
			m_trail.enabled = false;
		}

		protected virtual void InitializePlayer()
		{
			m_player = GetComponentInParent<Player>();
			m_player.states.events.onChange.AddListener(HandleActive);
		}

		protected virtual void InitializeHand()
		{
			if (hand)
				return;

			LogRecursionWarning();

			if (!TryFindHand(out hand))
				LogHandNotAssignedWarning();
		}

		protected virtual void InitializeTransform()
		{
			if (!hand)
				return;

			transform.parent = hand;
			transform.localPosition = Vector3.zero;
		}

		protected virtual void HandleActive()
		{
			m_trail.enabled = m_player.states.IsCurrentOfType(typeof(SpinPlayerState));
		}

		protected virtual bool TryFindHand(out Transform handTransform)
		{
			handTransform = FindHandRecursive(m_player.skin);
			return handTransform;
		}

		protected virtual Transform FindHandRecursive(Transform from, int currentDepth = 0)
		{
			if (currentDepth > k_findHandMaxDepth)
				return null;

			foreach (Transform child in from)
			{
				if (child.name.IndexOf(k_handName, System.StringComparison.OrdinalIgnoreCase) >= 0)
					return child;

				m_tempHand = FindHandRecursive(child, currentDepth + 1);

				if (m_tempHand)
					return m_tempHand;
			}

			return null;
		}

		protected virtual void LogRecursionWarning()
		{
			Debug.LogWarning(
				"PlayerSpinTrail: hand not assigned. The component will attempt to find "
					+ "the hand in the player's skin which may be slow or fail. "
					+ "It is recommended to assign the hand manually."
			);
		}

		protected virtual void LogHandNotAssignedWarning()
		{
			Debug.LogWarning(
				"PlayerSpinTrail: hand not assigned and could not be found in the player's skin. "
					+ "The spin trail will not work as expected."
			);
		}

		protected virtual void Start()
		{
			InitializeTrail();
			InitializePlayer();
			InitializeHand();
			InitializeTransform();
		}
	}
}
