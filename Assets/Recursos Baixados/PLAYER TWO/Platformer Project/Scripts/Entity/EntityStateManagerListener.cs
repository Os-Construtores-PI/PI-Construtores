using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Entity/Entity State Manager Listener")]
	public class EntityStateManagerListener : MonoBehaviour
	{
		public UnityEvent onEnter;
		public UnityEvent onExit;

		public List<string> states;

		protected EntityStateManager m_manager;

		protected virtual void OnEnter(Type state)
		{
			if (states.Contains(state.Name))
			{
				onEnter.Invoke();
			}
		}

		protected virtual void OnExit(Type state)
		{
			if (states.Contains(state.Name))
			{
				onExit.Invoke();
			}
		}

		protected virtual void Start()
		{
			if (!m_manager)
			{
				m_manager = GetComponentInParent<EntityStateManager>();
			}

			m_manager.events.onEnter.AddListener(OnEnter);
			m_manager.events.onExit.AddListener(OnExit);
		}
	}
}
