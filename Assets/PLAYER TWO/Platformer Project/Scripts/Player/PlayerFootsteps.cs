using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Footsteps")]
	public class PlayerFootsteps : MonoBehaviour
	{
		[System.Serializable]
		public class Surface
		{
			public string tag;
			public AudioClip[] footsteps;
			public AudioClip[] landings;
		}

		public Surface[] surfaces;
		public AudioClip[] defaultFootsteps;
		public AudioClip[] defaultLandings;

		[Header("General Settings")]
		public float stepOffset = 1.25f;
		public float footstepVolume = 0.5f;

		protected Vector3 m_lastLateralPosition;

		protected Dictionary<string, AudioClip[]> m_footsteps = new();
		protected Dictionary<string, AudioClip[]> m_landings = new();

		protected Player m_player;
		protected AudioSource m_audio;

		protected virtual void PlayRandomClip(AudioClip[] clips)
		{
			if (clips.Length > 0)
			{
				var index = Random.Range(0, clips.Length);
				m_audio.PlayOneShot(clips[index], footstepVolume);
			}
		}

		protected virtual bool TryPlayFootstep(Dictionary<string, AudioClip[]> dict, Collider other)
		{
			foreach (var key in dict.Keys)
			{
				if (other.CompareTag(key))
				{
					PlayRandomClip(dict[key]);
					return true;
				}
			}

			return false;
		}

		protected virtual void Landing()
		{
			if (!m_player.onWater)
			{
				if (!TryPlayFootstep(m_landings, m_player.groundHit.collider))
					PlayRandomClip(defaultLandings);
			}
		}

		protected virtual void Start()
		{
			m_player = GetComponent<Player>();
			m_player.entityEvents.OnGroundEnter.AddListener(Landing);

			if (!TryGetComponent(out m_audio))
			{
				m_audio = gameObject.AddComponent<AudioSource>();
			}

			foreach (var surface in surfaces)
			{
				m_footsteps.Add(surface.tag, surface.footsteps);
				m_landings.Add(surface.tag, surface.landings);
			}
		}

		protected virtual void Update()
		{
			if (!m_player.isGrounded || !m_player.states.IsCurrentOfType(typeof(WalkPlayerState)))
				return;

			var position = transform.position;
			var distance = (m_lastLateralPosition - position).magnitude;

			if (distance < stepOffset)
				return;

			if (!TryPlayFootstep(m_footsteps, m_player.groundHit.collider))
				PlayRandomClip(defaultFootsteps);

			m_lastLateralPosition = position;
		}
	}
}
