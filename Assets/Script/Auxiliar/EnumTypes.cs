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
  Stopwatch,
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
  Player,
  Enemy,
  Entity,
}

public enum ModifyType
{
  Delta,
  Multiplier,
}

public enum TimeType
{
  Permanent,
  Temporary,
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
  Singleplayer,
  Multiplayer,
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

public enum DialogueLayoutType
{
  Pandora,
  Enemy,
}

public enum WolfActionType
{
  Patrol,
  Chase,
  Attack,
}

public enum DeviceType
{
  Keyboard,
  Xbox,
  Playstation,
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

public enum EntityEffectType
{
  PlayerDashEffect,
  PlayerJumpEffect,
  PlayerBoostEffect,
  PlayerSpeedEffect,
  EntityDeathEffect,
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

public enum RankType
{
  S,
  A,
  B,
  C,
  D,
}

public enum MenuPanelTypes
{
  None,
  Menu,
  OptionsMenu,
  AudioMenu,
  SaveMenu,
  LeaderboardMenu,
}
