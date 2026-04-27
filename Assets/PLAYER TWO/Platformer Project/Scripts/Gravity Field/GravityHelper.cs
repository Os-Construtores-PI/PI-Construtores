using UnityEngine;
using UnityEngine.Splines;

namespace PLAYERTWO.PlatformerProject
{
	public static class GravityHelper
	{
		/// <summary>
		/// Returns true if a given point is inside a field's collider.
		/// </summary>
		/// <param name="collider">The collider of the field.</param>
		/// <param name="point">The point you want to check.</param>
		public static bool IsPointInsideField(Collider collider, Vector3 point)
		{
			if (collider is BoxCollider)
				return BoundsHelper.IsPointBellowTop(collider, point);
			else if (collider is SphereCollider)
				return BoundsHelper.IsPointInExtentsRadius(collider, point);
			else if (collider is CapsuleCollider capsule)
				return BoundsHelper.IsPointInsideCapsule(capsule, point);

			return false;
		}

		/// <summary>
		/// Returns the up direction from a capsule.
		/// </summary>
		/// <param name="collider">The capsule you want to get the up direction from.</param>
		/// <param name="point">The reference point in world space.</param>
		public static Vector3 GetUpFromCapsule(CapsuleCollider collider, Vector3 point)
		{
			var closestPoint = BoundsHelper.GetClosestPointFromCapsule(collider, point);
			return (point - closestPoint).normalized;
		}

		/// <summary>
		/// Returns the collider up direction from a given point.
		/// </summary>
		/// <param name="collider">The collider you want to calculate the up direction from.</param>
		/// <param name="point">The point used as reference to calculate the up direction.</param>
		public static Vector3 GetUpDirection(Collider collider, Vector3 point)
		{
			if (collider is BoxCollider)
				return collider.transform.up;
			else if (collider is SphereCollider)
				return (point - collider.transform.position).normalized;
			else if (collider is CapsuleCollider capsule)
				return GetUpFromCapsule(capsule, point);

			return Vector3.up;
		}

		public static Vector3 GetUpDirectionFromSphere(GravityField field, Vector3 point)
		{
			return (point - field.center).normalized;
		}

		public static Vector3 GetUpDirectionFromBox(GravityField field, Vector3 point)
		{
			var closestPoint = BoundsHelper.NearestPointOnBox(field.center, field.size, field.rotation, point);
			var direction = (point - closestPoint).normalized;
			return direction;
		}

		public static Vector3 GetUpDirectionFromCylinder(GravityField field, Vector3 point, bool capped = false)
		{
			var line = (field.bottom - field.top).normalized;
			var distance = Vector3.Dot(point - field.top, line);
			var closestPoint = BoundsHelper.NearestPointOnFiniteLine(field.top, field.bottom, point);

			if (!capped)
			{
				if (distance <= 0)
					closestPoint = BoundsHelper.NearestPointOnDisc(field.top, field.up, field.radius, point);
				else if (distance >= field.height)
					closestPoint = BoundsHelper.NearestPointOnDisc(field.bottom, -field.up, field.radius, point);

			}
			
			var direction = (point - closestPoint).normalized;

			return direction;
		}

		public static Vector3 GetUpDirectionFromCapsule(GravityField field, Vector3 point)
		{
			var closestPoint = BoundsHelper.NearestPointOnFiniteLine(field.topSphere, field.bottomSphere, point);
			return (point - closestPoint).normalized;
		}

		public static Vector3 GetUpDirectionFromSpline(SplineContainer container, Vector3 point)
		{
			var localPoint = container.transform.InverseTransformPoint(point);
			SplineUtility.GetNearestPoint(container.Spline, localPoint, out var nearest, out _, 128, 16);
			var globalNearestPoint = container.transform.TransformPoint(nearest);
			return (point - globalNearestPoint).normalized;
		}

		/// <summary>
		/// Returns the up direction from a half pipe.
		/// </summary>
		/// <param name="field">The half pipe field.</param>
		/// <param name="point">The reference point to get te up direction from.</param>
		/// <param name="inwards">If true, calculate the direction from a inwards half pipe.</param>
		public static Vector3 GetUpDirectionFromHalfPipe(GravityField field, Vector3 point, bool inwards = false)
		{
			if (inwards)
			{
				point -= field.forward * field.halfPipeRadius;
				point -= field.up * field.halfPipeRadius;
			}

			var closestPoint = BoundsHelper.NearestPointOnFiniteLine(field.halfPipeRight, field.halfPipeLeft, point);
			var direction = (point - closestPoint).normalized * (inwards ? -1 : 1);
			var angle = Vector3.SignedAngle(field.up, direction, field.right);

			angle = Mathf.Clamp(angle, 0, 90);
			direction = Quaternion.AngleAxis(angle, field.right) * field.up;
			direction = direction.normalized;

			return direction;
		}

		/// <summary>
		/// Returns the up direction from a disc.
		/// </summary>
		/// <param name="field">The disc gravity field.</param>
		/// <param name="point">The reference point to get the up direction from.</param>
		public static Vector3 GetUpDirectionFromDisc(GravityField field, Vector3 point)
		{
			var radius = field.radius - field.height * 0.5f;
			var nearestCenterPoint = BoundsHelper.NearestPointOnDisc(field.center, field.up, radius, point);
			var direction = (point - nearestCenterPoint).normalized;
			return direction;
		}
	}
}
