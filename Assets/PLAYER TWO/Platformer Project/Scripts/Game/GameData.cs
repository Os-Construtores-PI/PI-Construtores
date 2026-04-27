using System;
using System.Linq;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[Serializable]
	public class GameData
	{
		public int retries;
		public LevelData[] levels;
		public CollectibleSerializer[] inventory;
		public string createdAt;
		public string updatedAt;

		/// <summary>
		/// Returns a new instance of Game Data at runtime.
		/// </summary>
		public static GameData Create()
		{
			return new GameData()
			{
				retries = Game.instance.initialRetries,
				createdAt = DateTime.UtcNow.ToString(),
				updatedAt = DateTime.UtcNow.ToString(),
				levels = Game
					.instance.levels.Select(
						(level) =>
						{
							return new LevelData()
							{
								locked = level.locked,
								bestScore = new LevelBestScore(),
								collectibles = new CollectibleSerializer[0],
							};
						}
					)
					.ToArray(),
				inventory = new CollectibleSerializer[0],
			};
		}

		/// <summary>
		/// Returns the total amount of a specific collectible reference in the inventory.
		/// </summary>
		/// <param name="reference">The collectible reference to sum amounts for.</param>
		/// <returns>The total amount of the given collectible reference.</returns>
		public virtual int GetTotalAmountOf(string reference)
		{
			if (inventory == null || inventory.Length == 0)
				return 0;

			return inventory
				.Where((item) => string.Equals(item.reference, reference))
				.Sum((item) => item.amount);
		}

		/// <summary>
		/// Returns a JSON string representation of this Game Data.
		/// </summary>
		public virtual string ToJson() => JsonUtility.ToJson(this);

		/// <summary>
		/// Returns a new instance of Game Data from a given JSON string.
		/// </summary>
		/// <param name="json">The JSON string to parse.</param>
		public static GameData FromJson(string json) => JsonUtility.FromJson<GameData>(json);
	}
}
