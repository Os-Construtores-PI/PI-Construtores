using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Platform/Rotating Platform")]
	public class RotatingPlatform : Platform
	{
		[Header("Rotation Setting")]
		[Tooltip("The space in which to rotate the platform.")]
		public Space space = Space.Self;

		[Tooltip("The rotation speed, in degrees per second, of the platform in each axis.")]
		public Vector3 rotation = new(0, 180, 0);

		public override void PlatformUpdate()
		{
			CacheTransform();
			HandleRotation();
			HandleAttachedTransforms();
		}

		protected virtual void HandleRotation()
		{
			transform.Rotate(rotation * Time.deltaTime, space);
		}
	}
}
