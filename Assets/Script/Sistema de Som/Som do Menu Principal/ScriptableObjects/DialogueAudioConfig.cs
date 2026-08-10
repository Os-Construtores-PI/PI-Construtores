using UnityEngine;

[CreateAssetMenu(fileName = "DialogueAudioConfig", menuName = "Audio/Dialogue Audio Config")]
public class DialogueAudioConfig : ScriptableObject
{
  [Tooltip("Sound played when a dialogue box opens.")]
  public AudioClip DialogueOpen;

  [Tooltip("Sound played when advancing to the next dialogue line.")]
  public AudioClip DialogueNext;

  [Tooltip("Sound played when going back in dialogue.")]
  public AudioClip DialogueBack;

  [Tooltip("Sound played when closing dialogue.")]
  public AudioClip DialogueClose;
}
