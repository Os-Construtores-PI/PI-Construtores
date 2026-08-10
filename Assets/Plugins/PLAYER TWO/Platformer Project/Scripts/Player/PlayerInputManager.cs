using UnityEngine;
using UnityEngine.InputSystem;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Input Manager")]
	public class PlayerInputManager : MonoBehaviour
	{
		public InputActionAsset actions;

		protected InputAction m_movement;
		protected InputAction m_run;
		protected InputAction m_jump;
		protected InputAction m_roll;
		protected InputAction m_cancel;
		protected InputAction m_rollCharge;
		protected InputAction m_dive;
		protected InputAction m_swimUpward;
		protected InputAction m_spin;
		protected InputAction m_pickAndDrop;
		protected InputAction m_crouch;
		protected InputAction m_airDive;
		protected InputAction m_stomp;
		protected InputAction m_releaseLedge;
		protected InputAction m_pause;
		protected InputAction m_look;
		protected InputAction m_glide;
		protected InputAction m_dash;
		protected InputAction m_grindBrake;
		protected InputAction m_homingDash;

		protected Player m_player;
		protected Camera m_camera;

		protected float m_movementDirectionUnlockTime;
		protected float? m_lastJumpTime;

		/// <summary>
		/// Inverts the X axis for movement input.
		/// </summary>
		public bool invertXAxis { get; set; }

		/// <summary>
		/// Inverts the Z axis for movement input.
		/// </summary>
		public bool invertZAxis { get; set; }

		protected const string k_mouseDeviceName = "Mouse";
		protected const float k_jumpBuffer = 0.15f;

		public bool lockedMovementDirection => Time.time < m_movementDirectionUnlockTime;

		protected virtual void CacheActions()
		{
			m_movement = actions["Movement"];
			m_run = actions["Run"];
			m_jump = actions["Jump"];
			m_roll = actions["Roll"];
			m_cancel = actions["Action Cancel"];
			m_rollCharge = actions["Roll Charge"];
			m_dive = actions["Dive"];
			m_swimUpward = actions["Swim Up"];
			m_spin = actions["Spin"];
			m_pickAndDrop = actions["PickAndDrop"];
			m_crouch = actions["Crouch"];
			m_airDive = actions["AirDive"];
			m_stomp = actions["Stomp"];
			m_releaseLedge = actions["ReleaseLedge"];
			m_pause = actions["Pause"];
			m_look = actions["Look"];
			m_glide = actions["Glide"];
			m_dash = actions["Dash"];
			m_grindBrake = actions["Grind Brake"];
			m_homingDash = actions["Homing Dash"];
		}

		protected virtual void InitializePlayer() => m_player = GetComponent<Player>();

		public virtual Vector3 GetMovementDirection()
		{
			if (Time.time < m_movementDirectionUnlockTime)
				return Vector3.zero;

			var value = m_movement.ReadValue<Vector2>();

			if (invertXAxis)
				value.x = -value.x;

			if (invertZAxis)
				value.y = -value.y;

			return GetAxisWithCrossDeadZone(value);
		}

		public virtual Vector3 GetLookDirection()
		{
			var value = m_look.ReadValue<Vector2>();

			if (IsLookingWithMouse())
			{
				return new Vector3(value.x, 0, value.y);
			}

			return GetAxisWithCrossDeadZone(value);
		}

		public virtual Vector3 GetMovementCameraDirection(bool localSpace = true) =>
			GetMovementCameraDirection(out _, localSpace);

		public virtual Vector3 GetMovementCameraDirection(
			out float magnitude,
			bool localSpace = true
		)
		{
			return m_player.movingMode switch
			{
				PlayerMovementMode.SideScroller => GetHorizontalMovementCameraDirection(
					out magnitude
				),
				_ => GetLateralMovementCameraDirection(out magnitude, localSpace),
			};
		}

		public virtual Vector3 GetLateralMovementCameraDirection(
			out float magnitude,
			bool localSpace = true
		)
		{
			var direction = GetMovementDirection();
			magnitude = 0;

			if (direction.sqrMagnitude > 0)
			{
				var rotation = Quaternion.FromToRotation(m_camera.transform.up, transform.up);
				direction = rotation * m_camera.transform.rotation * direction;

				if (localSpace)
				{
					direction = Vector3.ProjectOnPlane(direction, transform.up);
					direction = Quaternion.FromToRotation(transform.up, Vector3.up) * direction;
				}

				magnitude = direction.magnitude;
				direction /= magnitude;
			}

			return direction;
		}

		public virtual Vector3 GetHorizontalMovementCameraDirection(out float magnitude)
		{
			var direction = GetMovementDirection();
			direction.z = magnitude = 0;

			if (direction.x != 0)
			{
				direction = m_player.pathForward * direction.x;
				magnitude = direction.magnitude;
				direction /= magnitude;
			}

			return direction;
		}

		/// <summary>
		/// Remaps a given axis considering the Input System's default deadzone.
		/// This method uses a cross shape instead of a circle one to evaluate the deadzone range.
		/// </summary>
		/// <param name="axis">The axis you want to remap.</param>
		public virtual Vector3 GetAxisWithCrossDeadZone(Vector2 axis)
		{
			var deadzone = InputSystem.settings.defaultDeadzoneMin;
			axis.x = Mathf.Abs(axis.x) > deadzone ? RemapToDeadzone(axis.x, deadzone) : 0;
			axis.y = Mathf.Abs(axis.y) > deadzone ? RemapToDeadzone(axis.y, deadzone) : 0;
			return new Vector3(axis.x, 0, axis.y);
		}

		public virtual bool IsLookingWithMouse()
		{
			if (m_look.activeControl == null)
			{
				return false;
			}

			return m_look.activeControl.device.name.Equals(k_mouseDeviceName);
		}

		public virtual bool GetRun() => m_run.IsPressed();

		public virtual bool GetRunUp() => m_run.WasReleasedThisFrame();

		public virtual bool GetJumpDown()
		{
			if (m_lastJumpTime != null && Time.time - m_lastJumpTime < k_jumpBuffer)
			{
				m_lastJumpTime = null;
				return true;
			}

			return false;
		}

		public virtual bool GetJumpUp() => m_jump.WasReleasedThisFrame();

		public virtual bool GetRoll() => m_roll.IsPressed();

		public virtual bool GetRollDown() => m_roll.WasPressedThisFrame();

		public virtual bool GetCancelDown() => m_cancel.WasPressedThisFrame();

		public virtual bool GetRollCharge() => m_rollCharge.IsPressed();

		public virtual bool GetSwimUpward() => m_swimUpward.IsPressed();

		public virtual bool GetDive() => m_dive.IsPressed();

		public virtual bool GetSpinDown() => m_spin.WasPressedThisFrame();

		public virtual bool GetPickAndDropDown() => m_pickAndDrop.WasPressedThisFrame();

		public virtual bool GetCrouch() => m_crouch.IsPressed();

		public virtual bool GetCrouchDown() => m_crouch.WasPressedThisFrame();

		public virtual bool GetAirDiveDown() => m_airDive.WasPressedThisFrame();

		public virtual bool GetStompDown() => m_stomp.WasPressedThisFrame();

		public virtual bool GetReleaseLedgeDown() => m_releaseLedge.WasPressedThisFrame();

		public virtual bool GetGlide() => m_glide.IsPressed();

		public virtual bool GetDashDown() => m_dash.WasPressedThisFrame();

		public virtual bool GetGrindBrake() => m_grindBrake.IsPressed();

		public virtual bool GetPauseDown() => m_pause.WasPressedThisFrame();

		public virtual bool GetHomingDashDown() => m_homingDash.WasPressedThisFrame();

		public virtual bool EscPressed()
		{
#if UNITY_STANDALONE
			return Keyboard.current.escapeKey.wasPressedThisFrame;
#else
			return false;
#endif
		}

		/// <summary>
		/// Remaps a value to a 0-1 range considering a given deadzone.
		/// </summary>
		/// <param name="value">The value you wants to remap.</param>
		/// <param name="deadzone">The minimum deadzone value.</param>
		protected float RemapToDeadzone(float value, float deadzone) =>
			Mathf.Sign(value) * ((Mathf.Abs(value) - deadzone) / (1 - deadzone));

		/// <summary>
		/// Temporally locks the movement direction input.
		/// </summary>
		/// <param name="duration">The duration of the locking state in seconds.</param>
		public virtual void LockMovementDirection(float duration = 0.25f)
		{
			m_movementDirectionUnlockTime = Time.time + duration;
		}

		/// <summary>
		/// Clears the jump buffer to avoid unwanted jumps.
		/// </summary>
		public virtual void ClearJumpBuffer() => m_lastJumpTime = null;

		protected virtual void Awake()
		{
			CacheActions();
			InitializePlayer();
		}

		protected virtual void Start()
		{
			m_camera = Camera.main;
			actions.Enable();
		}

		protected virtual void Update()
		{
			if (m_jump.WasPressedThisFrame() && !LevelPauser.Paused())
			{
				m_lastJumpTime = Time.time;
			}
		}

		protected virtual void OnEnable() => actions?.Enable();

		protected virtual void OnDisable() => actions?.Disable();
	}
}
