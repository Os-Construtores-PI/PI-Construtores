using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Image))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Flash")]
	public class Flash : Singleton<Flash>
	{
		public float duration = 0.1f;
		public float fadeDuration = 0.5f;

		protected Image image;

		protected virtual void Start()
		{
			image = GetComponent<Image>();
		}

		/// <summary>
		/// Triggers the flash effect with the default duration values.
		/// </summary>
		public void Trigger() => Trigger(duration, fadeDuration);

		/// <summary>
		/// Triggers the flash effect with custom duration values.
		/// </summary>
		public void Trigger(float duration, float fadeDuration)
		{
			StopAllCoroutines();
			StartCoroutine(Routine(duration, fadeDuration));
		}

		protected IEnumerator Routine(float duration, float fadeDuration)
		{
			var elapsedTime = 0f;
			var color = image.color;

			color.a = 1;
			image.color = color;

			yield return new WaitForSeconds(duration);

			while (elapsedTime < fadeDuration)
			{
				color.a = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
				elapsedTime += Time.deltaTime;
				image.color = color;

				yield return null;
			}

			color.a = 0;
			image.color = color;
		}
	}
}
