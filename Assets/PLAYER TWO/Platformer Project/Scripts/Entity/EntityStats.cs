using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	public abstract class EntityStats : ScriptableObject { }

	public abstract class EntityStats<T> : EntityStats
		where T : ScriptableObject { }
}
