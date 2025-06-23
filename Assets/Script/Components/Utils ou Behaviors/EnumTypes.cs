using System;

    public enum EntityType
    {
        PLAYER, ENEMY, ENTITY
    }
    public enum EnemyType
    {
        NONE, SIMPLE, RANGED, FLYING, TANK
    }
    public enum ErrorType
    {
        SUCCESS, ATTRIBUTE_ERROR, COMPONENT_ERROR, TYPE_ERROR, ENEMYTYPE_ERROR,ENTITYTYPE_ERROR,ID_ERROR
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
public struct Entidade
{
    public int ID;
    public EntityType TipoEntidade;
    public EnemyType TipoInimigo;
}
