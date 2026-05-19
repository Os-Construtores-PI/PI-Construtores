public enum ErrorType
{
  SUCCESS,
  ATTRIBUTE_ERROR,
  COMPONENT_ERROR,
  TYPE_ERROR,
  ENEMYTYPE_ERROR,
  ENTITYTYPE_ERROR,
  ID_ERROR,
}

public enum QualityTier
{
  COMMON,
  UNCOMMON,
  RARE,
  EPIC,
  LEGENDARY,
}

public enum ItemUsageType
{
  Equipable,
  Consumable,
  Passive,
}

public enum AIType
{
  NONE,
  AUTOMATIC,
  MANUAL,
}

public enum HealthHUDType
{
  PLAYER,
  ENEMY,
  ENTITY,
}

public enum ModifyTYPE
{
  POSITIVE,
  NEGATIVE,
}

public enum TimeTYPE
{
  PERMANENT,
  TEMPORARY,
}

public enum ColorCode
{
  YELLOW,
  BLUE,
  RED,
  GREEN,
}

public enum GameMode
{
  SINGLEPLAYER,
  MULTIPLAYER,
}

public enum ActionType
{
  None,
  Idle,
  Move,
  Jump,
  Fall,
  Dash,
  Attack,
  Slide,
  Interact,
  GroundSlam,
  Boost,
  Bounce,
}

public enum InputType
{
  Keyboard,
  JoystickXbox,
  JoystickPlaystation,
}

public enum LevelPathType
{
  tation,
  visible,
  bile,
  ll,
}

public enum BillboardType
{
  LookAtCamera,
  CameraForward,
}

// LookUp

public enum EffectType
{
  DashEffect,
  JumpEffect,
  BoostEffect,
  ChargingEffect,
  SpeedEffect,
}

public enum TrailType
{
  MovementTrail,
}

public enum PlayerAudioType
{
  Jump,
  Dash,
}
