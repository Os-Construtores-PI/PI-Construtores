public static class GameContext
{
  public static bool IsTutorialActive;
  public static bool IsDialogueActive;
  public static bool IsPaused = false;

  public static bool CanPause()
  {
    return !IsTutorialActive && !IsDialogueActive;
  }
}
