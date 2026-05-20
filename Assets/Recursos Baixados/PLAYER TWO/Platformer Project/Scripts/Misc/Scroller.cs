using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Scroller")]
	public class Scroller : MonoBehaviour
	{
		[Tooltip("If true, the scroller will update when the game is paused.")]
		public bool unscaledTime;

		[Tooltip("The target graphic to scroll")]
		public Graphic targetGraphic;

		[Tooltip("The speed at which to scroll the texture")]
		public Vector2 scrollSpeed;

		public void Start()
		{
			InitializeGraphic();
			InitializeMaterial();
		}

		public void Update()
		{
			HandleTextureOffset();
		}

		protected virtual void InitializeGraphic()
		{
			if (!targetGraphic)
				targetGraphic = GetComponent<Graphic>();
		}

		protected virtual void InitializeMaterial()
		{
			targetGraphic.material = Instantiate(targetGraphic.material);
		}

		protected virtual void HandleTextureOffset()
		{
			var deltaTime = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
			targetGraphic.material.mainTextureOffset += scrollSpeed * deltaTime;
		}
	}
}
