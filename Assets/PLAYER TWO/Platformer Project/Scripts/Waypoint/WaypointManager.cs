using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Waypoint/Waypoint Manager")]
	public class WaypointManager : MonoBehaviour
	{
		[Tooltip("The way the Waypoint Manager will traverse the Waypoints.")]
		public WaypointMode mode;

		[Tooltip("The time in seconds the Waypoint Manager will wait before moving to the next Waypoint.")]
		public float waitTime;

		[Tooltip("The list of Waypoints the Waypoint Manager will traverse.")]
		public List<Transform> waypoints;

		protected Transform m_current;
		protected Transform m_previous;

		protected bool m_pong;
		protected bool m_changing;

		/// <summary>
		/// The instance of the current Waypoint.
		/// </summary>
		public Transform current
		{
			get
			{
				if (!m_current)
				{
					m_current = waypoints[0];
				}

				return m_current;
			}

			protected set { m_current = value; }
		}

		/// <summary>
		/// The instance of the previous Waypoint.
		/// </summary>
		public Transform previous
		{
			get
			{
				if (!m_previous)
				{
					m_previous = waypoints[0];
				}

				return m_previous;
			}

			protected set { m_previous = value; }
		}

		/// <summary>
		/// Returns the index of the current Waypoint.
		/// </summary>
		public int index => waypoints.IndexOf(current);

		/// <summary>
		/// Changes the current Waypoint to the next one based on the Waypoint Mode.
		/// </summary>
		public virtual void Next()
		{
			if (m_changing)
			{
				return;
			}

			if (mode == WaypointMode.PingPong)
			{
				if (!m_pong)
				{
					m_pong = (index + 1 == waypoints.Count);
				}
				else
				{
					m_pong = (index - 1 >= 0);
				}

				var next = !m_pong ? index + 1 : index - 1;
				StartCoroutine(Change(next));
			}
			else if (mode == WaypointMode.Loop)
			{
				if (index + 1 < waypoints.Count)
				{
					StartCoroutine(Change(index + 1));
				}
				else
				{
					StartCoroutine(Change(0));
				}
			}
			else if (mode == WaypointMode.Once)
			{
				if (index + 1 < waypoints.Count)
				{
					StartCoroutine(Change(index + 1));
				}
			}
		}

		protected virtual IEnumerator Change(int to)
		{
			m_changing = true;
			yield return new WaitForSeconds(waitTime);
			previous = current;
			current = waypoints[to];
			m_changing = false;
		}
	}
}
