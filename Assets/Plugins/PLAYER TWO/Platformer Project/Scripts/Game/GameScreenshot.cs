using UnityEngine;
using UnityEngine.InputSystem;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Game/Game Screenshot")]
	public class GameScreenshot : MonoBehaviour
	{
#if UNITY_EDITOR
		protected virtual void Update()
		{
			if (Keyboard.current.f11Key.wasPressedThisFrame)
			{
				var path = Application.dataPath + "/Screenshots/";
				var time = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
				ScreenCapture.CaptureScreenshot($"{path}screenshot_{time}.png");
			}
		}
#endif
	}
}
