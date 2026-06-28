using System;

namespace PLAYERTWO.PlatformerProject
{
	[Serializable]
	public class LevelData
	{
		/// <summary>
		/// Whether the level is locked or not.
		/// </summary>
		public bool locked;

		/// <summary>
		/// The number of times the level has been beaten.
		/// </summary>
		public int beatenTimes;

		/// <summary>
		/// The best score achieved in the level.
		/// </summary>
		public LevelBestScore bestScore = new();

		/// <summary>
		/// The collectibles data.
		/// </summary>
		public CollectibleSerializer[] collectibles;
	}
}
