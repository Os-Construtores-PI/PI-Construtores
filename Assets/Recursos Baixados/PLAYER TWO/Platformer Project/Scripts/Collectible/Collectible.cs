using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collider))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Collectible/Collectible")]
	public class Collectible : MonoBehaviour, ILevelTracked
	{
		[Header("General Settings")]
		[SerializeField, UniqueIdGen]
		[Tooltip("The unique identifier of the collectible item.")]
		protected string m_identifier;

		[Tooltip("The profile of the collectible item.")]
		public CollectibleProfile profile;

		[Tooltip("The trigger collider used to detect player contact.")]
		public Collider triggerCollider;

		[Tooltip("If true, the collectible will be collected when the player touches it.")]
		public bool collectOnContact = true;

		[Header("Feedback Settings")]
		[Tooltip("The game object to display when the collectible is active.")]
		public GameObject display;

		[Tooltip("The audio clip to play when the collectible is collected.")]
		public AudioClip audioClip;

		[Tooltip("The particle system to play when the collectible is collected.")]
		public ParticleSystem particle;

		[Header("Events")]
		public UnityEvent<Player> onCollect;
		public UnityEvent<Player> onCollectLoop;

		protected AudioSource m_audio;
		protected WaitForSeconds m_collectionWait;
		protected CollectiblePhysics m_physics;
		protected CollectibleHidden m_hidden;
		protected CollectibleLifetime m_lifetime;

		protected Vector3 m_initialPosition;
		protected Quaternion m_initialRotation;

		/// <summary>
		/// The unique identifier of the collectible.
		/// </summary>
		public string identifier => m_identifier;

		/// <summary>
		/// Can the collectible be collected?
		/// </summary>
		public bool canCollect { get; set; } = true;

		/// <summary>
		/// If true, the collectible will be visible when the scene starts.
		/// </summary>
		public bool visibleOnStart { get; set; } = true;

		/// <summary>
		/// If true, the collectible will automatically hide itself when collected.
		/// </summary>
		public bool autoHide { get; set; } = true;

		/// <summary>
		/// If true, the collectible will play its collection particle effect when collected.
		/// </summary>
		public bool playParticleOnCollect { get; set; } = true;

		/// <summary>
		/// The reference of the collectible profile.
		/// </summary>
		public string reference => profile ? profile.reference : string.Empty;

		/// <summary>
		/// Is the collectible currently visible?
		/// </summary>
		public bool isVisible => display ? display.activeSelf : false;

		protected virtual void Awake()
		{
			InitializeTag();
			InitializeCollider();
			InitializeAudio();
			InitializeCollectionWait();
			InitializeOptionalComponents();
		}

		protected virtual void Start()
		{
			InitializeTransform();
			InitializeVisibility();
			HandleCollectionDisabling();
		}

		protected virtual void OnEnable()
		{
			HandleCollectionDisabling();
		}

		protected virtual void OnTriggerStay(Collider other)
		{
			if (!collectOnContact || !GameTags.IsPlayer(other))
				return;

			if (other.TryGetComponent<Player>(out var player))
				Collect(player);
		}

		protected virtual void InitializeTag() => gameObject.tag = GameTags.Collectible;

		protected virtual void InitializeCollider()
		{
			if (!triggerCollider)
				triggerCollider = GetComponent<Collider>();

			triggerCollider.isTrigger = true;
		}

		protected virtual void InitializeAudio()
		{
			if (!TryGetComponent(out m_audio))
			{
				m_audio = gameObject.AddComponent<AudioSource>();
			}
		}

		protected virtual void InitializeCollectionWait()
		{
			m_collectionWait = new WaitForSeconds(0.1f);
		}

		protected virtual void InitializeOptionalComponents()
		{
			TryGetComponent(out m_physics);
			TryGetComponent(out m_hidden);
			TryGetComponent(out m_lifetime);
		}

		protected virtual void InitializeTransform()
		{
			m_initialPosition = transform.position;
			m_initialRotation = transform.rotation;
		}

		protected virtual void InitializeVisibility() => display.SetActive(visibleOnStart);

		/// <summary>
		/// Play the collection audio clip.
		/// </summary>
		public virtual void PlayCollectionAudio()
		{
			if (m_audio && audioClip)
				m_audio.PlayOneShot(audioClip);
		}

		/// <summary>
		/// Play the collection particle effect.
		/// </summary>
		public virtual void PlayCollectionParticle()
		{
			if (playParticleOnCollect && particle)
				particle.Play();
		}

		/// <summary>
		/// Collect the collectible.
		/// </summary>
		/// <param name="player">The player that collected the collectible.</param>
		public virtual void Collect(Player player)
		{
			if (!canCollect)
				return;

			HandleAutoHide();
			PlayCollectionParticle();
			StartCoroutine(CollectRoutine(player));
			triggerCollider.enabled = false;
			onCollect?.Invoke(player);
		}

		/// <summary>
		/// Show the collectible display.
		/// </summary>
		public virtual void ShowDisplay() => display.SetActive(true);

		/// <summary>
		/// Hide the collectible display.
		/// </summary>
		public virtual void HideDisplay() => display.SetActive(false);

		/// <summary>
		/// Registers a callback that fires when this collectible is collected by the player.
		/// </summary>
		/// <param name="listener">The action to invoke on collection.</param>
		public virtual void AddInteractionListener(System.Action listener) =>
			onCollect.AddListener(_ => listener());

		/// <summary>
		/// No state to capture; the score snapshot handles collectible state at checkpoint time.
		/// </summary>
		public virtual void OnCheckpointActivated() { }

		/// <summary>
		/// No action needed; protected collectibles remain in their collected state.
		/// </summary>
		public virtual void RestoreToCheckpoint() { }

		/// <summary>
		/// Restores the collectible to its initial uncollected state.
		/// </summary>
		public virtual void Restore()
		{
			if (profile.doNotRespawn)
				return;

			StopAllCoroutines();
			triggerCollider.enabled = true;
			canCollect = true;
			transform.SetPositionAndRotation(m_initialPosition, m_initialRotation);
			display.SetActive(true);

			if (m_physics)
				m_physics.Restore();
			if (m_hidden)
				m_hidden.Restore();
			if (m_lifetime)
				m_lifetime.Restore();
		}

		protected virtual IEnumerator CollectRoutine(Player player)
		{
			for (int i = 0; i < profile.collectionAmount; i++)
			{
				PlayCollectionAudio();
				LevelScore.instance.Collect(this);
				onCollectLoop?.Invoke(player);
				yield return m_collectionWait;
			}
		}

		protected virtual void HandleAutoHide()
		{
			if (autoHide)
				HideDisplay();
		}

		protected virtual void HandleCollectionDisabling()
		{
			if (!profile.collectOnce)
				return;

			if (LevelScore.instance.collectibles.HasCollected(this))
				gameObject.SetActive(false);
		}
	}
}
