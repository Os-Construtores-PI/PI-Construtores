using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Container")]
	public class UIContainer : MonoBehaviour
	{
		[Header("Containers")]
		[Tooltip("The container that will slide in from top.")]
		public RectTransform header;

		[Tooltip("The container that will slide in from the left.")]
		public RectTransform left;

		[Tooltip("The container that will slide in from the right.")]
		public RectTransform right;

		[Tooltip("The container that will zoom in from the center.")]
		public RectTransform center;

		[Tooltip("The container that will slide in from the bottom.")]
		public RectTransform footer;

		[Tooltip("The container that will fade in and out.")]
		public RectTransform background;

		[Header("Animation Settings")]
		[Tooltip("How long it takes for the containers to show in.")]
		[Min(0.001f)]
		public float showDuration = 0.2f;

		[Tooltip("How long it takes for the containers to hide out.")]
		[Min(0.001f)]
		public float hideDuration = 0.2f;

		[Tooltip(
			"How long to wait before the containers trigger the onComplete event when showing."
		)]
		[Min(0.001f)]
		public float showCompleteDelay = 0.1f;

		[Tooltip(
			"How long to wait before the containers trigger the onComplete event when hiding."
		)]
		[Min(0.001f)]
		public float hideCompleteDelay = 0.1f;

		[Header("Miscellaneous")]
		[Tooltip("If true, the containers will be hidden on awake.")]
		public bool hiddenOnAwake;

		[Tooltip("The object to focus on when the containers are shown.")]
		public GameObject focusObject;

		protected float m_leftWidth;
		protected float m_rightWidth;
		protected float m_headerHeight;
		protected float m_footerHeight;
		protected float m_centerScale;

		protected CanvasGroup m_canvasGroup;
		protected CanvasGroup m_centerCanvasGroup;
		protected CanvasGroup m_backgroundCanvasGroup;

		protected WaitForSecondsRealtime m_showWait;
		protected WaitForSecondsRealtime m_hideWait;
		protected WaitForSecondsRealtime m_showCompleteWait;
		protected WaitForSecondsRealtime m_hideCompleteWait;

		protected virtual void Awake()
		{
			InitializeCanvasGroup();
			InitializeState();
			InitializeYieldInstructions();
			InitializeVisibility();
		}

		protected virtual void InitializeCanvasGroup()
		{
			if (!TryGetComponent(out m_canvasGroup))
				m_canvasGroup = gameObject.AddComponent<CanvasGroup>();

			if (center && !center.TryGetComponent(out m_centerCanvasGroup))
				m_centerCanvasGroup = center.gameObject.AddComponent<CanvasGroup>();

			if (background && !background.TryGetComponent(out m_backgroundCanvasGroup))
				m_backgroundCanvasGroup = background.gameObject.AddComponent<CanvasGroup>();
		}

		protected virtual void InitializeState()
		{
			if (left)
				m_leftWidth = left.rect.width;

			if (right)
				m_rightWidth = right.rect.width;

			if (header)
				m_headerHeight = header.rect.height;

			if (footer)
				m_footerHeight = footer.rect.height;

			if (center)
				m_centerScale = center.localScale.x;
		}

		protected virtual void InitializeVisibility()
		{
			if (!hiddenOnAwake)
				return;

			SetAnchorPosition(left, new Vector2(-m_leftWidth, 0));
			SetAnchorPosition(right, new Vector2(m_rightWidth, 0));
			SetAnchorPosition(header, new Vector2(0, m_headerHeight));
			SetAnchorPosition(footer, new Vector2(0, -m_footerHeight));
			SetScale(center, 1.1f);
			SetAlpha(m_centerCanvasGroup, 0f);
			SetAlpha(m_backgroundCanvasGroup, 0f);
			m_canvasGroup.interactable = false;
			m_canvasGroup.blocksRaycasts = false;
		}

		protected virtual void InitializeYieldInstructions()
		{
			m_showWait = new WaitForSecondsRealtime(showDuration);
			m_hideWait = new WaitForSecondsRealtime(hideDuration);
			m_showCompleteWait = new WaitForSecondsRealtime(showCompleteDelay);
			m_hideCompleteWait = new WaitForSecondsRealtime(hideCompleteDelay);
		}

		public virtual void Show(UnityAction onComplete = null)
		{
			StopAllCoroutines();
			StartCoroutine(ShowRoutine(onComplete));
		}

		public virtual void Hide(UnityAction onComplete = null)
		{
			StopAllCoroutines();
			StartCoroutine(HideRoutine(onComplete));
		}

		protected virtual void SetAnchorPosition(RectTransform target, Vector2 position)
		{
			if (target)
				target.anchoredPosition = position;
		}

		protected virtual void SetScale(RectTransform target, float scale)
		{
			if (target)
				target.localScale = new Vector3(scale, scale, 1f);
		}

		protected virtual void SetAlpha(CanvasGroup target, float alpha)
		{
			if (target)
				target.alpha = alpha;
		}

		public virtual void SetActive(bool value) => gameObject.SetActive(value);

		protected virtual void FocusGameObject()
		{
			if (!focusObject || !EventSystem.current)
				return;

			EventSystem.current.SetSelectedGameObject(focusObject);
		}

		protected IEnumerator ShowRoutine(UnityAction onComplete)
		{
			if (left)
				StartCoroutine(
					AnimatePosition(left, left.anchoredPosition, Vector2.zero, showDuration)
				);

			if (right)
				StartCoroutine(
					AnimatePosition(right, right.anchoredPosition, Vector2.zero, showDuration)
				);

			if (header)
				StartCoroutine(
					AnimatePosition(header, header.anchoredPosition, Vector2.zero, showDuration)
				);

			if (footer)
				StartCoroutine(
					AnimatePosition(footer, footer.anchoredPosition, Vector2.zero, showDuration)
				);

			if (center)
			{
				StartCoroutine(AnimateScale(center, 1.1f, m_centerScale, showDuration));
				StartCoroutine(AnimateAlpha(m_centerCanvasGroup, 0f, 1f, showDuration));
			}

			if (background)
				StartCoroutine(AnimateAlpha(m_backgroundCanvasGroup, 0f, 1f, showDuration));

			yield return m_showWait;
			m_canvasGroup.interactable = true;
			m_canvasGroup.blocksRaycasts = true;
			FocusGameObject();
			yield return m_showCompleteWait;
			onComplete?.Invoke();
		}

		protected IEnumerator HideRoutine(UnityAction onComplete)
		{
			if (left)
				StartCoroutine(
					AnimatePosition(
						left,
						left.anchoredPosition,
						new Vector2(-m_leftWidth, 0),
						hideDuration
					)
				);

			if (right)
				StartCoroutine(
					AnimatePosition(
						right,
						right.anchoredPosition,
						new Vector2(m_rightWidth, 0),
						hideDuration
					)
				);

			if (header)
				StartCoroutine(
					AnimatePosition(
						header,
						header.anchoredPosition,
						new Vector2(0, m_headerHeight),
						hideDuration
					)
				);

			if (footer)
				StartCoroutine(
					AnimatePosition(
						footer,
						footer.anchoredPosition,
						new Vector2(0, -m_footerHeight),
						hideDuration
					)
				);

			if (center)
			{
				StartCoroutine(AnimateScale(center, m_centerScale, 1.1f, 0.5f));
				StartCoroutine(AnimateAlpha(m_centerCanvasGroup, 1f, 0f, hideDuration));
			}

			if (background)
				StartCoroutine(AnimateAlpha(m_backgroundCanvasGroup, 1f, 0f, hideDuration));

			m_canvasGroup.interactable = false;
			m_canvasGroup.blocksRaycasts = false;
			yield return m_hideWait;
			yield return m_hideCompleteWait;
			onComplete?.Invoke();
		}

		protected IEnumerator AnimatePosition(
			RectTransform target,
			Vector2 from,
			Vector2 to,
			float duration
		)
		{
			var time = 0f;

			while (time < duration)
			{
				time += Time.unscaledDeltaTime;
				target.anchoredPosition = Vector2.Lerp(from, to, time / duration);
				yield return null;
			}
		}

		protected IEnumerator AnimateScale(
			RectTransform target,
			float from,
			float to,
			float duration
		)
		{
			var time = 0f;

			while (time < duration)
			{
				time += Time.unscaledDeltaTime;
				var scale = Mathf.Lerp(from, to, time / duration);
				target.localScale = new Vector3(scale, scale, 1f);
				yield return null;
			}
		}

		protected IEnumerator AnimateAlpha(CanvasGroup target, float from, float to, float duration)
		{
			var time = 0f;

			while (time < duration)
			{
				time += Time.unscaledDeltaTime;
				target.alpha = Mathf.Lerp(from, to, time / duration);
				yield return null;
			}
		}
	}
}
