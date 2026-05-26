using System.Collections.Generic;

namespace PLAYERTWO.PlatformerProject
{
	[System.Serializable]
	public class CollectibleInstance
	{
		/// <summary>
		/// The collectible profile reference.
		/// </summary>
		public CollectibleProfile profile;

		/// <summary>
		/// The amount of collected items from this collectible.
		/// </summary>
		public int amount;

		/// <summary>
		/// The unique identifiers of the "collect once" collectibles.
		/// If the collectible profile is not set to "collect once", this will be empty.
		/// </summary>
		public List<string> identifiers = new();

		/// <summary>
		/// The reference of the collectible profile.
		/// </summary>
		public string reference => profile ? profile.reference : string.Empty;

		/// <summary>
		/// Creates a copy of the current CollectibleInstance.
		/// </summary>
		/// <returns>A new CollectibleInstance with the same data.</returns>
		public virtual CollectibleInstance Clone() =>
			new()
			{
				profile = profile,
				amount = amount,
				identifiers = identifiers != null ? new(identifiers) : null,
			};

		/// <summary>
		/// Creates a new CollectibleInstance from a given Collectible.
		/// </summary>
		/// <param name="collectible">The collectible to create the instance from.</param>
		/// <returns>The created CollectibleInstance.</returns>
		public static CollectibleInstance Create(Collectible collectible)
		{
			if (!collectible)
				return null;

			return new CollectibleInstance()
			{
				profile = collectible.profile,
				amount = 1,
				identifiers = collectible.profile.collectOnce
					? new() { collectible.identifier }
					: null,
			};
		}

		/// <summary>
		/// Creates collectible instances from collectible serializers.
		/// </summary>
		/// <param name="data">The collectible serializers to create the instances from.</param>
		/// <returns>The created collectible instances.</returns>
		public static CollectibleInstanceList CreateFromData(CollectibleSerializer[] data)
		{
			if (data == null || data.Length == 0)
				return new CollectibleInstanceList();

			var instances = new CollectibleInstanceList();

			foreach (var item in data)
			{
				if (item.profileId < 0 || item.profileId >= Game.instance.collectibles.Count)
					continue;

				var profile = Game.instance.collectibles[item.profileId];
				var instance = new CollectibleInstance()
				{
					profile = profile,
					amount = item.amount,
					identifiers = profile.collectOnce ? new(item.identifiers) : null,
				};

				instances.Add(instance);
			}

			return instances;
		}
	}
}
