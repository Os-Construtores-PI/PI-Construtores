using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Level Card")]
	public class UILevelCard : MonoBehaviour
	{
		[Header("Texts")]
		[Tooltip("References the text component for the level title.")]
		public TMP_Text title;

		[Tooltip("References the text component for the level description.")]
		public TMP_Text description;

		[Tooltip("References the text component for the coins collected in the level.")]
		public TMP_Text coins;

		[Tooltip("References the text component for the time taken to complete the level.")]
		public TMP_Text time;

		[Tooltip(
			"References the text component for the missing stars counter to unlock the level."
		)]
		public TMP_Text missingStarsText;

		[Header("Images")]
		[Tooltip("References the image component for the level image.")]
		public Image image;

		[Tooltip("References the image components for the stars collected in the level.")]
		public Image[] starsImages;

		[Header("Buttons")]
		[Tooltip("References the button component to play the level.")]
		public Button play;

		[Header("Containers")]
		[Tooltip("References the parent game object for the play button \"Play\" text.")]
		public GameObject playContainer;

		[Tooltip("References the parent game object for the missing stars counter.")]
		public GameObject missingStarsContainer;

		protected bool m_locked;
		protected GameLevel m_level;

		/// <summary>
		/// Indicates if the level is locked or not.
		/// </summary>
		public bool locked
		{
			get { return m_locked; }
			set
			{
				m_locked = value;
				play.interactable = !m_locked;
			}
		}

		protected virtual void Start()
		{
			play.onClick.AddListener(Play);
		}

		/// <summary>
		/// Loads the scene of the level currently set in the card.
		/// </summary>
		public virtual void Play() => m_level.StartLevel();

		/// <summary>
		/// Fills the card with the level data.
		/// </summary>
		/// <param name="level">The level data to fill the card with.</param>
		public virtual void Fill(GameLevel level)
		{
			if (level == null)
				return;

			m_level = level;
			RefreshUI();
			RefreshLockingState();
		}

		/// <summary>
		/// Refreshes the UI with the current level data.
		/// </summary>
		protected virtual void RefreshUI()
		{
			title.text = m_level.name;
			description.text = m_level.description;
			time.text = GameLevel.FormattedTime(m_level.bestScore.time);
			coins.text = m_level.bestScore.coins.ToString("000");
			image.sprite = m_level.image;

			for (int i = 0; i < starsImages.Length; i++)
			{
				starsImages[i].enabled = m_level.bestScore.stars > i;
			}
		}

		/// <summary>
		/// Refreshes the locking state of the level.
		/// </summary>
		protected virtual void RefreshLockingState()
		{
			if (m_level.requiredStars > 0)
			{
				var totalStars = Game.instance.GetTotalStars();
				locked = m_level.requiredStars > totalStars;
				missingStarsText.text = (m_level.requiredStars - totalStars).ToString();

				if (locked && playContainer && missingStarsContainer)
				{
					playContainer.SetActive(false);
					missingStarsContainer.SetActive(true);
				}

				return;
			}

			locked = m_level.locked;
		}
	}
}
