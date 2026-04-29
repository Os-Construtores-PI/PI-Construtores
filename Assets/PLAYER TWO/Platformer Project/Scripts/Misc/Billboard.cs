using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Billboard")]
	public class Billboard : MonoBehaviour
	{
		protected Camera m_camera;

		protected virtual void Start()
		{
			m_camera = Camera.main;
		}

		protected virtual void LateUpdate()
		{
			if (m_camera)
			{
				transform.LookAt(m_camera.transform.position);
			}
		}
	}
}
