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
  NONE,
  COMMON,
  UNCOMMON,
  RARE,
  EPIC,
  LEGENDARY,
}

public enum StatType
{
  Speed,
  RunSpeedMultiplier,
  RunAccelMultiplier,
  Regen,
  CanDash,
  RegenInterval,
  DamagedCooldown,
  CombatCooldown,
  JumpForce,
  Health,
  Gravity,
  MaxHealth,
}

public enum ItemUsageType
{
  Equipable,
  Consumable,
  Passive,
}

public enum HudPanelType
{
  GameOver,
  Pause,
  Dialogue,
  HealthBar,
  BoostBar,
  DashIcon,
  Combo,
  ComboPopup,
  Score,
  MaxComboPopup,
  EndGame,
  AmethystCounter,
  InteractionPopup,
  InteractionLetter,
  LockOnOverlay,
  Cutscene,
  TeleportFadePanel,
}

public enum ComboPopupType
{
  None,
  Good,
  Great,
  Awesome,
  Radical,
}

public enum HealthHUDType
{
  PLAYER,
  ENEMY,
  ENTITY,
}

public enum ModifyTYPE
{
  CUSTOM,
  POSITIVE,
  NEGATIVE,
}

public enum TimeTYPE
{
  PERMANENT,
  TEMPORARY,
}

public enum ImpactPopupType
{
  Slam,
  Splash,
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

public enum PlayerActionType
{
  Locked,
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
  RailSlide,
}

public enum WolfActionType
{
  Patrol,
  Chase,
  Attack,
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
  MovementSupport1Trail,
  MovementSupport2Trail,
}

public enum PlayerAudioType
{
  Jump,
  Dash,
}
