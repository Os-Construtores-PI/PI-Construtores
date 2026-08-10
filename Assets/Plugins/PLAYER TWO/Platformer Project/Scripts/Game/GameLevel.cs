using System;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[Serializable]
	public class GameLevel
	{
		[Header("General Settings")]
		[Tooltip("The name of the level. This will be displayed in the level selection.")]
		public string name;

		[Tooltip("The description of the level. This will be displayed in the level selection.")]
		public string description;

		[Tooltip("The image of the level. This will be displayed in the level selection.")]
		public Sprite image;

		[Header("Locking Settings")]
		[Tooltip(
			"This level will be inaccessible from the level selection unless manually unlocked from code."
		)]
		public bool locked;

		[Min(0)]
		[Tooltip(
			"If greater than 0, this property overrides the 'locked' flag and makes the level inaccessible if the total stars is not enough."
		)]
		public int requiredStars;

		[Header("Start Level Settings")]
		[Tooltip("The name of the scene to load when the Level is played.")]
		public string scene;

		[Tooltip("The name of the scene to load when the Level is played for the first time.")]
		public string firstTimeScene;

		[Header("Finish Level Settings")]
		[Tooltip("Unlocks the next level from the level list unless it's locked by stars counter.")]
		public bool unlockNextLevel;

		[Tooltip("Scene name to load when the Level is finished.")]
		public string nextScene;

		[Tooltip("Scene name to load when the Level is finished for the first time.")]
		public string firstTimeNextScene;

		[Header("Exit Level Settings")]
		[Tooltip(
			"Scene name to load when the Level is exited. If empty, it will load the global exit scene."
		)]
		public string exitScene;

		/// <summary>
		/// Returns the array of collectibles instances present in this level.
		/// </summary>
		public CollectibleInstanceList collectibles { get; set; } = new();

		/// <summary>
		/// Returns the best score achieved in this level.
		/// </summary>
		public LevelBestScore bestScore { get; set; } = new();

		/// <summary>
		/// Returns the amount of times this level has been beaten.
		/// </summary>
		public int beatenTimes { get; set; }

		/// <summary>
		/// Returns true if this level has been completed at least once.
		/// </summary>
		public bool wasCompletedOnce => beatenTimes > 0;

		/// <summary>
		/// Loads this Game Level state from a given Game Data.
		/// </summary>
		/// <param name="data">The Game Data to read the state from.</param>
		public virtual void LoadState(LevelData data)
		{
			locked = data.locked;
			beatenTimes = data.beatenTimes;
			bestScore = data.bestScore ?? new LevelBestScore();
			collectibles = CollectibleInstance.CreateFromData(data.collectibles);
		}

		/// <summary>
		/// Loads the scene of this Game Level. If the first time scene is set
		/// and the level has not been completed, it will load the first time scene instead.
		/// </summary>
		public virtual void StartLevel()
		{
			if (wasCompletedOnce || string.IsNullOrEmpty(firstTimeScene))
				GameLoader.instance.Load(scene);
			else
				GameLoader.instance.Load(firstTimeScene);
		}

		/// <summary>
		/// Loads the next scene of this Game Level. If the first time scene is set
		/// and the level has not been completed, it will load the first time scene instead.
		/// If the "unlock next level" flag is set, it will unlock the next level from the level list.
		/// </summary>
		public virtual void FinishLevel()
		{
			if (unlockNextLevel)
				Game.instance.UnlockNextLevel();

			if (beatenTimes > 1 || string.IsNullOrEmpty(firstTimeNextScene))
				GameLoader.instance.Load(nextScene);
			else
				GameLoader.instance.Load(firstTimeNextScene);
		}

		/// <summary>
		/// Loads the exit scene of this Game Level. If the exit scene is not set,
		/// it will load the global exit scene from the Game instance.
		/// </summary>
		public virtual void ExitLevel()
		{
			if (string.IsNullOrEmpty(exitScene))
				GameLoader.instance.Load(Game.instance.levelExitScene);
			else
				GameLoader.instance.Load(exitScene);
		}

		/// <summary>
		/// Sets the best score of this level given the amount of coins, stars and time.
		/// </summary>
		/// <param name="coins">The amount of coins collected.</param>
		/// <param name="stars">The amount of stars collected.</param>
		/// <param name="time">The time it took to complete the level.</param>
		public virtual void SetBestScore(int coins, int stars, float time)
		{
			bestScore ??= new LevelBestScore();
			bestScore.coins = Mathf.Max(bestScore.coins, coins);
			bestScore.time = bestScore.time == 0 ? time : Mathf.Min(bestScore.time, time);
			bestScore.stars = Mathf.Max(bestScore.stars, stars);
		}

		/// <summary>
		/// Returns this Level Data of this Game Level to be used by the Data Layer.
		/// </summary>
		public virtual LevelData ToData()
		{
			return new LevelData()
			{
				locked = locked,
				beatenTimes = beatenTimes,
				bestScore = bestScore,
				collectibles = CollectibleSerializer.CreateFromInstances(collectibles.ToArray()),
			};
		}

		/// <summary>
		/// Returns a given time as a formatted string.
		/// </summary>
		/// <param name="time">The time you want to format.</param>
		/// <param name="minutesSeparator">The separator between minutes and seconds.</param>
		/// <param name="secondsSeparator">The separator between seconds and milliseconds.</param>
		public static string FormattedTime(
			float time,
			string minutesSeparator = "'",
			string secondsSeparator = "\""
		)
		{
			var minutes = Mathf.FloorToInt(time / 60f);
			var seconds = Mathf.FloorToInt(time % 60f);
			var milliseconds = Mathf.FloorToInt(time * 100f % 100f);
			return minutes.ToString("0")
				+ minutesSeparator
				+ seconds.ToString("00")
				+ secondsSeparator
				+ milliseconds.ToString("00");
		}
	}
}
