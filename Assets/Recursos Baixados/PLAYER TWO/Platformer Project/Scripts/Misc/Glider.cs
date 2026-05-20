using UnityEngine;
using System.Collections;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Glider")]
	public class Glider : MonoBehaviour
	{
		public Player player;
		public TrailRenderer[] trails;
		public float scaleDuration = 0.7f;

		[Header("Audio Settings")]
		public AudioClip openAudio;
		public AudioClip closeAudio;

		protected AudioSource m_audio;

		protected virtual void InitializePlayer()
		{
			if (!player)
				player = GetComponentInParent<Player>();
		}

		protected virtual void InitializeAudio()
		{
			if (!TryGetComponent(out m_audio))
				m_audio = gameObject.AddComponent<AudioSource>();
		}

		protected virtual void InitializeCallbacks()
		{
			player.playerEvents.OnGlidingStart.AddListener(ShowGlider);
			player.playerEvents.OnGlidingStop.AddListener(HideGlider);
		}

		protected virtual void InitializeGlider()
		{
			SetTrailsEmitting(false);
			transform.localScale = Vector3.zero;
		}

		protected virtual void ShowGlider()
		{
			StopAllCoroutines();
			StartCoroutine(ScaleGliderRoutine(Vector3.zero, Vector3.one));
			SetTrailsEmitting(true);
			m_audio.PlayOneShot(openAudio);
		}

		protected virtual void HideGlider()
		{
			StopAllCoroutines();
			StartCoroutine(ScaleGliderRoutine(Vector3.one, Vector3.zero));
			SetTrailsEmitting(false);
			m_audio.PlayOneShot(closeAudio);
		}

		protected virtual void SetTrailsEmitting(bool value)
		{
			if (trails == null) return;

			foreach (var trail in trails)
			{
				trail.emitting = value;
			}
		}

		protected IEnumerator ScaleGliderRoutine(Vector3 from, Vector3 to)
		{
			var time = 0f;

			transform.localScale = from;

			while (time < scaleDuration)
			{
				var scale = Vector3.Lerp(from, to, time / scaleDuration);
				transform.transform.localScale = scale;
				time += Time.deltaTime;
				yield return null;
			}

			transform.localScale = to;
		}

		protected virtual void Start()
		{
			InitializePlayer();
			InitializeAudio();
			InitializeCallbacks();
			InitializeGlider();
		}
	}
}
