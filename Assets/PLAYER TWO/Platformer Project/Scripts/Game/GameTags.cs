using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	public class GameTags
	{
		public static string Player = "Player";
		public static string Enemy = "Enemy";
		public static string Hazard = "Hazard";
		public static string Platform = "Platform";
		public static string Pole = "Pole";
		public static string Panel = "Panel";
		public static string Spring = "Spring";
		public static string Hitbox = "Hitbox";
		public static string SurfaceGrass = "Surface/Grass";
		public static string SurfaceWood = "Surface/Wood";
		public static string SurfaceMetal = "Surface/Metal";
		public static string VolumeWater = "Volume/Water";
		public static string InteractiveRail = "Interactive/Rail";
		public static string GravityField = "Gravity Field";
		public static string Collectible = "Collectible";

		public static bool IsPlayer(Collider collider) => collider.CompareTag(Player);

		public static bool IsEnemy(Collider collider) => collider.CompareTag(Enemy);

		public static bool IsEntity(Collider collider) => IsPlayer(collider) || IsEnemy(collider);

		public static bool IsHazard(Collider collider) => collider.CompareTag(Hazard);

		public static bool IsPlatform(Collider collider) => collider.CompareTag(Platform);

		public static bool IsRail(Collider collider) => collider.CompareTag(InteractiveRail);

		public static bool IsCollectible(Collider collider) => collider.CompareTag(Collectible);
	}
}
