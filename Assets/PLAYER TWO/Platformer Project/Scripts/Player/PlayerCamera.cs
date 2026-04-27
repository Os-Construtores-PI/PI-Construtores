using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(CinemachineCamera))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Camera")]
	public class PlayerCamera : MonoBehaviour
	{
		[SerializeField]
		[Header("Camera Settings")]
		[Tooltip(
			"The reference to the Player this camera will follow. "
				+ "If not set, the first Player found in the scene will be used."
		)]
		protected Player m_player;

		[Tooltip("The maximum distance the camera can be from the Player.")]
		public float maxDistance = 15f;

		[Tooltip(
			"The initial angle of the camera. Values greater than 0 will look down at the Player."
		)]
		public float initialPitch = 20f;

		[Tooltip(
			"The initial yaw of the camera. Values greater than 0 will look to the right of the Player."
		)]
		public float initialYaw = 0f;

		[Tooltip("The height offset of the camera from the Player.")]
		public float heightOffset = 1f;

		[Header("Following Settings")]
		[Tooltip(
			"The dead zone for the vertical movement when the Player is grounded. "
				+ "Values greater than 0 will make the camera move up when the Player is above the dead zone"
		)]
		public float verticalUpDeadZone = 0.15f;

		[Tooltip(
			"The dead zone for the vertical movement when the Player is grounded. "
				+ "Values greater than 0 will make the camera move down when the Player is below the dead zone"
		)]
		public float verticalDownDeadZone = 0.15f;

		[Tooltip(
			"The dead zone for the vertical movement when the Player is in the air. "
				+ "Values greater than 0 will make the camera move up when the Player is above the dead zone"
		)]
		public float verticalAirUpDeadZone = 4f;

		[Tooltip(
			"The dead zone for the vertical movement when the Player is in the air. "
				+ "Values greater than 0 will make the camera move down when the Player is below the dead zone"
		)]
		public float verticalAirDownDeadZone = 0;

		[Tooltip("The maximum vertical speed the camera can move when following the Player.")]
		public float maxVerticalSpeed = 10f;

		[Tooltip(
			"The maximum vertical speed the camera can move when following the Player in the air."
		)]
		public float maxAirVerticalSpeed = 100f;

		[Tooltip(
			"The minimum rotation speed of the camera when aligning with the Player's up vector."
		)]
		public float minRotationSpeed = 90f;

		[Tooltip(
			"The maximum rotation speed of the camera when aligning with the Player's up vector."
		)]
		public float maxRotationSpeed = 360f;

		[Header("Orbit Settings")]
		[Tooltip("Whether the camera can orbit around the Player by moving the mouse/look stick.")]
		public bool canOrbit = true;

		[Tooltip(
			"Whether the camera can orbit around the Player based on the Player's lateral velocity."
		)]
		public bool canOrbitWithVelocity = true;

		[Tooltip(
			"The multiplier for the orbit velocity based on the Player's lateral velocity. "
				+ "Higher values will make the camera orbit faster when the Player moves."
		)]
		public float orbitVelocityMultiplier = 5;

		[Range(0, 90)]
		[Tooltip("The maximum angle the camera can reach when looking up.")]
		public float verticalMaxRotation = 80;

		[Range(-90, 0)]
		[Tooltip("The minimum angle the camera can reach when looking down.")]
		public float verticalMinRotation = -20;

		[Header("Sensitivity Settings")]
		[Tooltip("The sensitivity of the camera when moving the mouse/look stick on the x-axis.")]
		public float xSensitivity = 1f;

		[Tooltip("The sensitivity of the camera when moving the mouse/look stick on the y-axis.")]
		public float ySensitivity = 1f;

		[Tooltip("The global sensitivity of the camera for both x and y axes.")]
		public float globalSensitivity = 1f;

		[Header("Lock On Settings")]
		[Tooltip(
			"The target the camera will lock on to. If set, the camera will "
				+ "ignore the orbit settings and focus on the target."
		)]
		public Transform lockTarget;

		[Tooltip(
			"The speed at which the camera will rotate on the x-axis to lock on the target. "
				+ "This helps to avoid sudden changes in the camera's pitch and smooths the movement."
		)]
		public float lockOnXSpeed = 180f;

		[Tooltip(
			"The speed at which the camera will rotate on the y-axis to lock on the target. "
				+ "This helps to avoid sudden changes in the camera's yaw and smooths the movement."
		)]
		public float lockOnYSpeed = 180f;

		protected float m_cameraDistance;
		protected float m_cameraTargetYaw;
		protected float m_cameraTargetPitch;

		protected Vector3 m_cameraTargetPosition;
		protected Quaternion m_currentUpRotation;

		protected Camera m_camera;
		protected CinemachineCamera m_virtualCamera;
		protected CinemachineThirdPersonFollow m_cameraBody;
		protected CinemachineBrain m_brain;

		protected Transform m_target;

		protected Coroutine m_redirectRoutine;

		/// <summary>
		/// The list of Player States where the camera will vertically center on the Player.
		/// </summary>
		protected readonly System.Type[] m_verticalCenterStates = new[]
		{
			typeof(SwimPlayerState),
			typeof(PoleClimbingPlayerState),
			typeof(WallDragPlayerState),
			typeof(WallRunPlayerState),
			typeof(LedgeHangingPlayerState),
			typeof(LedgeClimbingPlayerState),
			typeof(RailGrindPlayerState),
			typeof(HomingDashPlayerState),
		};

		protected string k_targetName = "Player Follower Camera Target";

		public Player player
		{
			get
			{
				if (m_player)
					return m_player;

				return Level.instance ? Level.instance.player : null;
			}
		}

		/// <summary>
		/// Whether the camera is frozen and won't update its position or rotation.
		/// </summary>
		public bool freeze { get; set; }

		/// <summary>
		/// Returns true if the camera is currently redirecting to a specific direction.
		/// </summary>
		public bool isRedirecting => m_redirectRoutine != null;

		protected virtual void InitializeComponents()
		{
			m_camera = Camera.main;
			m_virtualCamera = GetComponent<CinemachineCamera>();
			m_cameraBody = gameObject.AddComponent<CinemachineThirdPersonFollow>();
			m_brain = m_camera.GetComponent<CinemachineBrain>();
		}

		protected virtual void InitializeFollower()
		{
			m_target = new GameObject(k_targetName).transform;
			m_target.SetPositionAndRotation(player.transform.position, player.transform.rotation);
		}

		protected virtual void InitializeCamera()
		{
			m_virtualCamera.Follow = m_target;
			m_virtualCamera.LookAt = player.transform;
			m_brain.WorldUpOverride = m_target;

			Reset();
		}

		protected virtual bool CenterPlayerVertically() =>
			player.states.IsCurrentOfType(m_verticalCenterStates);

		/// <summary>
		/// Resets the camera to its initial position and rotation.
		/// </summary>
		public virtual void Reset()
		{
			if (!player || !m_target)
				return;

			m_cameraDistance = maxDistance;
			m_cameraTargetPitch = initialPitch;
			m_cameraTargetYaw = initialYaw;
			m_cameraTargetPosition = player.unsizedPosition + player.transform.up * heightOffset;
			m_target.SetPositionAndRotation(
				m_cameraTargetPosition,
				player.transform.rotation
					* Quaternion.Euler(m_cameraTargetPitch, m_cameraTargetYaw, 0)
			);
			m_currentUpRotation = player.transform.rotation;

			if (player.IsSideScroller)
			{
				var pathRotation = Quaternion.FromToRotation(m_target.right, player.pathForward);
				m_target.rotation = pathRotation * m_target.rotation;
			}

			m_cameraBody.CameraDistance = m_cameraDistance;
			var prevUpdateMethod = m_brain.UpdateMethod;
			m_brain.UpdateMethod = CinemachineBrain.UpdateMethods.ManualUpdate;
			m_brain.ManualUpdate();
			m_brain.UpdateMethod = prevUpdateMethod;
		}

		/// <summary>
		/// Snaps the camera to a world position and rotation based on a target transform.
		/// </summary>
		/// <param name="target">The target transform to snap to.</param>
		public virtual void SnapTo(Transform target)
		{
			if (!target)
				return;

			var localDirection = Quaternion.Inverse(m_currentUpRotation) * target.forward;
			var targetYaw = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
			var targetPitch = -Mathf.Asin(localDirection.y) * Mathf.Rad2Deg;

			m_cameraTargetYaw = targetYaw;
			m_cameraTargetPitch = targetPitch;

			m_target.SetPositionAndRotation(
				target.position,
				Quaternion.Euler(m_cameraTargetPitch, m_cameraTargetYaw, 0)
			);

			m_currentUpRotation =
				Quaternion.FromToRotation(target.up, m_target.up) * m_target.rotation;
		}

		/// <summary>
		/// Redirects the camera to look towards a specific direction over a duration.
		/// </summary>
		/// <param name="direction">The direction to look towards.</param>
		/// <param name="yawOffset">The yaw offset to rotate the camera sideways.</param>
		/// <param name="pitchOffset">The pitch offset to rotate the camera up or down.</param>
		/// <param name="duration">The duration over which to redirect the camera.</param>
		public virtual void RedirectTo(
			Vector3 direction,
			float yawOffset,
			float pitchOffset,
			float duration = 0.5f
		)
		{
			if (m_redirectRoutine != null)
				StopCoroutine(m_redirectRoutine);

			m_redirectRoutine = StartCoroutine(
				RedirectRoutine(direction, yawOffset, pitchOffset, duration)
			);
		}

		protected virtual IEnumerator RedirectRoutine(
			Vector3 direction,
			float yawOffset,
			float pitchOffset,
			float duration
		)
		{
			// Transform the target direction into the camera's local-up space
			var localDirection = Quaternion.Inverse(m_currentUpRotation) * direction.normalized;

			// Compute yaw and pitch from that local direction
			var targetYaw = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
			var targetPitch = -Mathf.Asin(localDirection.y) * Mathf.Rad2Deg;

			targetYaw += yawOffset;
			targetPitch += pitchOffset;

			// Smoothly interpolate toward those target angles
			var initialYaw = m_cameraTargetYaw;
			var initialPitch = m_cameraTargetPitch;
			var timeElapsed = 0f;

			while (timeElapsed < duration)
			{
				var t = timeElapsed / duration;
				m_cameraTargetYaw = Mathf.LerpAngle(initialYaw, targetYaw, t);
				m_cameraTargetPitch = Mathf.LerpAngle(initialPitch, targetPitch, t);

				timeElapsed += Time.deltaTime;
				yield return null;
			}

			m_cameraTargetYaw = targetYaw;
			m_cameraTargetPitch = targetPitch;
			m_redirectRoutine = null;
		}

		protected virtual void HandleOffset()
		{
			var grounded = player.isGrounded && player.verticalVelocity.y <= 0;
			var target = player.unsizedPosition + player.transform.up * heightOffset;
			var head = target - m_cameraTargetPosition;

			var xOffset = Vector3.Dot(head, player.transform.right);
			var yOffset = Vector3.Dot(head, player.transform.up);
			var zOffset = Vector3.Dot(head, player.transform.forward);

			var targetXOffset = xOffset;
			var targetYOffset = 0f;
			var targetZOffset = zOffset;

			var maxGroundDelta = maxVerticalSpeed * Time.deltaTime;
			var maxAirDelta = maxAirVerticalSpeed * Time.deltaTime;

			if (grounded || CenterPlayerVertically())
			{
				if (yOffset > verticalUpDeadZone)
				{
					var offset = yOffset - verticalUpDeadZone;
					targetYOffset += Mathf.Min(offset, maxGroundDelta);
				}
				else if (yOffset < verticalDownDeadZone)
				{
					var offset = yOffset - verticalDownDeadZone;
					targetYOffset += Mathf.Max(offset, -maxGroundDelta);
				}
			}
			else if (yOffset > verticalAirUpDeadZone)
			{
				var offset = yOffset - verticalAirUpDeadZone;
				targetYOffset += Mathf.Min(offset, maxAirDelta);
			}
			else if (yOffset < verticalAirDownDeadZone)
			{
				var offset = yOffset - verticalAirDownDeadZone;
				targetYOffset += Mathf.Max(offset, -maxAirDelta);
			}

			var rightOffset = player.transform.right * targetXOffset;
			var upOffset = player.transform.up * targetYOffset;
			var forwardOffset = player.transform.forward * targetZOffset;

			m_cameraTargetPosition =
				m_cameraTargetPosition + rightOffset + upOffset + forwardOffset;
		}

		protected virtual void HandleOrbit()
		{
			if (canOrbit && !lockTarget && !isRedirecting)
			{
				var direction = player.inputs.GetLookDirection();

				if (direction.sqrMagnitude > 0)
				{
					var usingMouse = player.inputs.IsLookingWithMouse();
					var deltaTimeMultiplier = usingMouse ? Time.timeScale : Time.deltaTime;
					var xSensitivity = this.xSensitivity * globalSensitivity * deltaTimeMultiplier;
					var ySensitivity = this.ySensitivity * globalSensitivity * deltaTimeMultiplier;

					m_cameraTargetYaw += direction.x * xSensitivity;
					m_cameraTargetPitch -= direction.z * ySensitivity;
					m_cameraTargetPitch = ClampAngle(
						m_cameraTargetPitch,
						verticalMinRotation,
						verticalMaxRotation
					);
				}
			}
		}

		protected virtual void HandleVelocityOrbit()
		{
			if (canOrbitWithVelocity && player.isGrounded && !lockTarget && !isRedirecting)
			{
				var localVelocity = m_target.InverseTransformVector(player.velocity);
				m_cameraTargetYaw += localVelocity.x * orbitVelocityMultiplier * Time.deltaTime;
			}
		}

		protected virtual void MoveTarget()
		{
			var rotationSpeed = Mathf.Lerp(
				minRotationSpeed,
				maxRotationSpeed,
				player.velocity.magnitude / player.targetTopSpeed
			);
			var upRotationMaxDelta = rotationSpeed * Time.deltaTime;
			var canRotateUpward =
				player.gravityField
				|| (player.rotateToGround && !player.IsSideScroller && player.groundAngle > 45f);

			var targetUp = canRotateUpward ? player.transform.up : Vector3.up;
			var upRotation =
				Quaternion.FromToRotation(m_currentUpRotation * Vector3.up, targetUp)
				* m_currentUpRotation;
			var smoothUpRotation = Quaternion.RotateTowards(
				m_currentUpRotation,
				upRotation,
				upRotationMaxDelta
			);

			m_currentUpRotation = smoothUpRotation;

			var yawRotation = Quaternion.Euler(0f, m_cameraTargetYaw, 0f);
			var pitchRotation = Quaternion.Euler(m_cameraTargetPitch, 0f, 0f);

			var baseRotation = yawRotation * pitchRotation;
			var finalRotation = m_currentUpRotation * baseRotation;

			m_target.SetPositionAndRotation(m_cameraTargetPosition, finalRotation);
			m_cameraBody.CameraDistance = m_cameraDistance;

			if (player.IsSideScroller)
			{
				var forward = -Vector3.Cross(targetUp, player.pathForward).normalized;

				if (forward.sqrMagnitude < 0.001f)
					forward = m_target.forward;

				var newRotation = Quaternion.LookRotation(forward, targetUp);
				m_target.rotation = newRotation;
			}
		}

		protected virtual void LockOnTarget()
		{
			if (!lockTarget)
				return;

			var direction = lockTarget.position - m_target.position;
			var localDirection = Quaternion.Inverse(m_currentUpRotation) * direction.normalized;

			var targetYaw = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
			var targetPitch = -Mathf.Asin(localDirection.y) * Mathf.Rad2Deg;

			var yawMaxDelta = lockOnYSpeed * Time.deltaTime;
			var pitchMaxDelta = lockOnXSpeed * Time.deltaTime;

			m_cameraTargetYaw = Mathf.MoveTowardsAngle(m_cameraTargetYaw, targetYaw, yawMaxDelta);
			m_cameraTargetPitch = Mathf.MoveTowardsAngle(
				m_cameraTargetPitch,
				targetPitch,
				pitchMaxDelta
			);
		}

		protected virtual void AssignWorldUpOverride()
		{
			if (m_brain && m_target)
				m_brain.WorldUpOverride = m_target;
		}

		protected virtual void OnEnable()
		{
			AssignWorldUpOverride();
		}

		protected virtual void OnValidate()
		{
			m_cameraDistance = maxDistance;
		}

		protected virtual void Start()
		{
			InitializeComponents();
			InitializeFollower();
			InitializeCamera();
		}

		protected virtual void LateUpdate()
		{
			if (freeze || !player)
				return;

			HandleOrbit();
			HandleVelocityOrbit();
			LockOnTarget();
			HandleOffset();
			MoveTarget();
		}

		protected virtual float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360)
				angle += 360;

			if (angle > 360)
				angle -= 360;

			return Mathf.Clamp(angle, min, max);
		}
	}
}
