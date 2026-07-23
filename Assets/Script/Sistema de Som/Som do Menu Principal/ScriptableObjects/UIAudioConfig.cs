using UnityEngine;

[CreateAssetMenu(fileName = "UIAudioConfig", menuName = "Audio/UI Audio Config")]
public class UIAudioConfig : ScriptableObject
{
  [Tooltip("Sound played when hovering over a UI element.")]
  public AudioClip Hover;

  [Tooltip("Sound played when clicking a UI element.")]
  public AudioClip Click;

  [Tooltip("Sound played when going back in the menu.")]
  public AudioClip Back;

  [Tooltip("Sound played when pausing the game.")]
  public AudioClip Pause;
}
