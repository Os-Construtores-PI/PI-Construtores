using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Button")]
	public class UIButton
		: MonoBehaviour,
			ISelectHandler,
			IDeselectHandler,
			IPointerEnterHandler,
			IPointerExitHandler
	{
		[Header("Text Settings")]
		[Tooltip("If true, the text color will change when the button is highlighted.")]
		public bool useTextColors = true;

		[Tooltip("The color of the text when the button is not interacted with.")]
		public Color normalColor = Color.white;

		[Tooltip("The color of the text when the button is highlighted.")]
		public Color highlightedColor = Color.white;

		[Tooltip("The color of the text when the button is pressed.")]
		public Color pressedColor = Color.white;

		[Header("Scale Settings")]
		public bool useScale = true;

		[Tooltip("The scale multiplier when the button is highlighted.")]
		public float scaleMultiplier = 1.1f;

		[Tooltip("The duration of the scale animation.")]
		public float scaleDuration = 0.1f;

		[Header("Audio Settings")]
		[Tooltip("If true, the button will play audio when clicked.")]
		public bool useClickAudio = true;

		[Tooltip("If true, the button will play audio when highlighted.")]
		public bool useHighlightAudio = true;

		[Tooltip("The type of audio to play when the button is clicked.")]
		public GameAudio.ButtonAudioType audioType = GameAudio.ButtonAudioType.Regular;

		protected Button m_button;
		protected TMP_Text[] m_texts;
		protected bool m_highlighted;

		protected virtual void Awake()
		{
			InitializeButton();
			InitializeTexts();
		}

		protected virtual void InitializeButton()
		{
			if (!TryGetComponent(out m_button))
				m_button = gameObject.AddComponent<Button>();

			m_button.onClick.AddListener(Select);
		}

		protected virtual void InitializeTexts()
		{
			m_texts = GetComponentsInChildren<TMP_Text>();
		}

		protected virtual void SetTextColors(Color color)
		{
			if (m_texts == null || !useTextColors)
				return;

			foreach (TMP_Text text in m_texts)
				text.color = color;
		}

		protected void Scale(Vector3 to)
		{
			if (!gameObject.activeInHierarchy || !useScale)
				return;

			StopAllCoroutines();
			StartCoroutine(ScaleRoutine(to));
		}

		protected IEnumerator ScaleRoutine(Vector3 to)
		{
			var time = 0f;
			var from = transform.localScale;

			while (time < scaleDuration)
			{
				transform.localScale = Vector3.Lerp(from, to, time / scaleDuration);
				time += Time.unscaledDeltaTime;
				yield return null;
			}

			transform.localScale = to;
		}

		protected virtual void Highlight()
		{
			if (!m_button.interactable || m_highlighted)
				return;

			if (useHighlightAudio)
				GameAudio.instance.PlayButtonHighlightAudio();

			m_highlighted = true;
			SetTextColors(highlightedColor);
			Scale(Vector3.one * scaleMultiplier);
		}

		protected virtual void Select()
		{
			if (!m_button.interactable)
				return;

			if (useClickAudio)
				GameAudio.instance.PlayButtonAudio(audioType);
		}

		protected virtual void ResetButton()
		{
			m_highlighted = false;
			SetTextColors(normalColor);
			Scale(Vector3.one);
		}

		public virtual void OnSelect(BaseEventData eventData) => Highlight();

		public virtual void OnDeselect(BaseEventData eventData) => ResetButton();

		public virtual void OnPointerEnter(PointerEventData eventData) => Highlight();

		public virtual void OnPointerExit(PointerEventData eventData) => ResetButton();
	}
}
