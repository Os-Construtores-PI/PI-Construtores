using System.Collections;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Enemy))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Enemy/Enemy Respawn")]
	public class EnemyRespawn : MonoBehaviour
	{
		[Header("Auto Respawn Settings")]
		[Tooltip("If true, the enemy will automatically respawn after dying.")]
		public bool autoRespawn = false;

		[Tooltip("The delay in seconds before the enemy automatically respawns after dying.")]
		public float autoRespawnDelay = 5f;

		[Header("Level Respawn Settings")]
		[Tooltip(
			"If true, the enemy will respawn instantly when the Level Respawner fires its OnRespawn event."
		)]
		public bool respawnOnLevelRespawn = true;

		protected Enemy m_enemy;

		protected WaitForSeconds m_autoRespawnWait;
		protected Coroutine m_autoRespawnRoutine;

		protected virtual void Start()
		{
			InitializeEnemy();
			InitializeCallbacks();

			m_autoRespawnWait = new WaitForSeconds(autoRespawnDelay);
		}

		protected virtual void InitializeEnemy()
		{
			m_enemy = GetComponent<Enemy>();
		}

		protected virtual void InitializeCallbacks()
		{
			m_enemy.enemyEvents.OnDie.AddListener(OnDie);

			if (LevelRespawner.instance)
				LevelRespawner.instance.OnRespawn.AddListener(OnLevelRespawn);
		}

		protected virtual void OnDie()
		{
			if (!autoRespawn)
				return;

			if (m_autoRespawnRoutine != null)
				StopCoroutine(m_autoRespawnRoutine);

			m_autoRespawnRoutine = StartCoroutine(AutoRespawnRoutine());
		}

		protected virtual void OnLevelRespawn()
		{
			if (!respawnOnLevelRespawn)
				return;

			if (m_autoRespawnRoutine != null)
			{
				StopCoroutine(m_autoRespawnRoutine);
				m_autoRespawnRoutine = null;
			}

			m_enemy.Respawn();
		}

		protected IEnumerator AutoRespawnRoutine()
		{
			yield return m_autoRespawnWait;

			m_enemy.Respawn();
			m_autoRespawnRoutine = null;
		}
	}
}
