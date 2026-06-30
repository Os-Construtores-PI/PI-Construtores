using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Gravity Handler")]
	public class GravityHandler : MonoBehaviour
	{
		public enum Mode
		{
			Rigidbody,
			Transform,
		}

		public Mode mode = Mode.Rigidbody;

		protected Rigidbody m_rigidbody;
		protected Collider m_currentField;
		protected GravityField m_gravityField;
		protected GravityField m_tempGravityField;

		public Vector3 gravityDirection { get; protected set; } = new Vector3(0, -1, 0);

		protected virtual void InitializeRigidbody()
		{
			if (mode != Mode.Rigidbody)
				return;

			m_rigidbody = GetComponent<Rigidbody>();
			m_rigidbody.useGravity = false;
		}

		protected virtual void UpdateGravityDirection()
		{
			if (!m_gravityField)
				return;

			gravityDirection = m_gravityField.GetGravityDirectionFrom(transform.position);
		}

		protected virtual void SetGravityField(Collider collider)
		{
			m_currentField = collider;
			m_gravityField = m_tempGravityField;
		}

		protected virtual void Start()
		{
			InitializeRigidbody();
		}

		protected virtual void Update()
		{
			if (mode != Mode.Transform)
				return;

			UpdateGravityDirection();

			var rotation = Quaternion.FromToRotation(transform.up, -gravityDirection);
			transform.rotation = rotation * transform.rotation;
		}

		protected virtual void FixedUpdate()
		{
			if (mode != Mode.Rigidbody || m_rigidbody.isKinematic)
				return;

			UpdateGravityDirection();

			var multiplier = m_gravityField ? m_gravityField.gravityMultiplier : 1;
			var force = multiplier * Physics.gravity.magnitude * gravityDirection;
#if UNITY_6000_0_OR_NEWER
			m_rigidbody.linearVelocity += force * Time.deltaTime;
#else
			m_rigidbody.velocity += force * Time.deltaTime;
#endif
		}

		protected virtual void OnTriggerExit(Collider other)
		{
			if (m_currentField != other || !m_gravityField.detachOnExit)
				return;

			m_gravityField = null;
		}

		protected virtual void OnTriggerStay(Collider other)
		{
			if (!other.CompareTag(GameTags.GravityField) || m_currentField == other)
				return;
			if (!other.TryGetComponent(out m_tempGravityField))
				return;

			if (!m_gravityField)
				SetGravityField(other);
			else if (m_tempGravityField.priority > m_gravityField.priority)
				SetGravityField(other);
			else if (m_tempGravityField.priority == m_gravityField.priority)
			{
				var currentDistance = Vector3.Distance(transform.position, m_gravityField.center);
				var candidateDistance = Vector3.Distance(
					transform.position,
					m_tempGravityField.center
				);

				if (candidateDistance < currentDistance)
					SetGravityField(other);
			}
		}
	}
}
