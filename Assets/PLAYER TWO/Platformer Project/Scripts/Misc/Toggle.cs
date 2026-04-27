using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Toggle")]
	public class Toggle : MonoBehaviour
	{
		public bool state = true;
		public float delay;
		public Toggle[] multiTrigger;

		[Header("Auto Toggle Settings")]
		public bool autoToggle;
		public float autoToggleActiveDuration = 3f;
		public float autoToggleInactiveDuration = 1f;

		/// <summary>
		/// Called when the Toggle is activated.
		/// </summary>
		public UnityEvent onActivate;

		/// <summary>
		/// Called when the Toggle is deactivated.
		/// </summary>
		public UnityEvent onDeactivate;

		protected float m_lastToggleTime;

		/// <summary>
		/// Sets the state of the Toggle.
		/// </summary>
		/// <param name="value">The state you want to set.</param>
		public virtual void Set(bool value)
		{
			StopAllCoroutines();
			StartCoroutine(SetRoutine(value));
		}

		protected virtual IEnumerator SetRoutine(bool value)
		{
			m_lastToggleTime = Time.time;

			yield return new WaitForSeconds(delay);

			if (value)
			{
				if (!state)
				{
					state = true;

					foreach (var toggle in multiTrigger)
					{
						toggle.Set(state);
					}

					onActivate?.Invoke();
				}
			}
			else if (state)
			{
				state = false;

				foreach (var toggle in multiTrigger)
				{
					toggle.Set(state);
				}

				onDeactivate?.Invoke();
			}
		}

		protected virtual void Update()
		{
			if (!autoToggle)
				return;

			if (state && Time.time - m_lastToggleTime > autoToggleActiveDuration)
				Set(false);
			else if (!state && Time.time - m_lastToggleTime > autoToggleInactiveDuration)
				Set(true);
		}
	}
}
