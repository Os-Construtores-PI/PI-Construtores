using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collider))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Conveyor Belt")]
	public class ConveyorBelt : MonoBehaviour, IEntityContact
	{
		[Header("Conveyor Settings")]
		public float speed = 5;

		[Header("Material Settings")]
		public Renderer beltRenderer;
		public int materialIndex;
		public float scrollMultiplier = 0.1f;

		protected Collider m_collider;
		protected Material m_material;

		public Vector3 velocity => transform.forward * speed;

		protected virtual void Start()
		{
			InitializeCollider();
			InitializeMaterial();
		}

		protected virtual void LateUpdate()
		{
			UpdateMaterialOffset();
		}

		protected virtual void InitializeCollider()
		{
			m_collider = GetComponent<Collider>();
		}

		protected virtual void InitializeMaterial()
		{
			if (!beltRenderer)
				return;

			var materials = beltRenderer.materials;

			if (materialIndex >= 0 && materialIndex < materials.Length)
				m_material = materials[materialIndex];
		}

		protected virtual void UpdateMaterialOffset()
		{
			if (!m_material)
				return;

			var offset = m_material.mainTextureOffset;
			var scroll = speed * scrollMultiplier;
			offset += new Vector2(0, scroll * Time.deltaTime);
			m_material.mainTextureOffset = offset;
		}

		public virtual void OnEntityContact(Entity entity)
		{
			if (BoundsHelper.IsBellowPoint(m_collider, entity.stepPosition))
				entity.position += velocity * Time.deltaTime;
		}

		protected virtual void OnCollisionStay(Collision collision)
		{
			var point = collision.GetContact(0).point;

			if (!BoundsHelper.IsBellowPoint(m_collider, point))
				return;

			if (collision.body is Rigidbody rb)
				rb.MovePosition(rb.position + velocity * Time.deltaTime);
		}
	}
}
