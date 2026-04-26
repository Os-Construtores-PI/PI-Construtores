using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Button))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Social Button")]
	public class SocialButton : MonoBehaviour
	{
		[Tooltip("The button that will open the URL.")]
		public Button button;

		[Tooltip("The URL to open.")]
		public string url;

		protected virtual void Start()
		{
			if (!button)
				button = GetComponent<Button>();

			button.onClick.AddListener(() => Application.OpenURL(url));
		}
	}
}
