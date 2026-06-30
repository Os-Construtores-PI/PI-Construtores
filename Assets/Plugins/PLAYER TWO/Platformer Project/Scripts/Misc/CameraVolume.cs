using Unity.Cinemachine;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collider))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Camera Volume")]
	public class CameraVolume : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("The Cinemachine camera to enable when the player enters the volume.")]
		protected CinemachineCamera m_camera;

		protected Collider m_collider;

		protected virtual void Start()
		{
			InitializeCollider();
			InitializeCamera();
		}

		protected virtual void InitializeCollider()
		{
			m_collider = GetComponent<Collider>();
			m_collider.isTrigger = true;
		}

		protected virtual void InitializeCamera()
		{
			if (!m_camera)
				m_camera = GetComponentInChildren<CinemachineCamera>();

			m_camera.enabled = false;
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			if (!GameTags.IsPlayer(other))
				return;

			m_camera.enabled = true;
			PlayerCameraManager.instance.SetCurrentEnabled(false);
		}

		protected virtual void OnTriggerExit(Collider other)
		{
			if (!GameTags.IsPlayer(other))
				return;

			PlayerCameraManager.instance.SetCurrentEnabled(true);
			PlayerCameraManager.instance.SnapTo(m_camera.transform);
			m_camera.enabled = false;
		}
	}
}
