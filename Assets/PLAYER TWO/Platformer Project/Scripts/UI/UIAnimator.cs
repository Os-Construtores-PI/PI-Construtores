using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Animator))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/UI/UI Animator")]
	public class UIAnimator : MonoBehaviour
	{
		/// <summary>
		/// Called when the Show action is invoked.
		/// </summary>
		public UnityEvent OnShow;

		/// <summary>
		/// Called when the Hide action is invoked.
		/// </summary>
		public UnityEvent OnHide;

		public bool hidenOnAwake;
		public string normalTrigger = "Normal";
		public string showTrigger = "Show";
		public string hideTrigger = "Hide";

		protected Animator m_animator;

		/// <summary>
		/// Triggers the UI Animator Show animation.
		/// </summary>
		public virtual void Show()
		{
			m_animator.SetTrigger(showTrigger);
			OnShow?.Invoke();
		}

		/// <summary>
		/// Triggers the UI Animator Hide animation.
		/// </summary>
		public virtual void Hide()
		{
			m_animator.SetTrigger(hideTrigger);
			OnHide?.Invoke();
		}

		/// <summary>
		/// Calls the Game Object Set Active passing a given value.
		/// </summary>
		/// <param name="value">The value you want to pass.</param>
		public virtual void SetActive(bool value) => gameObject.SetActive(value);

		protected virtual void Awake()
		{
			m_animator = GetComponent<Animator>();

			if (hidenOnAwake)
			{
				m_animator.Play(hideTrigger, 0, 1);
			}
		}
	}
}
