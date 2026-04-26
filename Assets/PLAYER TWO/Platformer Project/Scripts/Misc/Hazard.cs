using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[RequireComponent(typeof(Collider))]
	[AddComponentMenu("PLAYER TWO/Platformer Project/Misc/Hazard")]
	public class Hazard : MonoBehaviour, IEntityContact
	{
		public bool isSolid;
		public bool damageOnlyFromAbove;
		public int damage = 1;

		protected Collider m_collider;

		protected virtual void Awake()
		{
			tag = GameTags.Hazard;
			m_collider = GetComponent<Collider>();
			m_collider.isTrigger = !isSolid;
		}

		protected virtual void TryToApplyDamageTo(Player player)
		{
			if (!damageOnlyFromAbove || player.verticalVelocity.y < 0 &&
				BoundsHelper.IsBellowPoint(m_collider, player.stepPosition))
			{
				player.ApplyDamage(damage, transform.position);
			}
		}

		public virtual void OnEntityContact(Entity entity)
		{
			if (entity is Player player)
			{
				TryToApplyDamageTo(player);
			}
		}

		protected virtual void OnTriggerStay(Collider other)
		{
			if (other.CompareTag(GameTags.Player))
			{
				if (other.TryGetComponent<Player>(out var player))
				{
					TryToApplyDamageTo(player);
				}
			}
		}
	}
}
