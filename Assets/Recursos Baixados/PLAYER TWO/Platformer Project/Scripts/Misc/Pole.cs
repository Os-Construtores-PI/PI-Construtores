using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(CapsuleCollider))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Pole")]
	public class Pole : MonoBehaviour
	{
		/// <summary>
		/// Returns the Collider of this Pole.
		/// </summary>
		public new CapsuleCollider collider { get; protected set; }

		/// <summary>
		/// The radius of this Pole.
		/// </summary>
		public float radius => collider.radius;

		/// <summary>
		/// The center point of this Pole.
		/// </summary>
		public Vector3 center => collider.bounds.center;

		protected Bounds m_localBounds;

		protected virtual void InitializeTag() => tag = GameTags.Pole;
		protected virtual void InitializeCollider() => collider = GetComponent<CapsuleCollider>();
		protected virtual void InitializeLocalBounds() => m_localBounds = BoundsHelper.GetLocalBounds(collider);

		/// <summary>
		/// Returns the direction of a given Transform to face this Pole.
		/// </summary>
		/// <param name="other">The transform you want to use.</param>
		/// <returns>The direction from the Transform to the Pole.</returns>
		public Vector3 GetDirectionToPole(Transform other) => GetDirectionToPole(other, out _);

		/// <summary>
		/// Returns the direction of a given Transform to face this Pole.
		/// </summary>
		/// <param name="other">The transform you want to use.</param>
		/// <param name="distance">The distance from the pole center.</param>
		/// <returns>The direction from the Transform to the Pole.</returns>
		public Vector3 GetDirectionToPole(Transform other, out float distance)
		{
			var target = collider.bounds.center - other.position;
			target = transform.InverseTransformDirection(target);
			target.y = 0;
			target = transform.TransformDirection(target);
			distance = target.magnitude;
			return target / distance;
		}

		/// <summary>
		/// Returns a point clamped to the Pole height.
		/// </summary>
		/// <param name="point">The point you want to clamp.</param>
		/// <param name="offset">Offset to adjust min and max height.</param>
		/// <returns>The point within the Pole height.</returns>
		public Vector3 ClampPointToPoleHeight(Vector3 point, float offset, out float t)
		{
			var heightExtents = m_localBounds.extents.y - offset;
			var top = collider.bounds.center + transform.up * heightExtents;
			var bottom = collider.bounds.center - transform.up * heightExtents;

			var localTop = transform.InverseTransformPoint(top);
			var localBottom = transform.InverseTransformPoint(bottom);
			var localPoint = transform.InverseTransformPoint(point);

			localPoint.y = Mathf.Clamp(localPoint.y, localBottom.y, localTop.y);
			t = Mathf.InverseLerp(localBottom.y, localTop.y, localPoint.y);

			return transform.TransformPoint(localPoint);
		}

		/// <summary>
		/// Rotates a given transform to match the Pole's upward direction.
		/// </summary>
		/// <param name="other">The transform you want to rotate.</param>
		public virtual void RotateToPole(Transform other)
		{
			var rotation = Quaternion.FromToRotation(other.up, transform.up);
			other.transform.rotation = rotation * other.transform.rotation;
		}

		protected virtual void Awake()
		{
			InitializeTag();
			InitializeCollider();
			InitializeLocalBounds();
		}
	}
}
