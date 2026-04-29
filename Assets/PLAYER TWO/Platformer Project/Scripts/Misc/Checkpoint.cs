using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collider))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Checkpoint")]
	public class Checkpoint : MonoBehaviour
	{
		[Header("Checkpoint Settings")]
		[Tooltip("The Transform where the Player will respawn.")]
		public Transform respawn;

		[Tooltip("The AudioClip played when the Checkpoint is activated.")]
		public AudioClip clip;

		/// <summary>
		/// Invoked when the Checkpoint is activated.
		/// </summary>
		[Space(10)]
		public UnityEvent OnActivate;

		protected Collider m_collider;
		protected AudioSource m_audio;

		/// <summary>
		/// Returns true if the Checkpoint is activated.
		/// </summary>
		public bool activated { get; protected set; }

		/// <summary>
		/// Activates this Checkpoint, updating the player's respawn transform and
		/// snapshotting the current collectible state via LevelCheckpoint.
		/// </summary>
		/// <param name="player">The Player you want to set the respawn.</param>
		public virtual void Activate(Player player)
		{
			if (activated)
				return;

			activated = true;
			m_audio.PlayOneShot(clip);
			LevelCheckpoint.instance.Activate(player, respawn);
			OnActivate?.Invoke();
		}

		protected virtual bool HasObstructions(Collider other) =>
			Physics.Linecast(
				m_collider.bounds.center,
				other.transform.position,
				Physics.DefaultRaycastLayers,
				QueryTriggerInteraction.Ignore
			);

		protected virtual void OnTriggerStay(Collider other)
		{
			if (activated || !GameTags.IsPlayer(other))
				return;

			if (!HasObstructions(other) && other.TryGetComponent<Player>(out var player))
				Activate(player);
		}

		protected virtual void Awake()
		{
			if (!TryGetComponent(out m_audio))
				m_audio = gameObject.AddComponent<AudioSource>();

			m_collider = GetComponent<Collider>();
			m_collider.isTrigger = true;
		}
	}
}
