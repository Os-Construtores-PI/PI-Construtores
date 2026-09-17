using UnityEngine;

public static class CameraSettings
{
  private const string INVERT_Y_KEY = "CameraInvert Y";
  
  public static bool InvertY
  {
    get => PlayerPrefs.GetInt(INVERT_Y_KEY, 0) == 1;
  }
  
  public static void SetInvertY(bool value)
  {
    PlayerPrefs.SetInt(INVERT_Y_KEY, value ? 1 : 0);
    PlayerPrefs.Save();
  }
  
  public static void ToggleInvertY()
  {
    SetInvertY(!InvertY);
  }
}
