using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
  public static AudioManager Instance;

  [Header("Sources")]
  [SerializeField] private AudioSource musicSource;
  [SerializeField] private AudioSource sfxSource;
  [SerializeField] private AudioSource ambientSource;

  private Coroutine _musicFadeRoutine;

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
  public void PlayMusic(AudioClip clip, bool loop = true, float fadeTime = 1f)
  {
    if (clip == null) return;

    if (_musicFadeRoutine != null)
      StopCoroutine(_musicFadeRoutine);

    _musicFadeRoutine = StartCoroutine(FadeMusicRoutine(clip, loop, fadeTime));
  }

  private IEnumerator FadeMusicRoutine(AudioClip newClip, bool loop, float duration)
  {
    // fade out
    float startVolume = musicSource.volume;

    for(float t = 0; t < duration; t += Time.unscaledDeltaTime)
    {
      musicSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
      yield return null;
    }

    musicSource.clip = newClip;
    musicSource.loop = loop;
    musicSource.Play();

    // fade in

    for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
    {
      musicSource.volume = Mathf.Lerp(0, startVolume, t / duration);
      yield return null;
    }

    musicSource.volume = startVolume;
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

  public void StopAmbient()
  {
    ambientSource.Stop();
  }
}
