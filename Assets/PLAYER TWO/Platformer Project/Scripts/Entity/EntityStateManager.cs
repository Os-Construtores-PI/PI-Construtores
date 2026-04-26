using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	public abstract class EntityStateManager : MonoBehaviour
	{
		public EntityStateManagerEvents events;
	}

	public abstract class EntityStateManager<T> : EntityStateManager
		where T : Entity<T>
	{
		protected List<EntityState<T>> m_list = new List<EntityState<T>>();

		protected Dictionary<Type, EntityState<T>> m_states =
			new Dictionary<Type, EntityState<T>>();

		/// <summary>
		/// Returns the instance of the current Entity State.
		/// </summary>
		/// <value></value>
		public EntityState<T> current { get; protected set; }

		/// <summary>
		/// Returns the instance of the last Entity State.
		/// </summary>
		/// <value></value>
		public EntityState<T> last { get; protected set; }

		/// <summary>
		/// Return the index of the current Entity State.
		/// </summary>
		public int index => m_list.IndexOf(current);

		/// <summary>
		/// Return the index of the current Entity State.
		/// </summary>
		public int lastIndex => m_list.IndexOf(last);

		/// <summary>
		/// Return the instance of the Entity associated with this Entity State Manager.
		/// </summary>
		public T entity { get; protected set; }

		protected abstract List<EntityState<T>> GetStateList();

		protected Dictionary<Type, List<UnityAction>> m_enterListeners = new();
		protected Dictionary<Type, List<UnityAction>> m_exitListeners = new();

		protected virtual void InitializeEntity() => entity = GetComponent<T>();

		protected EntityState<T> m_initialState;

		protected virtual void InitializeStates()
		{
			m_list = GetStateList();

			foreach (var state in m_list)
			{
				var type = state.GetType();

				if (!m_states.ContainsKey(type))
				{
					m_states.Add(type, state);
				}
			}

			if (m_list.Count > 0)
			{
				m_initialState = m_list[0];
				current = m_initialState;
			}
		}

		/// <summary>
		/// Change to a given Entity State based on its index on the States list.
		/// </summary>
		/// <param name="to">The index of the State you want to change to.</param>
		public virtual void Change(int to)
		{
			if (to >= 0 && to < m_list.Count)
			{
				Change(m_list[to]);
			}
		}

		/// <summary>
		/// Change to a given Entity State based on its class type.
		/// </summary>
		/// <typeparam name="TState">The class of the state you want to change to.</typeparam>
		public virtual void Change<TState>()
			where TState : EntityState<T>
		{
			var type = typeof(TState);

			if (m_states.ContainsKey(type))
			{
				Change(m_states[type]);
				return;
			}

			foreach (var state in m_states)
			{
				if (type.IsAssignableFrom(state.Key))
				{
					Change(state.Value);
					return;
				}
			}
		}

		/// <summary>
		/// Changes to a given Entity State based on its instance.
		/// </summary>
		/// <param name="to">The instance of the Entity State you want to change to.</param>
		public virtual void Change(EntityState<T> to)
		{
			if (to != null && Time.timeScale > 0)
			{
				if (current != null)
				{
					current.Exit(entity);
					events.onExit.Invoke(current.GetType());
					last = current;
				}

				current = to;
				current.Enter(entity);
				events.onEnter.Invoke(current.GetType());
				events.onChange?.Invoke();
			}
		}

		/// <summary>
		/// Changes back to the initial Entity State (the first state in the list).
		/// </summary>
		public virtual void Reset()
		{
			if (m_initialState != null)
				Change(m_initialState);
		}

		/// <summary>
		/// Returns true if the type of the current State matches a given one.
		/// </summary>
		/// <param name="type">The type you want to compare to.</param>
		public virtual bool IsCurrentOfType(params Type[] types)
		{
			if (current == null)
				return false;

			foreach (var type in types)
			{
				if (IsCurrentOfType(type))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Returns true if the type of the current State matches a given one.
		/// </summary>
		/// <param name="type">The type you want to compare to.</param>
		/// <returns>True if the current State matches the given type, false otherwise.</returns>
		public virtual bool IsCurrentOfType(Type type)
		{
			if (current == null)
				return false;

			return current.GetType() == type || type.IsAssignableFrom(current.GetType());
		}

		/// <summary>
		/// Returns true if the manager has a State of a given type.
		/// </summary>
		/// <param name="type">The Type of the State you want to find.</param>
		public virtual bool ContainsStateOfType(Type type) => m_states.ContainsKey(type);

		/// <summary>
		/// Adds a listener to be called when entering a given State.
		/// </summary>
		/// <typeparam name="TState">The type of the State you want to listen to.</typeparam>
		/// <param name="action">The action to be called when entering the State.</param>
		public virtual void AddEnterListener<TState>(UnityAction action)
			where TState : EntityState<T>
		{
			var type = typeof(TState);

			if (m_enterListeners.ContainsKey(type))
			{
				m_enterListeners[type].Add(action);
				return;
			}

			m_enterListeners.Add(type, new List<UnityAction> { action });
			events.onEnter.AddListener(
				(enteredType) =>
				{
					if (enteredType == type)
					{
						foreach (var listener in m_enterListeners[type])
							listener?.Invoke();
					}
				}
			);
		}

		/// <summary>
		/// Adds a listener to be called when exiting a given State.
		/// </summary>
		/// <typeparam name="TState">The type of the State you want to listen to.</typeparam>
		/// <param name="action">The action to be called when exiting the State.</param>
		public virtual void AddExitListener<TState>(UnityAction action)
			where TState : EntityState<T>
		{
			var type = typeof(TState);

			if (m_exitListeners.ContainsKey(type))
			{
				m_exitListeners[type].Add(action);
				return;
			}

			m_exitListeners.Add(type, new List<UnityAction> { action });
			events.onExit.AddListener(
				(exitedType) =>
				{
					if (exitedType == type)
					{
						foreach (var listener in m_exitListeners[type])
							listener?.Invoke();
					}
				}
			);
		}

		public virtual void Step()
		{
			if (current != null && Time.timeScale > 0)
			{
				current.Step(entity);
			}
		}

		public virtual void OnContact(Collider other)
		{
			if (current != null && Time.timeScale > 0)
			{
				current.OnContact(entity, other);
			}
		}

		protected virtual void Start()
		{
			InitializeEntity();
			InitializeStates();
		}
	}
}
