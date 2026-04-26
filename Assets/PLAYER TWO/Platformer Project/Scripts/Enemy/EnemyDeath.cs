using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/Enemy Death")]
	public class EnemyDeath : MonoBehaviour
	{
		[Header("Death Settings")]
		[Tooltip("GameObjects to disable when the enemy dies.")]
		public GameObject[] disableOnDeath;

		[Tooltip("GameObjects to enable when the enemy dies.")]
		public GameObject[] enableOnDeath;

		[Header("Particle Settings")]
		[Tooltip("Particle system to play when the enemy dies.")]
		public ParticleSystem deathParticle;

		[Tooltip("Particle system to play when the enemy revives.")]
		public ParticleSystem reviveParticle;

		protected Enemy m_enemy;

		protected virtual void Start()
		{
			InitializeEnemy();
			InitializeCallbacks();
		}

		protected virtual void InitializeEnemy()
		{
			if (!m_enemy)
				m_enemy = GetComponent<Enemy>();
		}

		protected virtual void InitializeCallbacks()
		{
			m_enemy.enemyEvents.OnDie.AddListener(OnDie);
			m_enemy.enemyEvents.OnRevive.AddListener(OnRevive);
			m_enemy.enemyEvents.OnRespawn.AddListener(OnRevive);
		}

		protected virtual void OnDie()
		{
			if (deathParticle)
				deathParticle.Play();

			HandleDeathGameObjects();
		}

		protected virtual void OnRevive()
		{
			if (reviveParticle)
				reviveParticle.Play();

			HandleReviveGameObjects();
		}

		protected virtual void HandleDeathGameObjects()
		{
			foreach (var go in disableOnDeath)
				go.SetActive(false);

			foreach (var go in enableOnDeath)
				go.SetActive(true);
		}

		protected virtual void HandleReviveGameObjects()
		{
			foreach (var go in disableOnDeath)
				go.SetActive(true);

			foreach (var go in enableOnDeath)
				go.SetActive(false);
		}
	}
}
