using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Level List")]
	public class UILevelList : MonoBehaviour
	{
		[Header("Settings")]
		[Tooltip("If true, the first element will be focused when the list is enabled.")]
		public bool focusFirstElement = true;

		[Tooltip("The level card prefab that will be instantiated for each level.")]
		public UILevelCard card;

		[Tooltip("The container that will hold the level cards.")]
		public RectTransform container;

		protected List<UILevelCard> m_cardList = new();

		protected List<GameLevel> m_levels => Game.instance.levels;

		protected virtual void OnEnable()
		{
			RefreshLevelList();
			FocusFirstElement();
		}

		protected virtual void RefreshLevelList()
		{
			for (int i = 0; i < m_levels.Count; i++)
			{
				if (m_cardList.Count <= i)
					m_cardList.Add(Instantiate(card, container));

				m_cardList[i].Fill(m_levels[i]);
			}
		}

		protected virtual void FocusFirstElement()
		{
			if (!focusFirstElement || !EventSystem.current)
				return;

			EventSystem.current.SetSelectedGameObject(m_cardList[0].play.gameObject);
		}
	}
}
