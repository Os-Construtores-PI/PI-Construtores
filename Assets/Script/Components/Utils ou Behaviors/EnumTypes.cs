using System;

    public enum EntityType
    {
        PLAYER, ENEMY, ENTITY
    }
    public enum Sub_EnemyType
    {
        NONE, SIMPLE, RANGED, FLYING, TANK
    }
    public enum ErrorType
    {
        SUCCESS, ATTRIBUTE_ERROR, COMPONENT_ERROR
    }
    public enum QualityTier
    {
        COMMON,UNCOMMON, RARE, EPIC, LEGENDARY
    }
    public enum StatType
    {
        ARMOR, ATTACK, SPEED, JUMP
    }
    public enum ItemUsageType
    {
        Equipable,
        Consumable,
        Passive
    }
    [Serializable]
    public struct Entities
    {
    public EntityType TipoEntidade;
    public Sub_EnemyType TipoInimigo;
    }
