using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace PLAYERTWO.PlatformerProject
{
	public abstract class EntityState<T> where T : Entity<T>
	{
		public UnityEvent onEnter;
		public UnityEvent onExit;

		public float timeSinceEntered { get; protected set; }

		public void Enter(T entity)
		{
			timeSinceEntered = 0;
			onEnter?.Invoke();
			OnEnter(entity);
		}

		public void Exit(T entity)
		{
			onExit?.Invoke();
			OnExit(entity);
		}

		public void Step(T entity)
		{
			OnStep(entity);
			timeSinceEntered += Time.deltaTime;
		}

		/// <summary>
		/// Called when this State is invoked.
		/// </summary>
		protected abstract void OnEnter(T entity);

		/// <summary>
		/// Called when this State changes to another.
		/// </summary>
		protected abstract void OnExit(T entity);

		/// <summary>
		/// Called every frame where this State is activated.
		/// </summary>
		protected abstract void OnStep(T entity);

		/// <summary>
		/// Called when the Entity is in contact with a collider.
		/// </summary>
		public abstract void OnContact(T entity, Collider other);

		/// <summary>
		/// Returns a new instance of the Entity State with a given type name.
		/// </summary>
		/// <param name="typeName">The type name of the Entity State class.</param>
		public static EntityState<T> CreateFromString(string typeName)
		{
			return (EntityState<T>)System.Activator
				.CreateInstance(System.Type.GetType(typeName));
		}

		/// <summary>
		/// Returns a new list with instances of Entity States matching the array of type names.
		/// </summary>
		/// <param name="array">The array of type names.</param>
		public static List<EntityState<T>> CreateListFromStringArray(string[] array)
		{
			var list = new List<EntityState<T>>();

			foreach (var typeName in array)
			{
				list.Add(CreateFromString(typeName));
			}

			return list;
		}
	}
}
