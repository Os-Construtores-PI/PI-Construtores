using System;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[Serializable]
	public class PlayerEvents
	{
		/// <summary>
		/// Called when the Player jumps.
		/// </summary>
		public UnityEvent OnJump;

		/// <summary>
		/// Called when the Player gets damage.
		/// </summary>
		public UnityEvent OnHurt;

		/// <summary>
		/// Called when the Player died.
		/// </summary>
		public UnityEvent OnDie;

		/// <summary>
		/// Called when the Player uses the Spin Attack.
		/// </summary>
		public UnityEvent OnSpin;

		/// <summary>
		/// Called when the Player pick up an object.
		/// </summary>
		public UnityEvent OnPickUp;

		/// <summary>
		/// Called when the Player throws an object.
		/// </summary>
		public UnityEvent OnThrow;

		/// <summary>
		/// Called when the Player started the Stomp Attack.
		/// </summary>
		public UnityEvent OnStompStarted;

		/// <summary>
		/// Called when the Player starts moving down with Stomp Attack.
		/// </summary>
		public UnityEvent OnStompFalling;

		/// <summary>
		/// Called when the Player landed from the Stomp Attack.
		/// </summary>
		public UnityEvent OnStompLanding;

		/// <summary>
		/// Called when the Player finished the Stomp Attack.
		/// </summary>
		public UnityEvent OnStompEnding;

		/// <summary>
		/// Called when the player grabs onto a ledge.
		/// </summary>
		public UnityEvent OnLedgeGrabbed;

		/// <summary>
		/// Called when the Player climbs a ledge.
		/// </summary>
		public UnityEvent OnLedgeClimbing;

		/// <summary>
		/// Called when the Player air dives.
		/// </summary>
		public UnityEvent OnAirDive;

		/// <summary>
		/// Called when the Player performs a backflip.
		/// </summary>
		public UnityEvent OnBackflip;

		/// <summary>
		/// Called when the Player starts gliding.
		/// </summary>
		public UnityEvent OnGlidingStart;

		/// <summary>
		/// Called when the Player stops gliding.
		/// </summary>
		public UnityEvent OnGlidingStop;

		/// <summary>
		/// Called when the Player starts dashing.
		/// </summary>
		public UnityEvent OnDashStarted;

		/// <summary>
		/// Called when the Player finishes dashing.
		/// </summary>
		public UnityEvent OnDashEnded;

		/// <summary>
		/// Called when the Player starts crouching.
		/// </summary>
		public UnityEvent OnCrouchStarted;

		/// <summary>
		/// Called when the Player stands up after crouching.
		/// </summary>
		public UnityEvent OnCrouchEnded;

		/// <summary>
		/// Called when the Player's movement mode changes.
		/// </summary>
		public UnityEvent<PlayerMovementMode> OnMovementModeChanged;

		/// <summary>
		/// Called when the Player's homing target is updated.
		/// </summary>
		public UnityEvent<Collider> OnHomingTargetUpdated;

		/// <summary>
		/// Called when the Player starts sliding.
		/// </summary>
		public UnityEvent OnSlideStarted;

		/// <summary>
		/// Called when the Player ends sliding.
		/// </summary>
		public UnityEvent OnSlideEnded;

		/// <summary>
		/// Called when the Player starts rolling.
		/// </summary>
		public UnityEvent OnRollStarted;

		/// <summary>
		/// Called when the Player ends rolling.
		/// </summary>
		public UnityEvent OnRollEnded;
	}
}
