using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collider))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Volume")]
	public class Volume : MonoBehaviour
	{
		public UnityEvent onStart;
		public UnityEvent onEnter;
		public UnityEvent onExit;

		public bool playerOnly;
		public AudioClip enterClip;
		public AudioClip exitClip;

		protected AudioSource m_audio;
		protected Collider m_collider;

		protected virtual void InitializeCollider()
		{
			m_collider = GetComponent<Collider>();
			m_collider.isTrigger = true;
		}

		protected virtual void InitializeAudioSource()
		{
			if (!enterClip && !exitClip) return;

			if (!TryGetComponent(out m_audio))
				m_audio = gameObject.AddComponent<AudioSource>();

			m_audio.spatialBlend = 0.5f;
		}

		protected virtual bool ValidCollider(Collider other) =>
			!other.CompareTag(GameTags.Hitbox) &&
			(!playerOnly || other.CompareTag(GameTags.Player));

		protected virtual void Start()
		{
			InitializeCollider();
			InitializeAudioSource();

			onStart.Invoke();
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			if (!ValidCollider(other)) return;

			if (enterClip)
				m_audio.PlayOneShot(enterClip);

			onEnter?.Invoke();
		}

		protected virtual void OnTriggerExit(Collider other)
		{
			if (!ValidCollider(other)) return;

			if (exitClip)
				m_audio.PlayOneShot(exitClip);

			onExit?.Invoke();
		}
	}
}
