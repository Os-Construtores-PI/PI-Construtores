using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	/// <summary>
	/// Helper methods for raycasting.
	/// </summary>
	public static class RaycastHelpers
	{
		/// <summary>
		/// Get the real normal of a surface by casting a ray from the hit point.
		/// </summary>
		/// <param name="fromHit">The hit point to cast from.</param>
		/// <param name="collisionLayer">The layer mask to check for collisions.</param>
		/// <returns>The real normal of the surface.</returns>
		public static Vector3 GetRealNormal(RaycastHit fromHit, LayerMask collisionLayer)
		{
			var origin = fromHit.point + fromHit.normal * 0.01f;
			var direction = -fromHit.normal;
			var distance = 0.02f;
			var colliding = Physics.Raycast(
				origin,
				direction,
				out var hit,
				distance,
				collisionLayer,
				QueryTriggerInteraction.Ignore
			);
			return colliding ? hit.normal : fromHit.normal;
		}
	}
}
