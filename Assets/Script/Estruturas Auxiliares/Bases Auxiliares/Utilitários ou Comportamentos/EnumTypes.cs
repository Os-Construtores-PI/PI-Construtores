using System;

public enum ErrorType
{
    SUCCESS, ATTRIBUTE_ERROR, COMPONENT_ERROR, TYPE_ERROR, ENEMYTYPE_ERROR,ENTITYTYPE_ERROR,ID_ERROR
}
public enum QualityTier
{
    COMMON, UNCOMMON, RARE, EPIC, LEGENDARY
}

public enum ItemUsageType
{
    Equipable,
    Consumable,
    Passive
}

public enum AIType
{
    NONE,AUTOMATIC,MANUAL
}

public enum HealthHUDType
{
    PLAYER, ENEMY, ENTITY
}

public enum ModifyTYPE
{
    POSITIVE,
    NEGATIVE
}

public enum TimeTYPE
{
    PERMANENT,
    TEMPORARY
}

public enum ColorCode
{
    YELLOW,
    BLUE,
    RED,
    GREEN
}

public enum GameMode
{
    SINGLEPLAYER,
    MULTIPLAYER
}

