using System;
using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[Serializable]
	public class EntityEvents
	{
		/// <summary>
		/// Called when the Entity lands on the ground.
		/// </summary>
		public UnityEvent OnGroundEnter;

		/// <summary>
		/// Called when the Entity leaves the ground.
		/// </summary>
		public UnityEvent OnGroundExit;

		/// <summary>
		/// Called when the Entity falls off the ground from a steep slope.
		/// </summary>
		public UnityEvent OnGroundFall;

		/// <summary>
		/// Called when the Entity enters a rail.
		/// </summary>
		public UnityEvent OnRailsEnter;

		/// <summary>
		/// Called when the Entity exists a rail.
		/// </summary>
		public UnityEvent OnRailsExit;
	}
}
