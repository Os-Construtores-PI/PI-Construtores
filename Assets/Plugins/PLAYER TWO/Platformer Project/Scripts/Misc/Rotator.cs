using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Rotator")]
	public class Rotator : MonoBehaviour
	{
		public Space space = Space.Self;
		public Vector3 eulers = new Vector3(0, -180, 0);

		protected Renderer m_renderer;

		protected virtual void Start()
		{
			InitializeRenderer();
		}

		protected virtual void LateUpdate()
		{
			if (m_renderer && !m_renderer.isVisible)
				return;

			transform.Rotate(eulers * Time.deltaTime, space);
		}

		protected virtual void InitializeRenderer()
		{
			m_renderer = GetComponent<Renderer>();

			if (!m_renderer)
				m_renderer = GetComponentInChildren<Renderer>();
		}
	}
}
