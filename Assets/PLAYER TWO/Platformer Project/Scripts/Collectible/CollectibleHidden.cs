using System.Collections;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collectible))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Collectible/Hidden Collectible")]
	public class CollectibleHidden : MonoBehaviour
	{
		[Header("Hidden Settings")]
		[Tooltip("The height the collectible will move to when revealed.")]
		public float revealHeight = 2f;

		[Tooltip("The duration of the reveal animation in seconds.")]
		public float revealDuration = 0.25f;

		[Tooltip("The delay before the collectible hides again in seconds.")]
		public float hideDelay = 0.5f;

		protected Collectible m_collectible;
		protected WaitForSeconds m_hideDelay;
		protected Vector3 m_initialPosition;

		protected virtual void Awake()
		{
			InitializeCollectible();
			InitializeWaits();
			m_initialPosition = transform.position;
		}

		protected virtual void InitializeCollectible()
		{
			m_collectible = GetComponent<Collectible>();
			m_collectible.visibleOnStart = false;
			m_collectible.autoHide = false;
			m_collectible.playParticleOnCollect = false;
			m_collectible.onCollect.AddListener(OnCollect);
		}

		protected virtual void InitializeWaits()
		{
			m_hideDelay = new WaitForSeconds(hideDelay);
		}

		protected virtual void OnCollect(Player player)
		{
			StopAllCoroutines();
			StartCoroutine(RevealRoutine());
		}

		/// <summary>
		/// Restores the hidden collectible to its initial spawned state.
		/// </summary>
		public virtual void Restore()
		{
			StopAllCoroutines();
			transform.position = m_initialPosition;
			m_collectible.HideDisplay();
		}

		protected virtual IEnumerator RevealRoutine()
		{
			var elapsedTime = 0f;
			var initialPosition = transform.position;
			var targetPosition = initialPosition + transform.up * revealHeight;

			m_collectible.ShowDisplay();

			while (elapsedTime < revealDuration)
			{
				var t = elapsedTime / revealDuration;
				transform.position = Vector3.Lerp(initialPosition, targetPosition, t);
				elapsedTime += Time.deltaTime;
				yield return null;
			}

			transform.position = targetPosition;
			yield return m_hideDelay;
			transform.position = initialPosition;
			m_collectible.HideDisplay();
		}
	}
}
