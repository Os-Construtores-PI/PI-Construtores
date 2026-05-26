using System.Linq;

namespace PLAYERTWO.PlatformerProject
{
	[System.Serializable]
	public class CollectibleSerializer
	{
		/// <summary>
		/// The collectible profile id.
		/// </summary>
		public int profileId;

		/// <summary>
		/// The collectible reference.
		/// </summary>
		public string reference;

		/// <summary>
		/// The amount of collected items from this collectible.
		/// </summary>
		public int amount;

		/// <summary>
		/// The unique identifiers of the "collect once" collectibles.
		/// If the collectible profile is not set to "collect once", this will be null.
		/// </summary>
		public string[] identifiers;

		/// <summary>
		/// Creates collectible serializers from collectible instances.
		/// </summary>
		/// <param name="instances">The collectible instances to create the serializers from.</param>
		/// <returns>The created collectible serializers.</returns>
		public static CollectibleSerializer[] CreateFromInstances(CollectibleInstance[] instances)
		{
			if (instances == null || instances.Length == 0)
				return new CollectibleSerializer[0];

			return instances
				.Select(
					(instance) =>
						new CollectibleSerializer()
						{
							profileId = Game.instance.collectibles.IndexOf(instance.profile),
							reference = instance.reference,
							amount = instance.amount,
							identifiers = instance.identifiers?.ToArray(),
						}
				)
				.ToArray();
		}
	}
}
