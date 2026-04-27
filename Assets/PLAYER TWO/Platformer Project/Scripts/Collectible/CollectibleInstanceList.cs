using System.Collections.Generic;
using System.Linq;

namespace PLAYERTWO.PlatformerProject
{
	[System.Serializable]
	public class CollectibleInstanceList : List<CollectibleInstance>
	{
		public CollectibleInstanceList()
			: base() { }

		public CollectibleInstanceList(CollectibleInstance[] instances)
			: base(instances) { }

		/// <summary>
		/// Adds a collectible to the list. If an instance with the same profile already exists,
		/// it increments the amount and adds the identifier if it's a "collect once" profile
		/// </summary>
		/// <param name="collectible">The collectible to add.</param>
		public virtual void AddOrStack(Collectible collectible)
		{
			if (collectible == null)
				return;

			if (Find((i) => i.profile == collectible.profile) is CollectibleInstance instance)
			{
				if (instance.profile.collectOnce)
				{
					if (!instance.identifiers.Contains(collectible.identifier))
					{
						instance.identifiers.Add(collectible.identifier);
						instance.amount = instance.identifiers.Count;
					}
				}
				else
				{
					instance.amount++;
				}

				return;
			}

			Add(CollectibleInstance.Create(collectible));
		}

		/// <summary>
		/// Adds a collectible instance to the list. If an instance with the same profile already exists,
		/// it increments the amount and merges identifiers
		/// </summary>
		/// <param name="newItem">The collectible instance to add.</param>
		public virtual void AddOrStack(CollectibleInstance newItem)
		{
			if (newItem == null)
				return;

			if (Find((i) => i.profile == newItem.profile) is CollectibleInstance currentItem)
			{
				if (currentItem.profile.collectOnce)
				{
					if (newItem.identifiers != null && newItem.identifiers.Count > 0)
					{
						var newIdentifiers = newItem
							.identifiers.Except(currentItem.identifiers)
							.ToList();
						currentItem.identifiers.AddRange(newIdentifiers);
						currentItem.amount = currentItem.identifiers.Count;
					}
				}
				else
				{
					currentItem.amount += newItem.amount;
				}

				return;
			}

			Add(newItem.Clone());
		}

		/// <summary>
		/// Adds or stacks multiple collectible instances to the list.
		/// </summary>
		/// <param name="instances">The collectible instances to add or stack.</param>
		public virtual void AddOrStackMany(CollectibleInstanceList instances)
		{
			if (instances == null || instances.Count == 0)
				return;

			foreach (var instance in instances)
				AddOrStack(instance);
		}

		/// <summary>
		/// Returns true if the collectible has been collected.
		/// </summary>
		/// <param name="collectible">The collectible to check for.</param>
		/// <returns>True if the collectible has been collected.</returns>
		public virtual bool HasCollected(Collectible collectible)
		{
			if (collectible == null)
				return false;

			return this.Any(
				(instance) =>
					string.Equals(instance.reference, collectible.reference)
					&& instance.identifiers.Contains(collectible.identifier)
			);
		}

		/// <summary>
		/// Returns the sum of amounts for a given collectible reference.
		/// </summary>
		/// <param name="reference">The collectible reference to sum amounts for.</param>
		/// <returns>The sum of amounts for the given collectible reference.</returns>
		public virtual int SumAmount(string reference) =>
			this.Where((instance) => string.Equals(instance.reference, reference))
				.Sum((instance) => instance.amount);

		/// <summary>
		/// Creates a deep copy of the current CollectibleInstanceList.
		/// </summary>
		public virtual CollectibleInstanceList Clone()
		{
			var clone = new CollectibleInstanceList();
			foreach (var instance in this)
				clone.Add(instance.Clone());
			return clone;
		}
	}
}
