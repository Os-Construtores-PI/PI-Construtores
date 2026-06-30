using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Save Card")]
	public class UISaveCard : MonoBehaviour
	{
		[Header("Text Formatting")]
		[Tooltip("The format string for the retries text.")]
		public string retriesFormat = "00";

		[Tooltip("The format string for the stars text.")]
		public string starsFormat = "00";

		[Tooltip("The format string for the coins text.")]
		public string coinsFormat = "000";

		[Tooltip("The format string for the date text.")]
		public string dateFormat = "MM/dd/y hh:mm";

		[Header("Containers")]
		[Tooltip("The parent game object of the save data elements.")]
		public GameObject dataContainer;

		[Tooltip("The parent game object of the free slot elements.")]
		public GameObject emptyContainer;

		[Header("Texts")]
		[Tooltip("References the retries text.")]
		public TMP_Text retries;

		[Tooltip("References the stars text.")]
		public TMP_Text stars;

		[Tooltip("References the coins text.")]
		public TMP_Text coins;

		[Tooltip("References the created at text.")]
		public TMP_Text createdAt;

		[Tooltip("References the updated at text.")]
		public TMP_Text updatedAt;

		[Header("Action Buttons")]
		[Tooltip("References the load button.")]
		public Button loadButton;

		[Tooltip("References the delete button.")]
		public Button deleteButton;

		[Tooltip("References the new game button.")]
		public Button newGameButton;

		[Header("Deleting Confirmation")]
		[Tooltip("The title of the confirmation screen.")]
		public string title = "Delete Save Data";

		[Tooltip("The message of the confirmation screen.")]
		public string message = "Are you sure you want to delete this save data?";

		protected int m_index;
		protected GameData m_data;

		/// <summary>
		/// Indicates if the card is filled with any data.
		/// </summary>
		public bool isFilled { get; protected set; }

		protected virtual void Start()
		{
			loadButton.onClick.AddListener(Load);
			deleteButton.onClick.AddListener(Delete);
			newGameButton.onClick.AddListener(Create);
		}

		/// <summary>
		/// Loads the game state from the save data.
		/// </summary>
		public virtual void Load() => Game.instance.LoadState(m_index, m_data);

		/// <summary>
		/// Deletes the save data from the save file.
		/// </summary>
		public virtual void Delete()
		{
			UIConfirmationScreen.instance.Show(
				title,
				message,
				() =>
				{
					GameSaver.instance.Delete(m_index);
					Fill(m_index, null);
					EventSystem.current.SetSelectedGameObject(newGameButton.gameObject);
				}
			);
		}

		/// <summary>
		/// Creates a new save data and saves it to the save file.
		/// </summary>
		public virtual void Create()
		{
			var data = GameData.Create();
			GameSaver.instance.Save(data, m_index);
			Fill(m_index, data);
			EventSystem.current.SetSelectedGameObject(loadButton.gameObject);
		}

		/// <summary>
		/// Fills the card with the save data.
		/// </summary>
		/// <param name="index">The index of the save data.</param>
		/// <param name="data">The save data to fill the card with.</param>
		public virtual void Fill(int index, GameData data)
		{
			m_index = index;
			isFilled = data != null;
			dataContainer.SetActive(isFilled);
			emptyContainer.SetActive(!isFilled);
			loadButton.interactable = isFilled;
			deleteButton.interactable = isFilled;
			newGameButton.interactable = !isFilled;

			if (data != null)
			{
				m_data = data;
				retries.text = data.retries.ToString(retriesFormat);
				stars.text = data.GetTotalAmountOf("star").ToString(starsFormat);
				coins.text = data.GetTotalAmountOf("coin").ToString(coinsFormat);
				createdAt.text = System
					.DateTime.Parse(data.createdAt)
					.ToLocalTime()
					.ToString(dateFormat);
				updatedAt.text = System
					.DateTime.Parse(data.updatedAt)
					.ToLocalTime()
					.ToString(dateFormat);
			}
		}
	}
}
