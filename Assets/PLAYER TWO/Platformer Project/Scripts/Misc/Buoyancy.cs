using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Rigidbody))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Buoyancy")]
	public class Buoyancy : MonoBehaviour
	{
		public float force = 10f;

		protected Rigidbody m_rigidbody;

		protected virtual void Start()
		{
			m_rigidbody = GetComponent<Rigidbody>();
		}

		protected virtual void OnTriggerStay(Collider other)
		{
			if (!other.CompareTag(GameTags.VolumeWater)) return;

			var localBounds = BoundsHelper.GetLocalBounds(other);
			var up = GravityHelper.GetUpDirection(other, transform.position);
			var top = other.bounds.center + up * localBounds.extents.y;

			var localPoint = other.transform.InverseTransformPoint(transform.position);
			var localTop = other.transform.InverseTransformPoint(top);

			localTop = new Vector3(localPoint.x, localTop.y, localPoint.z);
			top = other.transform.TransformPoint(localTop);

			var multiplier = Vector3.Distance(transform.position, top);
			var buoyancy = up * force * multiplier;

			m_rigidbody.AddForce(buoyancy);
		}
	}
}
