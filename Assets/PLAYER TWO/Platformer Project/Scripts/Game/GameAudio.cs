using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Game/Game Audio")]
	public class GameAudio : Singleton<GameAudio>
	{
		public enum ButtonAudioType
		{
			Regular,
			Confirm,
			Enter,
			Open,
			Cancel,
			Back,
		}

		[Header("Audio Sources")]
		[Tooltip("Audio source for UI sounds. If not set, one will be added to this game object.")]
		public AudioSource uiSource;

		[Header("UI Button Sounds")]
		public AudioClip buttonHighlightSound;
		public AudioClip buttonRegularSound;
		public AudioClip buttonConfirmSound;
		public AudioClip buttonEnterSound;
		public AudioClip buttonOpenSound;
		public AudioClip buttonCancelSound;
		public AudioClip buttonBackSound;

		protected virtual void Start()
		{
			InitializeUIAudioSource();
		}

		protected virtual void InitializeUIAudioSource()
		{
			if (uiSource == null)
				uiSource = gameObject.AddComponent<AudioSource>();
		}

		protected virtual void PlayUISound(AudioClip clip)
		{
			if (clip == null)
				return;

			uiSource.PlayOneShot(clip);
		}

		public virtual void PlayButtonAudio(ButtonAudioType type)
		{
			switch (type)
			{
				default:
				case ButtonAudioType.Regular:
					PlayUISound(buttonRegularSound);
					break;
				case ButtonAudioType.Confirm:
					PlayUISound(buttonConfirmSound);
					break;
				case ButtonAudioType.Enter:
					PlayUISound(buttonEnterSound);
					break;
				case ButtonAudioType.Open:
					PlayUISound(buttonOpenSound);
					break;
				case ButtonAudioType.Cancel:
					PlayUISound(buttonCancelSound);
					break;
				case ButtonAudioType.Back:
					PlayUISound(buttonBackSound);
					break;
			}
		}

		public virtual void PlayButtonHighlightAudio() => PlayUISound(buttonHighlightSound);
	}
}
