using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Health")]
	public class Health : MonoBehaviour
	{
		public int initial = 3;
		public int max = 3;
		public float coolDown = 1f;

		/// <summary>
		/// Called when the health count changed.
		/// </summary>
		public UnityEvent onChange;

		/// <summary>
		/// Called when it receives damage.
		/// </summary>
		public UnityEvent onDamage;

		protected int m_currentHealth;
		protected float m_lastDamageTime;

		/// <summary>
		/// Returns the current amount of health.
		/// </summary>
		public int current
		{
			get { return m_currentHealth; }

			protected set
			{
				var last = m_currentHealth;

				if (value != last)
				{
					m_currentHealth = Mathf.Clamp(value, 0, max);
					onChange?.Invoke();
				}
			}
		}

		/// <summary>
		/// Returns true if the Health is empty.
		/// </summary>
		public virtual bool isEmpty => current == 0;

		/// <summary>
		/// Returns true if it's still recovering from the last damage.
		/// </summary>
		public virtual bool recovering => Time.time < m_lastDamageTime + coolDown;

		/// <summary>
		/// Sets the current health to a given amount.
		/// </summary>
		/// <param name="amount">The total health you want to set.</param>
		public virtual void Set(int amount) => current = amount;

		/// <summary>
		/// Increases the amount of health.
		/// </summary>
		/// <param name="amount">The amount you want to increase.</param>
		public virtual void Increase(int amount) => current += amount;

		/// <summary>
		/// Decreases the amount of health.
		/// </summary>
		/// <param name="amount">The amount you want to decrease.</param>
		public virtual void Damage(int amount)
		{
			if (!recovering)
			{
				current -= Mathf.Abs(amount);
				m_lastDamageTime = Time.time;
				onDamage?.Invoke();
			}
		}

		/// <summary>
		/// Set the current health back to its initial value.
		/// </summary>
		public virtual void ResetHealth() => current = initial;

		protected virtual void Awake() => current = initial;
	}
}
