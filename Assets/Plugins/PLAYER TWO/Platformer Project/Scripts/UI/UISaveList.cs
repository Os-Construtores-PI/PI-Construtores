using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Save List")]
	public class UISaveList : MonoBehaviour
	{
		[Header("Settings")]
		[Tooltip("If true, the first element will be focused when the list is enabled.")]
		public bool focusFirstElement = true;

		[Tooltip("The save card prefab that will be instantiated for each save.")]
		public UISaveCard card;

		[Tooltip("The container that will hold the save cards.")]
		public RectTransform container;

		protected List<UISaveCard> m_cardList = new();

		protected GameData[] m_data => GameSaver.instance.LoadList();

		protected virtual void OnEnable()
		{
			RefreshSaveList();
			FocusFirstElement();
		}

		protected virtual void RefreshSaveList()
		{
			for (int i = 0; i < m_data.Length; i++)
			{
				if (m_cardList.Count <= i)
					m_cardList.Add(Instantiate(card, container));

				m_cardList[i].Fill(i, m_data[i]);
			}
		}

		protected virtual void FocusFirstElement()
		{
			if (!focusFirstElement || !EventSystem.current)
				return;

			if (m_cardList[0].isFilled)
				EventSystem.current.SetSelectedGameObject(m_cardList[0].loadButton.gameObject);
			else
				EventSystem.current.SetSelectedGameObject(m_cardList[0].newGameButton.gameObject);
		}
	}
}
