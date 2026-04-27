using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Particles")]
	public class PlayerParticles : MonoBehaviour
	{
		[Header("Settings")]
		[Tooltip("Minimum speed required to play the walk dust particle.")]
		public float walkDustMinSpeed = 3.5f;

		[Tooltip("Minimum landing speed required to play the landing dust particle.")]
		public float landingParticleMinSpeed = 5f;

		[Header("Particles")]
		[Tooltip("Particle played when the player is walking.")]
		public ParticleSystem walkDust;

		[Tooltip("Particle played when the player lands.")]
		public ParticleSystem landDust;

		[Tooltip("Particle played when the player gets hurt.")]
		public ParticleSystem hurtDust;

		[Tooltip("Particle played when the player dashes.")]
		public ParticleSystem dashDust;

		[Tooltip("Particle played when the player is dashing.")]
		public ParticleSystem speedTrails;

		[Tooltip("Particle played when the player is grinding.")]
		public ParticleSystem grindTrails;

		[Tooltip("Particle played when the player is charging a roll.")]
		public ParticleSystem rollCharge;

		protected Player m_player;

		/// <summary>
		/// Start playing a given particle.
		/// </summary>
		/// <param name="particle">The particle you want to play.</param>
		public virtual void Play(ParticleSystem particle)
		{
			if (!particle.isPlaying)
			{
				particle.Play();
			}
		}

		/// <summary>
		/// Stop a given particle.
		/// </summary>
		/// <param name="particle">The particle you want to stop.</param>
		public virtual void Stop(ParticleSystem particle, bool clear = false)
		{
			if (particle.isPlaying)
			{
				var mode = clear
					? ParticleSystemStopBehavior.StopEmittingAndClear
					: ParticleSystemStopBehavior.StopEmitting;
				particle.Stop(true, mode);
			}
		}

		protected virtual void HandleWalkParticle()
		{
			if (m_player.isGrounded && !m_player.onRails && !m_player.onWater)
			{
				if (m_player.lateralVelocity.magnitude > walkDustMinSpeed)
				{
					Play(walkDust);
				}
				else
				{
					Stop(walkDust);
				}
			}
			else
			{
				Stop(walkDust);
			}
		}

		protected virtual void HandleRailParticle()
		{
			if (m_player.onRails)
				Play(grindTrails);
			else
				Stop(grindTrails, true);
		}

		protected virtual void HandleLandParticle()
		{
			if (!m_player.onWater && Mathf.Abs(m_player.velocity.y) >= landingParticleMinSpeed)
			{
				Play(landDust);
			}
		}

		protected virtual void HandleHurtParticle() => Play(hurtDust);

		protected virtual void OnDashStarted()
		{
			Play(dashDust);
			Play(speedTrails);
		}

		protected virtual void Start()
		{
			m_player = GetComponent<Player>();
			m_player.entityEvents.OnGroundEnter.AddListener(HandleLandParticle);
			m_player.playerEvents.OnHurt.AddListener(HandleHurtParticle);
			m_player.playerEvents.OnDashStarted.AddListener(OnDashStarted);
			m_player.playerEvents.OnDashEnded.AddListener(() => Stop(speedTrails, true));

			m_player.states.AddEnterListener<RollChargePlayerState>(() =>
			{
				if (m_player.isGrounded)
					Play(rollCharge);
			});

			m_player.states.AddExitListener<RollChargePlayerState>(() => Stop(rollCharge));
		}

		protected virtual void Update()
		{
			HandleWalkParticle();
			HandleRailParticle();
		}
	}
}
