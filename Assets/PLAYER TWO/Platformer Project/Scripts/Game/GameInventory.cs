using UnityEngine.Events;

namespace PLAYERTWO.PlatformerProject
{
	[System.Serializable]
	public class GameInventory
	{
		/// <summary>
		/// The list of items in the inventory.
		/// </summary>
		public CollectibleInstanceList items;

		public UnityEvent OnInventoryChanged;

		public GameInventory()
		{
			items = new CollectibleInstanceList();
		}

		public GameInventory(CollectibleSerializer[] data)
		{
			items = CollectibleInstance.CreateFromData(data);
		}

		/// <summary>
		/// Adds an item to the inventory.
		/// </summary>
		/// <param name="instance">The instance to add.</param>
		public virtual void Add(CollectibleInstance instance)
		{
			items.AddOrStack(instance);
			OnInventoryChanged?.Invoke();
		}

		/// <summary>
		/// Adds multiple items to the inventory.
		/// </summary>
		/// <param name="instances">The instances to add.</param>
		public virtual void AddMany(CollectibleInstanceList instances)
		{
			items.AddOrStackMany(instances);
			OnInventoryChanged?.Invoke();
		}

		/// <summary>
		/// Tries to expend an item from the inventory.
		/// </summary>
		/// <param name="reference">The reference of the item to expend.</param>
		/// <returns>True if the item was successfully expended, false otherwise.</returns>
		public virtual bool TryExpend(string reference)
		{
			var instance = items.Find(i => i.reference == reference);

			if (instance == null || instance.amount <= 0)
				return false;

			instance.amount--;
			OnInventoryChanged?.Invoke();
			return true;
		}

		/// <summary>
		/// Returns the inventory data as an array of serializable objects.
		/// </summary>
		/// <returns>The array of serializable objects.</returns>
		public virtual CollectibleSerializer[] ToData() =>
			CollectibleSerializer.CreateFromInstances(items.ToArray());
	}
}
