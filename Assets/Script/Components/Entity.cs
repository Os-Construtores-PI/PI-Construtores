using UnityEngine;

public abstract class EntityBehavior : MonoBehaviour
{
    public enum EntityType
    {
        player, enemy, entity
    }
    public enum StatType
    {
        armor, attack, speed, jump
    }
}
