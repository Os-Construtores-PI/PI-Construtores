using UnityEngine;
using System.Collections.Generic;

namespace PLAYERTWO.PlatformerProject
{
	public static class BoundsHelper
	{
		private static Dictionary<Collider, Bounds> m_localBounds = new();

		/// <summary>
		/// Returns the Bounds of a given collider without rotation.
		/// </summary>
		/// <param name="collider">The collider you want to get the Bounds from.</param>
		public static Bounds GetLocalBounds(Collider collider)
		{
			if (!m_localBounds.ContainsKey(collider))
			{
				var originalRotation = collider.transform.rotation;
				collider.transform.rotation = Quaternion.identity;
				var localBounds = collider.bounds;
				collider.transform.rotation = originalRotation;
				m_localBounds.Add(collider, localBounds);
			}

			return m_localBounds[collider];
		}

		/// <summary>
		/// Returns true if the bounds of a collider is bellow a given point.
		/// </summary>
		/// <param name="collider">The collider you want to check.</param>
		/// <param name="point">The point in world space.</param>
		public static bool IsBellowPoint(Collider collider, Vector3 point)
		{
			var localBounds = GetLocalBounds(collider);
			var extents = localBounds.extents.y;
			var top = collider.bounds.center + collider.transform.up * extents;

			var localPoint = collider.transform.InverseTransformPoint(point);
			var localTop = collider.transform.InverseTransformPoint(top);

			return localPoint.y >= localTop.y;
		}

		/// <summary>
		/// Returns true if the bounds of a collider is above a given point.
		/// </summary>
		/// <param name="collider">The collider you want to check.</param>
		/// <param name="point">The point in world space.</param>
		public static bool IsAbovePoint(Collider collider, Vector3 point)
		{
			var localBounds = GetLocalBounds(collider);
			var extents = localBounds.extents.y;
			var bottom = collider.bounds.center - collider.transform.up * extents;

			var localPoint = collider.transform.InverseTransformPoint(point);
			var localBottom = collider.transform.InverseTransformPoint(bottom);

			return localPoint.y <= localBottom.y;
		}

		/// <summary>
		/// Returns true if a given point is bellow the top of the collider.
		/// </summary>
		/// <param name="collider">The collider you want to check.</param>
		/// <param name="point">The point in world space.</param>
		public static bool IsPointBellowTop(Collider collider, Vector3 point)
		{
			var localBounds = GetLocalBounds(collider);
			var extents = localBounds.extents.y;
			var top = collider.bounds.center + collider.transform.up * extents;

			var localPoint = collider.transform.InverseTransformPoint(point);
			var localTop = collider.transform.InverseTransformPoint(top);

			return localPoint.y <= localTop.y;
		}

		/// <summary>
		/// Returns true if a given point in the extents radius of the collider.
		/// </summary>
		/// <param name="collider">The collider you want to check.</param>
		/// <param name="point">The point in world space.</param>
		public static bool IsPointInExtentsRadius(Collider collider, Vector3 point)
		{
			var head = collider.bounds.center - point;
			return head.magnitude <= collider.bounds.extents.x;
		}

		/// <summary>
		/// Returns true if a given point is inside a capsule.
		/// </summary>
		/// <param name="collider">The collider of the capsule.</param>
		/// <param name="point">The reference point in world space.</param>
		public static bool IsPointInsideCapsule(CapsuleCollider collider, Vector3 point)
		{
			var localBounds = GetLocalBounds(collider);
			var closestPoint = GetClosestPointFromCapsule(collider, point);
			var distance = (point - closestPoint).magnitude;

			return distance <= localBounds.extents.x;
		}

		/// <summary>
		/// Returns the closest position from a given point in a capsule.
		/// </summary>
		/// <param name="collider">The collider of the capsule.</param>
		/// <param name="point">The reference point in world space.</param>
		public static Vector3 GetClosestPointFromCapsule(CapsuleCollider collider, Vector3 point)
		{
			var localBounds = GetLocalBounds(collider);
			var center = collider.bounds.center;
			var capsuleOffset = localBounds.extents.y - localBounds.extents.x;
			var top = center + collider.transform.up * capsuleOffset;
			var bottom = center - collider.transform.up * capsuleOffset;

			return NearestPointOnFiniteLine(top, bottom, point);
		}

		/// <summary>
		/// Returns the nearest position from a given point in a line.
		/// </summary>
		/// <param name="start">The start of the line in world space.</param>
		/// <param name="end">The end of the line in world space.</param>
		/// <param name="point">The reference point in world space.</param>
		public static Vector3 NearestPointOnFiniteLine(Vector3 start, Vector3 end, Vector3 point)
		{
			var line = end - start;
			var len = line.magnitude;
			line /= len;

			var v = point - start;
			var d = Vector3.Dot(v, line);
			d = Mathf.Clamp(d, 0f, len);
			return start + line * d;
		}

		/// <summary>
		/// Returns the nearest point in the surface of a box.
		/// </summary>
		/// <param name="center">The center of the box.</param>
		/// <param name="size">The size of the box.</param>
		/// <param name="rotation">The rotation of the box.</param>
		/// <param name="point">The reference point.</param>
		public static Vector3 NearestPointOnBox(Vector3 center, Vector3 size, Quaternion rotation, Vector3 point)
		{
			var extents = size * 0.5f;
			var direction = point - center;

			direction = Quaternion.Inverse(rotation) * direction;

			var clampedDirection = new Vector3(
				Mathf.Clamp(direction.x, -extents.x, extents.x),
				Mathf.Clamp(direction.y, -extents.y, extents.y),
				Mathf.Clamp(direction.z, -extents.z, extents.z)
			);

			return center + rotation * clampedDirection;
		}

		/// <summary>
		/// Returns the nearest point from the surface of a disc.
		/// </summary>
		/// <param name="center">The center of the disc.</param>
		/// <param name="normal">The normal direction of the disc surface.</param>
		/// <param name="radius">The radius of the disc.</param>
		/// <param name="point">The reference point.</param>
		public static Vector3 NearestPointOnDisc(Vector3 center, Vector3 normal, float radius, Vector3 point)
		{
			var head = point - center;
			var distance = Vector3.Dot(head, normal);
			var pointOnPlane = point - normal * distance;
			var vectorToProjectedPoint = pointOnPlane - center;
			var magnitude = vectorToProjectedPoint.magnitude;

			if (magnitude <= radius)
				return pointOnPlane;

			return center + vectorToProjectedPoint / magnitude * radius;
		}
	}
}
