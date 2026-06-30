using UnityEngine;

public class MenuMusicController : MonoBehaviour
{
  [SerializeField]
  private AudioClip _menuMusic;

  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
    AudioManager.Instance.PlayMusic(_menuMusic, true, 1.5f);
    AudioManager.Instance.StopAmbient();
  }
}
