using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Scaler")]
	public class Scaler : MonoBehaviour
	{
		[Tooltip("If true, the animation will play when the game is paused.")]
		public bool unscaledTime;

		[Tooltip("The speed that controls the frequency of the scaling.")]
		public float speed = 1;

		[Tooltip("The amplitude that controls the intensity of the scaling.")]
		public float amplitude = 0.1f;

		[Tooltip("The base scale of the object.")]
		public float baseScale = 1f;

		protected virtual void Update()
		{
			var time = unscaledTime ? Time.unscaledTime : Time.time;
			var scale = baseScale + Mathf.Sin(time * speed) * amplitude;
			transform.localScale = new Vector3(scale, scale, scale);
		}
	}
}
