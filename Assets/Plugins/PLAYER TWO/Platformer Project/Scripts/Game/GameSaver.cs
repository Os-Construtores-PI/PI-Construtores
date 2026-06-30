using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Game/Game Saver")]
	public class GameSaver : Singleton<GameSaver>
	{
		public enum Mode
		{
			Binary,
			JSON,
			PlayerPrefs,
		}

		public Mode mode = Mode.Binary;
		public string fileName = "save";
		public string binaryFileExtension = "data";

		/// <summary>
		/// The amount of available slots to save data to.
		/// </summary>
		protected static readonly int TotalSlots = 5;

		/// <summary>
		/// Persists a given Game Data on a disk slot.
		/// </summary>
		/// <param name="data">The Game Data you want to persist.</param>
		/// <param name="index">The index of the slot.</param>
		public virtual void Save(GameData data, int index)
		{
			switch (mode)
			{
				default:
				case Mode.Binary:
					SaveBinary(data, index);
					break;
				case Mode.JSON:
					SaveJSON(data, index);
					break;
				case Mode.PlayerPrefs:
					SavePlayerPrefs(data, index);
					break;
			}
		}

		/// <summary>
		/// Returns the Game Data or null by reading a given slot.
		/// </summary>
		/// <param name="index">The index of the slot you want to read.</param>
		public virtual GameData Load(int index)
		{
			return mode switch
			{
				Mode.JSON => LoadJSON(index),
				Mode.PlayerPrefs => LoadPlayerPrefs(index),
				_ => LoadBinary(index),
			};
		}

		/// <summary>
		/// Erases the data from a slot.
		/// </summary>
		/// <param name="index">The index of the slot you want to erase.</param>
		public virtual void Delete(int index)
		{
			switch (mode)
			{
				default:
				case Mode.Binary:
				case Mode.JSON:
					DeleteFile(index);
					break;
				case Mode.PlayerPrefs:
					DeletePlayerPrefs(index);
					break;
			}
		}

		/// <summary>
		/// Returns an array of Game Data from all the slots.
		/// </summary>
		/// <returns></returns>
		public virtual GameData[] LoadList()
		{
			var list = new GameData[TotalSlots];

			for (int i = 0; i < TotalSlots; i++)
			{
				if (TryGetData(out var data, i))
					list[i] = data;
			}

			return list;
		}

		/// <summary>
		/// Returns true if any slot has data.
		/// </summary>
		public virtual bool HasAnyData()
		{
			for (int i = 0; i < TotalSlots; i++)
			{
				if (TryGetData(out _, i))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Tries to get the Game Data from a given slot.
		/// </summary>
		/// <param name="data">The Game Data from the slot.</param>
		/// <param name="index">The index of the slot.</param>
		/// <returns>True if the slot has data.</returns>
		public virtual bool TryGetData(out GameData data, int index)
		{
			data = Load(index);
			return data != null;
		}

		protected virtual void SaveBinary(GameData data, int index)
		{
			var path = GetFilePath(index);
			var formatter = new BinaryFormatter();
			var stream = new FileStream(path, FileMode.Create);
			formatter.Serialize(stream, data);
			stream.Close();
		}

		protected virtual GameData LoadBinary(int index)
		{
			var path = GetFilePath(index);

			if (File.Exists(path))
			{
				var formatter = new BinaryFormatter();
				var stream = new FileStream(path, FileMode.Open);
				var data = formatter.Deserialize(stream);
				stream.Close();
				return data as GameData;
			}

			return null;
		}

		protected virtual void SaveJSON(GameData data, int index)
		{
			var json = data.ToJson();
			var path = GetFilePath(index);
			File.WriteAllText(path, json);
		}

		protected virtual GameData LoadJSON(int index)
		{
			var path = GetFilePath(index);

			if (File.Exists(path))
			{
				var json = File.ReadAllText(path);
				return GameData.FromJson(json);
			}

			return null;
		}

		protected virtual void DeleteFile(int index)
		{
			var path = GetFilePath(index);

			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}

		protected virtual void SavePlayerPrefs(GameData data, int index)
		{
			var json = data.ToJson();
			var key = index.ToString();
			PlayerPrefs.SetString(key, json);
		}

		protected virtual GameData LoadPlayerPrefs(int index)
		{
			var key = index.ToString();

			if (PlayerPrefs.HasKey(key))
			{
				var json = PlayerPrefs.GetString(key);
				return GameData.FromJson(json);
			}

			return null;
		}

		protected virtual void DeletePlayerPrefs(int index)
		{
			var key = index.ToString();

			if (PlayerPrefs.HasKey(key))
			{
				PlayerPrefs.DeleteKey(key);
			}
		}

		protected virtual string GetFilePath(int index)
		{
			var extension = mode == Mode.JSON ? "json" : binaryFileExtension;
			return Application.persistentDataPath + $"/{fileName}_{index}.{extension}";
		}
	}
}
