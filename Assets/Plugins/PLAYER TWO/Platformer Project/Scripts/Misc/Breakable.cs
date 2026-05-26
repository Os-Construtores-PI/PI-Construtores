using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collider), typeof(AudioSource))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Breakable")]
	public class Breakable : MonoBehaviour, ILevelTracked
	{
		[Header("General Settings")]
		[Tooltip("The object to disable when this object breaks.")]
		public GameObject display;

		[Header("Break Settings")]
		[Tooltip("Audio clips to play when this object breaks.")]
		public AudioClip[] breakClips;

		[Tooltip("Particle systems to play when this object breaks.")]
		public ParticleSystem[] breakParticles;

		[Tooltip("Collectibles to activate when this object breaks.")]
		public Collectible[] collectibles;

		[Header("Damage Settings")]
		[Tooltip("The sound to play when this object takes damage.")]
		public AudioClip damageClip;

		[Tooltip("The initial health points of this object.")]
		public int initialHP = 1;

		[Tooltip("The amount of time in seconds before this object can take damage again.")]
		public float damageCooldown = 0.5f;

		[Header("Breakable Events")]
		[Tooltip("Called when this object takes damage.")]
		public UnityEvent<int> OnDamage;

		[Tooltip("Called when this object breaks.")]
		public UnityEvent OnBreak;

		protected Collider m_collider;
		protected AudioSource m_audio;
		protected Rigidbody m_rigidBody;

		protected float m_lastDamageTime;
		protected bool m_initialIsKinematic;

		public int HP { get; protected set; }

		public bool broken => HP <= 0;
		public bool canTakeDamage => Time.time - m_lastDamageTime >= damageCooldown;

		protected virtual void InitializeAudio()
		{
			if (!TryGetComponent(out m_audio))
				m_audio = gameObject.AddComponent<AudioSource>();
		}

		protected virtual void InitializeCollider()
		{
			m_collider = GetComponent<Collider>();
		}

		protected virtual void InitializeHP() => HP = initialHP;

		protected virtual void InitializeState()
		{
			m_initialIsKinematic = m_rigidBody && m_rigidBody.isKinematic;
		}

		public virtual void ApplyDamage(int amount)
		{
			if (broken || !canTakeDamage)
				return;

			HP = Mathf.Max(0, HP - amount);

			if (HP > 0)
				Damage(amount);
			else
				Break();
		}

		protected virtual void Damage(int amount)
		{
			PlayAudioClip(damageClip);
			m_lastDamageTime = Time.time;
			OnDamage.Invoke(amount);
		}

		protected virtual void Break()
		{
			DisableRigidbody();
			PlayBreakClips();
			PlayBreakParticles();
			ActivateCollectibles();
			display.SetActive(false);
			m_collider.enabled = false;
			OnBreak?.Invoke();
		}

		protected virtual void PlayBreakClips()
		{
			foreach (var clip in breakClips)
				PlayAudioClip(clip);
		}

		protected virtual void PlayBreakParticles()
		{
			foreach (var particle in breakParticles)
			{
				if (particle)
				{
					particle.gameObject.SetActive(true);
					particle.Play();
				}
			}
		}

		protected virtual void ActivateCollectibles()
		{
			foreach (var collectible in collectibles)
				if (collectible)
					collectible.gameObject.SetActive(true);
		}

		protected virtual void DisableRigidbody()
		{
			if (!m_rigidBody)
				return;

			m_rigidBody.isKinematic = true;
		}

		protected virtual void PlayAudioClip(AudioClip clip)
		{
			if (clip)
				m_audio.PlayOneShot(clip);
		}

		/// <summary>
		/// Restores the breakable to its initial unbroken state, including its collectibles.
		/// </summary>
		public virtual void Restore()
		{
			HP = initialHP;
			display.SetActive(true);
			m_collider.enabled = true;

			if (m_rigidBody)
				m_rigidBody.isKinematic = m_initialIsKinematic;

			RestoreParticles();
			RestoreCollectibles();
		}

		protected virtual void RestoreParticles()
		{
			foreach (var particle in breakParticles)
			{
				if (particle)
				{
					particle.Stop();
					particle.gameObject.SetActive(false);
				}
			}
		}

		protected virtual void RestoreCollectibles()
		{
			foreach (var collectible in collectibles)
			{
				if (collectible)
				{
					collectible.Restore();
					collectible.gameObject.SetActive(false);
					collectible.transform.SetParent(transform);
					collectible.transform.SetPositionAndRotation(
						transform.position,
						transform.rotation
					);
				}
			}
		}

		/// <summary>
		/// Registers a callback that fires when this breakable is broken.
		/// </summary>
		public virtual void AddInteractionListener(System.Action listener) =>
			OnBreak.AddListener(() => listener());

		/// <summary>
		/// No state to capture — a protected breakable simply stays broken.
		/// </summary>
		public virtual void OnCheckpointActivated() { }

		/// <summary>
		/// No-op: a breakable that was broken before the checkpoint stays broken.
		/// </summary>
		public virtual void RestoreToCheckpoint() { }

		protected virtual void Start()
		{
			InitializeAudio();
			InitializeCollider();
			TryGetComponent(out m_rigidBody);
			InitializeState();
			InitializeHP();
		}
	}
}
