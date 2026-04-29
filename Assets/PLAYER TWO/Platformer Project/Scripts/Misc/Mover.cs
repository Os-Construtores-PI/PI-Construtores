using System.Collections;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Mover")]
	public class Mover : MonoBehaviour
	{
		public Vector3 offset;
		public float duration;
		public float resetDuration;

		protected Vector3 m_initialPosition;

		public virtual void ApplyOffset()
		{
			StopAllCoroutines();
			StartCoroutine(ApplyOffsetRoutine(m_initialPosition, m_initialPosition + offset, duration));
		}

		public virtual void ResetMover()
		{
			StopAllCoroutines();
			StartCoroutine(ApplyOffsetRoutine(transform.localPosition, m_initialPosition, resetDuration));
		}

		protected virtual IEnumerator ApplyOffsetRoutine(Vector3 from, Vector3 to, float duration)
		{
			var elapsedTime = 0f;

			while (elapsedTime < duration)
			{
				var t = elapsedTime / duration;
				transform.localPosition = Vector3.Lerp(from, to, t);
				elapsedTime += Time.deltaTime;
				yield return null;
			}

			transform.localPosition = to;
		}

		protected virtual void Start()
		{
			m_initialPosition = transform.localPosition;
		}
	}
}
