using UnityEngine;

[CreateAssetMenu(fileName = "BackgroundMusicConfig", menuName = "Audio/Background Music Config")]
public class BackgroundMusicConfig : ScriptableObject
{
  [Tooltip("Main background music for the menu.")]
  public AudioClip BackgroundMusic;

  [Tooltip("Specific track for the Amethyst section/level.")]
  public AudioClip AmethystSong;

  [Tooltip("Specific track for the Portal section/level.")]
  public AudioClip PortalSong;

  [Tooltip("Music played on the Game Over screen.")]
  public AudioClip GameOverMusic;
}
