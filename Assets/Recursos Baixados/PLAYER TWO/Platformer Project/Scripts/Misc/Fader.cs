using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Image))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Fader")]
	public class Fader : Singleton<Fader>
	{
		public float speed = 1f;

		protected Image m_image;

		/// <summary>
		/// Fades out with no callback.
		/// </summary>
		public void FadeOut() => FadeOut(() => { });

		/// <summary>
		/// Fades in with no callback.
		/// </summary>
		public void FadeIn() => FadeIn(() => { });

		/// <summary>
		/// Fades in with callback.
		/// </summary>
		/// <param name="onFinished">The action that will be invoked in the end of the routine.</param>
		public void FadeOut(Action onFinished)
		{
			StopAllCoroutines();
			StartCoroutine(FadeOutRoutine(onFinished));
		}

		/// <summary>
		/// Fades in with callback.
		/// </summary>
		/// <param name="onFinished">The action that will be invoked in the end of the routine.</param>
		public void FadeIn(Action onFinished)
		{
			StopAllCoroutines();
			StartCoroutine(FadeInRoutine(onFinished));
		}

		/// <summary>
		/// Set the fader alpha to a given value.
		/// </summary>
		/// <param name="alpha">The desired alpha value.</param>
		public virtual void SetAlpha(float alpha)
		{
			var color = m_image.color;
			color.a = alpha;
			m_image.color = color;
		}

		/// <summary>
		/// Increases the alpha to one and invokes the callback afterwards.
		/// </summary>
		protected virtual IEnumerator FadeOutRoutine(Action onFinished)
		{
			while (m_image.color.a < 1)
			{
				var color = m_image.color;
				color.a += speed * Time.deltaTime;
				m_image.color = color;
				yield return null;
			}

			onFinished?.Invoke();
		}

		/// <summary>
		/// Decreases the alpha to zero and invokes the callback afterwards.
		/// </summary>
		protected virtual IEnumerator FadeInRoutine(Action onFinished)
		{
			while (m_image.color.a > 0)
			{
				var color = m_image.color;
				color.a -= speed * Time.deltaTime;
				m_image.color = color;
				yield return null;
			}

			onFinished?.Invoke();
		}

		protected override void Awake()
		{
			base.Awake();
			m_image = GetComponent<Image>();
		}
	}
}
