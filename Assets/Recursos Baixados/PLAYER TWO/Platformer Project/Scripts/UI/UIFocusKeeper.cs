using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(EventSystem))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Focus Keeper")]
	public class UIFocusKeeper : MonoBehaviour
	{
		protected Selectable m_lastSelected;
		protected CanvasGroup m_lastSelectedCanvasGroup;
		protected EventSystem m_eventSystem;

		protected virtual void Start()
		{
			m_eventSystem = GetComponent<EventSystem>();
		}

		protected virtual void CacheLastSelected()
		{
			if (!m_eventSystem.currentSelectedGameObject)
				return;

			m_lastSelected = m_eventSystem.currentSelectedGameObject.GetComponent<Selectable>();
			m_lastSelectedCanvasGroup = m_lastSelected.GetComponentInParent<CanvasGroup>();
		}

		protected virtual void Update()
		{
			if (!m_eventSystem.currentSelectedGameObject)
			{
				if (
					m_lastSelected
					&& m_lastSelected.gameObject.activeSelf
					&& m_lastSelected.interactable
					&& (!m_lastSelectedCanvasGroup || m_lastSelectedCanvasGroup.interactable)
				)
				{
					m_eventSystem.SetSelectedGameObject(m_lastSelected.gameObject);
				}
			}
			else
			{
				CacheLastSelected();
			}
		}
	}
}
