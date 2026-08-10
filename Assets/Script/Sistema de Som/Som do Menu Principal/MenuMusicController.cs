using UnityEngine;

public class MenuMusicController : MonoBehaviour
{
  [SerializeField]
  private AudioClip _menuMusic;

  [SerializeField] private float _defaultVolume = 0.078f;

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    AudioManager.Instance.PlayMusic(_menuMusic, true, 1.5f);
    AudioManager.Instance.StopAmbient();

    SetMusicVolume(_defaultVolume);
  }

  public void SetMusicVolume(float volume)
  {
    volume = Mathf.Clamp01(volume);

    AudioManager.Instance.SetMusicVolume(volume);
  }
}
