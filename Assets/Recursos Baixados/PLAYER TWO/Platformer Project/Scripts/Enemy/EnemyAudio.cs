using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/Enemy Audio")]
	public class EnemyAudio : MonoBehaviour
	{
		[Header("Effects")]
		public AudioClip death;

		protected Enemy m_enemy;
		protected AudioSource m_audio;

		protected virtual void InitializeEnemy() => m_enemy = GetComponent<Enemy>();

		protected virtual void InitializeAudio()
		{
			if (!TryGetComponent(out m_audio))
			{
				m_audio = gameObject.AddComponent<AudioSource>();
			}
		}

		protected virtual void InitializeCallbacks()
		{
			m_enemy.enemyEvents.OnDie.AddListener(() => m_audio.PlayOneShot(death));
		}

		protected virtual void Start()
		{
			InitializeEnemy();
			InitializeAudio();
			InitializeCallbacks();
		}
	}
}
