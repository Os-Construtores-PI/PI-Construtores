using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collider))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Entity/Entity Hitbox")]
	public class EntityHitbox : MonoBehaviour
	{
		[Header("Attack Settings")]
		public bool breakObjects;
		public bool onlyDamageOnFall;
		public int damage = 1;

		[Header("Rebound Settings")]
		public bool rebound;
		public bool reboundOnBreak;
		public float reboundMinForce = 10f;
		public float reboundMaxForce = 25f;

		[Header("Push Back Settings")]
		public bool pushBack;
		public bool pushBackOnBreak;
		public float pushBackMinMagnitude = 5f;
		public float pushBackMaxMagnitude = 10f;

		protected Entity m_entity;
		protected Collider m_collider;

		protected virtual void InitializeTag() => tag = GameTags.Hitbox;

		protected virtual void InitializeEntity()
		{
			if (m_entity) return;

			m_entity = GetComponentInParent<Entity>();
		}

		protected virtual void InitializeCollider()
		{
			m_collider = GetComponent<Collider>();
			m_collider.isTrigger = true;
		}

		protected virtual void HandleCollision(Collider other)
		{
			if (other == m_entity.controller) return;
			if (onlyDamageOnFall && (m_entity.isGrounded || m_entity.verticalVelocity.y > 0)) return;

			if (other.TryGetComponent(out Entity target))
			{
				HandleEntityAttack(target);
				HandleRebound();
				HandlePushBack();
			}
			else if (other.TryGetComponent(out Breakable breakable))
			{
				HandleBreakableObject(breakable);
			}
		}

		protected virtual void HandleEntityAttack(Entity other)
		{
			other.ApplyDamage(damage, transform.position);
		}

		protected virtual void HandleBreakableObject(Breakable breakable)
		{
			if (!breakObjects) return;

			breakable.ApplyDamage(damage);

			if (reboundOnBreak)
				HandleRebound();

			if (pushBackOnBreak)
				HandlePushBack();
		}

		protected virtual void HandleRebound()
		{
			if (!rebound) return;

			var force = -m_entity.verticalVelocity.y;
			force = Mathf.Clamp(force, reboundMinForce, reboundMaxForce);
			m_entity.verticalVelocity = Vector3.up * force;
		}

		protected virtual void HandlePushBack()
		{
			if (!pushBack) return;

			var force = m_entity.lateralVelocity.magnitude;
			force = Mathf.Clamp(force, pushBackMinMagnitude, pushBackMaxMagnitude);
			m_entity.lateralVelocity = -m_entity.localForward * force;
		}

		protected virtual void HandleCustomCollision(Collider other) { }

		protected virtual void Start()
		{
			InitializeTag();
			InitializeEntity();
			InitializeCollider();
		}

		protected virtual void OnTriggerEnter(Collider other)
		{
			HandleCollision(other);
			HandleCustomCollision(other);
		}
	}
}
