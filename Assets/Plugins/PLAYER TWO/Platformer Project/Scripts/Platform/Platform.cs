using UnityEngine;
using System.Collections.Generic;

namespace PLAYERTWO.PlatformerProject
{
	public abstract class Platform : MonoBehaviour
	{
		protected Vector3 m_lastPosition;
		protected Quaternion m_lastRotation;

		protected List<Transform> m_attachedTransforms = new();

		public abstract void PlatformUpdate();

		protected virtual void Awake()
		{
			tag = GameTags.Platform;
		}

		protected virtual void CacheTransform()
		{
			m_lastPosition = transform.position;
			m_lastRotation = transform.rotation;
		}

		protected virtual void HandleAttachedTransforms()
		{
			foreach (var attach in m_attachedTransforms)
			{
				var attachOffset = attach.position - transform.position;
				var positionOffset = transform.position - m_lastPosition;
				var rotationOffset = transform.rotation * Quaternion.Inverse(m_lastRotation);
				var finalOffset = attachOffset + positionOffset;
				attach.position = transform.position + rotationOffset * finalOffset;
				attach.rotation = rotationOffset * attach.rotation;
			}
		}

		/// <summary>
		/// Attach a transform to this platform.
		/// </summary>
		/// <param name="other">The transform you want to attach.</param>
		public virtual void Attach(Transform other)
		{
			if (other && !m_attachedTransforms.Contains(other))
				m_attachedTransforms.Add(other);
		}

		/// <summary>
		/// Detach a transform from this platform.
		/// </summary>
		/// <param name="other">The transform you want to detach.</param>
		public virtual void Detach(Transform other)
		{
			if (other && m_attachedTransforms.Contains(other))
				m_attachedTransforms.Remove(other);
		}

		/// <summary>
		/// Detach all transforms from this platform.
		/// </summary>
		public virtual void DetachAll()
		{
			m_attachedTransforms.Clear();
		}
	}
}
