using System.Collections;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Grid Platform")]
	public class GridPlatform : MonoBehaviour
	{
		[Tooltip("The transform of the platform to rotate.")]
		public Transform platform;

		[Tooltip("The duration in seconds the platform will take to rotate.")]
		public float rotationDuration = 0.5f;

		protected bool m_clockwise = true;

		protected Player m_player => Level.instance.player;

		protected virtual void InitializeCallback()
		{
			Level.instance.onPlayerChanged.AddListener(
				(player) => m_player.playerEvents.OnJump.AddListener(Move)
			);
		}

		/// <summary>
		/// Starts the coroutine to rotate the platform 180 degrees.
		/// </summary>
		public virtual void Move()
		{
			StopAllCoroutines();
			StartCoroutine(MoveRoutine());
		}

		protected IEnumerator MoveRoutine()
		{
			var elapsedTime = 0f;
			var from = platform.localRotation;
			var to = Quaternion.Euler(0, 0, m_clockwise ? 180 : 0);
			m_clockwise = !m_clockwise;

			while (elapsedTime < rotationDuration)
			{
				elapsedTime += Time.deltaTime;
				platform.localRotation = Quaternion.Lerp(from, to, elapsedTime / rotationDuration);
				yield return null;
			}

			platform.localRotation = to;
		}

		protected virtual void Awake()
		{
			InitializeCallback();
		}
	}
}
