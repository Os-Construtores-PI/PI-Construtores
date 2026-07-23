using UnityEngine;

public class MenuAudioManager : MonoBehaviour
{
  public static MenuAudioManager Instance;

  [SerializeField]
  private BackgroundMusicConfig _backgroundMusicConfig;

  [SerializeField]
  private UIAudioConfig _uiAudioConfig;

  [Header("Sources")]
  [SerializeField]
  private AudioSource musicSource;

  [SerializeField]
  private AudioSource sfxSource;

  [SerializeField]
  private AudioSource ambientSource;

  public void Awake()
  {
    if (Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
  }

  public void Start()
  {
    PlayMusic();
  }

  public void PlayMusic()
  {
    musicSource.clip = _backgroundMusicConfig.BackgroundMusic;
    musicSource.loop = true;
    musicSource.Play();
  }

  public void PlayHover()
  {
    sfxSource.PlayOneShot(_uiAudioConfig.Hover);
  }

  public void PlayClick()
  {
    sfxSource.PlayOneShot(_uiAudioConfig.Click);
  }

  public void PlayBack()
  {
    sfxSource.PlayOneShot(_uiAudioConfig.Back);
  }
}
