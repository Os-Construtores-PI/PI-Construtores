using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Ambiente")]
public class SomAmbiente : ScriptableObject
{
  public AudioClip _ambiente;
  public AudioClip _ambiente2;
}

public class GameAudioController : MonoBehaviour
{
  [SerializeField]
  private AudioClip _gamePlayMusic;

  [SerializeField]
  private SomAmbiente _somAmbiente;

  private void Start()
  {
    AudioManager.Instance.PlayMusic(_gamePlayMusic, true, 1.5f);
    AudioManager.Instance.PlayAmbient(_somAmbiente._ambiente);
  }
}
