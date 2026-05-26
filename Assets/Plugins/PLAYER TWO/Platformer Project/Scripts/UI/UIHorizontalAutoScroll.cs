using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(ScrollRect))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Horizontal Auto Scroll")]
	public class UIHorizontalAutoScroll : MonoBehaviour
	{
		public float scrollDuration = 0.25f;

		protected ScrollRect m_scrollRect;
		protected GameObject m_currentSelected;
		protected Dictionary<GameObject, int> m_selectableIndices = new();

		public ScrollRect scrollRect
		{
			get
			{
				if (m_scrollRect == null)
					m_scrollRect = GetComponent<ScrollRect>();

				return m_scrollRect;
			}
		}

		protected virtual void Start() => InitializeSelectableHash();

		protected virtual void OnEnable() => ResetScrollRect();

		protected virtual void InitializeSelectableHash()
		{
			for (var i = 0; i < scrollRect.content.childCount; i++)
			{
				foreach (
					var selectable in scrollRect
						.content.GetChild(i)
						.GetComponentsInChildren<Selectable>(true)
				)
					m_selectableIndices.Add(selectable.gameObject, i);
			}
		}

		protected virtual void ResetScrollRect()
		{
			scrollRect.horizontalNormalizedPosition = 0;
		}

		protected virtual void Scroll(int childIndex)
		{
			StopAllCoroutines();
			StartCoroutine(ScrollRoutine(childIndex));
		}

		protected virtual IEnumerator ScrollRoutine(int childIndex)
		{
			var initial = scrollRect.horizontalNormalizedPosition;
			var target = childIndex / ((float)scrollRect.content.childCount - 1);
			var elapsedTime = 0f;

			while (elapsedTime < scrollDuration)
			{
				elapsedTime += Time.deltaTime;
				scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
					initial,
					target,
					elapsedTime / scrollDuration
				);
				yield return null;
			}

			scrollRect.horizontalNormalizedPosition = target;
		}

		protected virtual void Update()
		{
			if (
				m_currentSelected != EventSystem.current.currentSelectedGameObject
				&& m_selectableIndices.ContainsKey(EventSystem.current.currentSelectedGameObject)
			)
			{
				m_currentSelected = EventSystem.current.currentSelectedGameObject;
				Scroll(m_selectableIndices[m_currentSelected]);
			}
		}
	}
}
