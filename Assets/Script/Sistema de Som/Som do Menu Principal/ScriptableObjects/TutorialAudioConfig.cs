using UnityEngine;

[CreateAssetMenu(fileName = "TutorialAudioConfig", menuName = "Audio/Tutorial Audio Config")]
public class TutorialAudioConfig : ScriptableObject
{
  [Tooltip("Sound played when the tutorial opens.")]
  public AudioClip TutorialOpen;

  [Tooltip("Sound played when going back in the tutorial.")]
  public AudioClip TutorialBack;

  [Tooltip("Sound played when closing the tutorial.")]
  public AudioClip TutorialClose;
}
