using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Button))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Start Game Button")]
	public class UIStartGameButton : MonoBehaviour
	{
		[Header("Start Game Settings")]
		[Tooltip("The screen that will be shown when the button is pressed.")]
		public UIMainMenu.Screen targetScreen = UIMainMenu.Screen.FileSelect;

		[Header("Animation Settings")]
		[Tooltip("If true, the button will animate when idle.")]
		public bool useAnimation = true;

		[Tooltip("The frequency of the zoom animation.")]
		public float zoomFrequency = 5.0f;

		[Tooltip("The magnitude of the zoom animation.")]
		public float zoomMagnitude = 0.05f;

		[Tooltip("The speed of the blink animation.")]
		public float blinkSpeed = 15.0f;

		protected Button m_button;
		protected bool m_pressed;
		protected Vector3 m_originalScale;

		protected WaitForSeconds m_changeScreenDelay = new(1f);

		protected virtual void Start()
		{
			InitializeButton();
		}

		protected virtual void Update()
		{
			HandleAnimations();
		}

		protected virtual void OnEnable()
		{
			if (m_button)
				m_button.interactable = true;
		}

		protected virtual void InitializeButton()
		{
			m_button = GetComponent<Button>();
			m_button.onClick.AddListener(OnButtonPressed);
			m_originalScale = transform.localScale;
		}

		protected virtual void HandleAnimations()
		{
			if (!useAnimation)
				return;

			HandleClickAnimation();
			HandleIdleAnimation();
		}

		protected virtual void HandleIdleAnimation()
		{
			if (m_pressed)
				return;

			float scaleFactor = 1.0f + Mathf.Sin(Time.time * zoomFrequency) * zoomMagnitude;
			transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
		}

		protected virtual void HandleClickAnimation()
		{
			if (!m_pressed)
				return;

			float blink = Mathf.PingPong(Time.time * blinkSpeed, 1);
			m_button.targetGraphic.enabled = blink > 0.5f;
		}

		protected virtual void OnButtonPressed()
		{
			if (m_pressed)
				return;

			m_pressed = true;
			m_button.interactable = false;
			transform.localScale = m_originalScale;
			StartCoroutine(ChangeSceneRoutine());
		}

		protected IEnumerator ChangeSceneRoutine()
		{
			yield return m_changeScreenDelay;

			m_pressed = false;
			m_button.targetGraphic.enabled = true;
			UIMainMenu.instance.ChangeTo(targetScreen);
		}
	}
}
