using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collider), typeof(AudioSource))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Portal")]
	public class Portal : MonoBehaviour
	{
		[Tooltip(
			"If true, the teleportation will trigger the flash effect form the Flash component."
		)]
		public bool useFlash = true;

		[Tooltip("The portal to teleport to.")]
		public Portal exit;

		[Tooltip("The forward offset to apply to the Player position when exiting the portal.")]
		public float exitOffset = 1f;

		[Tooltip("The sound to play when teleported.")]
		public AudioClip teleportClip;

		[Tooltip(
			"If true, the Player will be rotated to face the opposite direction when exiting the portal."
				+ "Only works when the Player is in side-scroller mode."
		)]
		public bool invertExitDirection;

		protected Collider m_collider;
		protected AudioSource m_audio;
		protected PlayerCamera m_camera;

		/// <summary>
		/// Returns the Portal global position.
		/// </summary>
		public Vector3 position => transform.position;

		/// <summary>
		/// Returns the Portal local forward direction.
		/// </summary>
		public Vector3 forward => transform.forward;

		protected virtual void Start()
		{
			m_collider = GetComponent<Collider>();
			m_audio = GetComponent<AudioSource>();
#if UNITY_6000_0_OR_NEWER
			m_camera = FindFirstObjectByType<PlayerCamera>();
#else
			m_camera = FindObjectOfType<PlayerCamera>();
#endif
			m_collider.isTrigger = true;
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			if (!exit || !other.TryGetComponent(out Player player))
				return;

			var offset = player.unsizedPosition - transform.position;
			var yOffset = Vector3.Dot(transform.up, offset);
			var localExitForward =
				Quaternion.FromToRotation(exit.transform.up, Vector3.up) * exit.forward;
			var lateralSpeed = player.lateralVelocity.magnitude;
			var verticalSpeed = player.verticalVelocity.y;

			if (player.IsSideScroller)
				player.pathForward = localExitForward * (invertExitDirection ? -1 : 1);

			player.transform.SetPositionAndRotation(
				exit.position + exit.transform.up * yOffset,
				exit.transform.rotation
			);

			player.FaceDirection(localExitForward);
			player.LockGravityField();

			player.gravityField?.IgnoreCollider(player.controller);
			player.gravityField = null;

			m_camera?.Reset();

			var inputDirection = player.inputs.GetMovementCameraDirection();

			if (Vector3.Dot(inputDirection, localExitForward) < 0)
				player.FaceDirection(-localExitForward);

			player.transform.position += player.transform.forward * exit.exitOffset;
			player.lateralVelocity = player.localForward * lateralSpeed;
			player.verticalVelocity = Vector3.up * verticalSpeed;

			if (useFlash && Flash.instance)
				Flash.instance.Trigger();

			m_audio.PlayOneShot(teleportClip);
		}
	}
}
