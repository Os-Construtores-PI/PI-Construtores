using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[CreateAssetMenu(
		fileName = "New Collectible Profile",
		menuName = "PLAYER TWO/Platformer Project/Collectible/New Collectible Profile"
	)]
	public class CollectibleProfile : ScriptableObject
	{
		[Header("Collectible Settings")]
		[Tooltip(
			"If true, the collectible can only be collected once per level. "
				+ "After beating a level, it won't show up in following playthroughs. "
				+ "Useful for collectibles like stars, keys, or unique items."
		)]
		public bool collectOnce;

		[Tooltip(
			"A unique identifier for this collectible profile. "
				+ "Needs to be unique, but if two profiles shares the same reference, they will be treated as the same collectible. "
				+ "For example, coins and blue coins can share the same reference, as they are both coins. "
		)]
		public string reference;

		[Tooltip(
			"If true, collectibles using this profile will not be restored when the player respawns. "
				+ "Useful for collectibles that should only be obtainable once per session, such as lives or power-ups."
		)]
		public bool doNotRespawn;

		[Min(1)]
		[Tooltip(
			"The default amount to collect when this collectible is collected. "
				+ "For example, special coins can give more than one coin."
		)]
		public int collectionAmount = 1;
	}
}
