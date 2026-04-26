using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(RectTransform))]
	public class UIHomingTarget : MonoBehaviour
	{
		protected Player m_player;
		protected Camera m_camera;
		protected RectTransform m_rectTransform;

		protected Vector3 m_initialScale;

		protected virtual void Awake()
		{
			m_camera = Camera.main;
			m_rectTransform = GetComponent<RectTransform>();
			m_initialScale = m_rectTransform.localScale;
		}

		protected virtual void Start()
		{
			UpdatePlayer(Level.instance.player);
			Level.instance.onPlayerChanged.AddListener(UpdatePlayer);
			gameObject.SetActive(false);
		}

		protected virtual void LateUpdate()
		{
			if (!m_player || !m_player.hasHomingTargets)
				return;

			var targetPosition = m_player.GetHomingTargetPosition();
			var screenPosition = m_camera.WorldToScreenPoint(targetPosition);
			m_rectTransform.position = screenPosition;
			m_rectTransform.localScale = screenPosition.z < 0 ? Vector3.zero : m_initialScale;
		}

		protected virtual void UpdatePlayer(Player player)
		{
			if (m_player && m_player == player)
				return;

			if (m_player)
				m_player.playerEvents.OnHomingTargetUpdated.RemoveListener(UpdateTarget);

			m_player = player;
			m_player.playerEvents.OnHomingTargetUpdated.AddListener(UpdateTarget);
		}

		protected virtual void UpdateTarget(Collider target)
		{
			gameObject.SetActive(target != null);
		}
	}
}
