using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Lean")]
	public class PlayerLean : MonoBehaviour
	{
		public Transform target;
		public float maxTiltAngle = 15;
		public float tiltSmoothTime = 0.2f;
		public float minSpeedToLean = 5f;

		protected Player m_player;
		protected Quaternion m_initialRotation;

		protected float m_velocity;

		protected readonly System.Type[] m_validLeanStates = new[]
		{
			typeof(WalkPlayerState),
			typeof(SwimPlayerState),
			typeof(GlidingPlayerState),
		};

		/// <summary>
		/// Returns true if the Player can lean in the current state.
		/// </summary>
		public virtual bool ValidLeanState() => m_player.states.IsCurrentOfType(m_validLeanStates);

		/// <summary>
		/// Returns true if the Player can lean based on speed and state.
		/// </summary>
		/// <param name="speed">The current speed of the Player.</param>
		/// <returns>True if the Player can lean, false otherwise.</returns>
		public virtual bool CanLean(float speed) => ValidLeanState() && speed > minSpeedToLean;

		protected virtual void Awake()
		{
			m_player = GetComponent<Player>();
		}

		protected virtual void LateUpdate()
		{
			var speed = m_player.lateralVelocity.magnitude;
			var moveDirection = m_player.lateralVelocity / speed;
			var targetMoveDirection = m_player.targetMoveDirection;

			var angle = Vector3.SignedAngle(targetMoveDirection, moveDirection, Vector3.up);
			var amount = CanLean(speed) ? Mathf.Clamp(angle, -maxTiltAngle, maxTiltAngle) : 0;
			var rotation = target.localEulerAngles;

			rotation.z = Mathf.SmoothDampAngle(rotation.z, amount, ref m_velocity, tiltSmoothTime);
			target.localEulerAngles = rotation;
		}
	}
}
