using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(UIContainer))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Confirmation Screen")]
	public class UIConfirmationScreen : Singleton<UIConfirmationScreen>
	{
		[Header("Text Elements")]
		[Tooltip("References the text component that displays the title.")]
		public TMP_Text titleText;

		[Tooltip("References the text component that displays the message.")]
		public TMP_Text messageText;

		[Header("Button Elements")]
		[Tooltip("References the button that accepts the confirmation.")]
		public Button yesButton;

		[Tooltip("References the button that declines the confirmation.")]
		public Button noButton;

		[Tooltip("References the button that declines the confirmation.")]
		public Button closeButton;

		protected GameObject m_lastSelected;
		protected Navigation m_navigation;
		protected UIContainer m_container;
		protected UnityAction m_onAccept;

		public UIContainer container
		{
			get
			{
				if (m_container == null)
					m_container = GetComponent<UIContainer>();

				return m_container;
			}
		}

		protected virtual void Start()
		{
			InitializeNavigation();
			InitializeButtons();
		}

		protected virtual void InitializeNavigation()
		{
			yesButton.navigation = new Navigation
			{
				mode = Navigation.Mode.Explicit,
				selectOnUp = closeButton,
				selectOnRight = noButton,
			};
			noButton.navigation = new Navigation
			{
				mode = Navigation.Mode.Explicit,
				selectOnUp = closeButton,
				selectOnLeft = yesButton,
			};
			closeButton.navigation = new Navigation
			{
				mode = Navigation.Mode.Explicit,
				selectOnDown = noButton,
			};
		}

		protected virtual void InitializeButtons()
		{
			yesButton.onClick.AddListener(Accept);
			noButton.onClick.AddListener(Decline);
			closeButton.onClick.AddListener(Decline);
		}

		public virtual void Accept()
		{
			m_onAccept?.Invoke();
			container.Hide();
		}

		public virtual void Decline()
		{
			m_onAccept = null;
			container.Hide(() =>
			{
				gameObject.SetActive(false);
				EventSystem.current.SetSelectedGameObject(m_lastSelected);
			});
		}

		/// <summary>
		/// Show the confirmation screen with the specified title and message.
		/// </summary>
		/// <param name="title">The title of the confirmation screen.</param>
		/// <param name="message">The message of the confirmation screen.</param>
		/// <param name="onAccept">The action to perform when the confirmation is accepted.</param>
		public virtual void Show(string title, string message, UnityAction onAccept)
		{
			m_lastSelected = EventSystem.current.currentSelectedGameObject;
			gameObject.SetActive(true);
			titleText.text = title;
			messageText.text = message;
			m_onAccept = onAccept;
			container.Show();
		}
	}
}
