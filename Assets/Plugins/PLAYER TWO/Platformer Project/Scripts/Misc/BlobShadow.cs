using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Blob Shadow")]
	public class BlobShadow : MonoBehaviour
	{
		[Tooltip("The target to follow. If not set, will default to the current GameObject.")]
		public Transform target;

		[Tooltip("The shadow prefab to instantiate.")]
		public GameObject shadowPrefab;

		[Tooltip("The maximum distance to cast the ray.")]
		public float maxGroundDistance = 50;

		[Tooltip("The scale of the shadow.")]
		public float shadowScale = 1f;

		[Tooltip("The layer mask to use for the raycast.")]
		public LayerMask groundLayer = ~0;

		protected GameObject m_instance;
		protected const float Epsilon = 0.001f;

		protected virtual void Start()
		{
			if (target == null)
				target = transform;

			m_instance = Instantiate(shadowPrefab, target.position, Quaternion.identity);
			m_instance.transform.localScale *= shadowScale;
		}

		protected virtual void LateUpdate()
		{
			if (!target || !m_instance)
				return;

			var colliding = Physics.Raycast(
				target.position,
				-target.up,
				out RaycastHit hit,
				maxGroundDistance,
				groundLayer,
				QueryTriggerInteraction.Ignore
			);

			if (colliding)
			{
				m_instance.transform.SetPositionAndRotation(
					hit.point + hit.normal * Epsilon,
					Quaternion.FromToRotation(Vector3.up, hit.normal)
				);
			}
			else
			{
				m_instance.transform.SetPositionAndRotation(
					m_instance.transform.position,
					Quaternion.FromToRotation(Vector3.up, target.up)
				);
			}
		}
	}
}
