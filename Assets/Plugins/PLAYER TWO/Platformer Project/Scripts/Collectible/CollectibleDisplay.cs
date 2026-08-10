using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Collectible/Collectible Display")]
	public class CollectibleDisplay : MonoBehaviour
	{
		public enum Source
		{
			LevelScore,
			LevelData,
			Inventory,
		}

		[Header("General Settings")]
		[Tooltip("The source to get the amount of collected items from.")]
		public Source source = Source.LevelScore;

		[Tooltip("The reference of the collectible to display.")]
		public string reference;

		[Header("UI Elements")]
		[Tooltip("The text to display the amount of collected items.")]
		public TMP_Text text;

		[Tooltip("The format to display the amount of collected items.")]
		public string textFormat = "000";

		[Tooltip("The images to display the collected items.")]
		public Image[] images;

		protected Game m_game => Game.instance;
		protected Level m_level => Level.instance;
		protected LevelScore m_levelScore => LevelScore.instance;

		protected virtual void Awake()
		{
			InitializeCallbacks();
		}

		protected virtual void InitializeCallbacks()
		{
			switch (source)
			{
				default:
				case Source.LevelScore:
					InitializeLevelScoreCallbacks();
					break;
				case Source.LevelData:
					InitializeLevelDataCallbacks();
					break;
				case Source.Inventory:
					InitializeInventoryCallbacks();
					break;
			}
		}

		protected virtual void InitializeLevelScoreCallbacks()
		{
			if (!m_levelScore)
				return;

			m_levelScore.OnScoreLoaded.AddListener(() =>
			{
				m_levelScore.OnCollectibleSet.AddListener(_ => Refresh());
				m_levelScore.OnCollectibleAdded.AddListener(_ => Refresh());
				Refresh();
			});
		}

		protected virtual void InitializeLevelDataCallbacks()
		{
			if (!m_level)
				return;

			m_level.onLevelInitialized.AddListener(Refresh);
			Refresh();
		}

		protected virtual void InitializeInventoryCallbacks()
		{
			if (m_game.inventory != null)
			{
				m_game.inventory.OnInventoryChanged.AddListener(Refresh);
				Refresh();
				return;
			}

			m_game.onLoadState.AddListener(_ =>
			{
				m_game.inventory.OnInventoryChanged.AddListener(Refresh);
				Refresh();
			});
		}

		protected virtual void Refresh()
		{
			switch (source)
			{
				default:
				case Source.LevelScore:
					RefreshText(m_levelScore.collectibles);
					RefreshImages(m_levelScore.collectibles);
					break;
				case Source.LevelData:
					RefreshText(m_level.gameLevel.collectibles);
					RefreshImages(m_level.gameLevel.collectibles);
					break;
				case Source.Inventory:
					RefreshText(m_game.inventory.items);
					RefreshImages(m_game.inventory.items);
					break;
			}
		}

		protected virtual void RefreshText(CollectibleInstanceList list)
		{
			if (!text)
				return;

			var amount = list.SumAmount(reference);
			text.text = amount.ToString(textFormat);
		}

		protected virtual void RefreshImages(CollectibleInstanceList list)
		{
			if (images == null || images.Length == 0)
				return;

			var amount = list.SumAmount(reference);

			for (int i = 0; i < images.Length; i++)
				images[i].enabled = i < amount;
		}
	}
}
