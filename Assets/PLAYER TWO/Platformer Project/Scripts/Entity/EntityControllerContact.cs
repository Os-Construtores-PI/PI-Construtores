using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	public struct EntityControllerContact
	{
		public Collider collider;
		public Vector3 point;
		public Vector3 normal;

		/// <summary>
		/// Indicates whether the contact was detected during a
		/// vertical pass (downward/upward movement).
		/// </summary>
		public bool verticalPass;

		/// <summary>
		/// Returns true if the contact's collider is valid and enabled.
		/// </summary>
		public readonly bool enabled => collider && collider.enabled;

		public EntityControllerContact(RaycastHit hit, bool verticalPass = false)
		{
			collider = hit.collider;
			point = hit.point;
			normal = hit.normal;
			this.verticalPass = verticalPass;
		}
	}
}
