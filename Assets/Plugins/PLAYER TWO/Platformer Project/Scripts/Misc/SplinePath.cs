using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(SplineContainer), typeof(Collider))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Spline Path")]
	public class SplinePath : MonoBehaviour
	{
		public struct SplineState
		{
			public bool backwards;
		}

		[SerializeField]
		protected Collider m_collider;

		[SerializeField]
		protected SplineContainer m_splineContainer;

		public int splineResolution = 128;

		protected Player m_tempPlayer;
		protected Dictionary<Player, SplineState> m_splineStates = new();

		protected virtual void InitializeCollider()
		{
			if (m_collider == null)
				m_collider = GetComponent<Collider>();

			m_collider.isTrigger = true;
		}

		protected virtual void InitializeSplineContainer()
		{
			if (m_splineContainer == null)
				m_splineContainer = GetComponent<SplineContainer>();
		}

		public virtual void AssignPlayer(Player player)
		{
			if (m_splineStates.ContainsKey(player) || !player.isAlive)
				return;

			var forward = GetSplineTangentFrom(player.position, out _);
			var dot = Vector3.Dot(forward, player.pathForward);

			m_splineStates.Add(player, new SplineState() { backwards = dot < 0f });
			player.splinePath = this;
		}

		public virtual void RemovePlayer(Player player)
		{
			if (!m_splineStates.ContainsKey(player))
				return;

			player.pathForward = GetPathForward(player, out _);
			m_splineStates.Remove(player);
			player.splinePath = null;
		}

		public virtual Vector3 GetPathForward(Player player, out Vector3 closestPoint)
		{
			var tangent = GetSplineTangentFrom(player.position, out closestPoint);
			var sign = m_splineStates[player].backwards ? -1f : 1f;
			return tangent * sign;
		}

		protected virtual Vector3 GetSplineTangentFrom(Vector3 point, out Vector3 closestPoint)
		{
			SplineUtility.GetNearestPoint(
				m_splineContainer.Spline,
				transform.InverseTransformPoint(point),
				out var nearest,
				out var t,
				splineResolution
			);
			closestPoint = transform.TransformPoint(nearest);
			return Vector3.Normalize(m_splineContainer.EvaluateTangent(t));
		}

		protected virtual void Awake()
		{
			InitializeCollider();
			InitializeSplineContainer();
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			if (GameTags.IsPlayer(other) && other.TryGetComponent(out m_tempPlayer))
				AssignPlayer(m_tempPlayer);
		}

		protected virtual void OnTriggerExit(Collider other)
		{
			if (GameTags.IsPlayer(other) && other.TryGetComponent(out m_tempPlayer))
				RemovePlayer(m_tempPlayer);
		}
	}
}
