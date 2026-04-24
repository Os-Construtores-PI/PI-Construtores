using UnityEngine;

public class AudioManager : MonoBehaviour
{
  public static AudioManager Instance;

  [Header("Sources")]
  [SerializeField] private AudioSource musicSource;
  [SerializeField] private AudioSource sfxSource;
  [SerializeField] private AudioSource ambientSource;

  private void Awake()
  {
    if(Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject); // mantém entre cenas
  }

  // Musica
  public void PlayMusic(AudioClip clip, bool loop = true)
  {
    if (clip == null) return;

    musicSource.clip = clip;
    musicSource.loop = loop;
    musicSource.Play();
  }

  //SFX
  public void PlaySFX(AudioClip clip)
  {
    if(clip == null) return;
    sfxSource.PlayOneShot(clip);
  }

  public void PlayAmbient(AudioClip clip, bool loop = true)
  {
    if (clip == null) return;
    ambientSource.clip = clip;
    ambientSource.loop = loop;
    ambientSource.Play();
  }
}
