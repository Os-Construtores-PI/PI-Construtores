using UnityEngine;

public class MenuAudioManager : MonoBehaviour
{
  public static MenuAudioManager Instance;

  [SerializeField] private somMenu somMenu;

  [Header("Sources")]
  [SerializeField] private AudioSource musicSource;
  [SerializeField] private AudioSource sfxSource;
  [SerializeField] private AudioSource ambientSource;
  // Start is called once before the first execution of Update after the MonoBehaviour is created

  private void Awake()
  {
    if(Instance != null && Instance != this)
    {
      Destroy(gameObject);
      return;
    }

    Instance = this;
  }
  private void Start()
  {
    TocarMusica();
    //PlayClick();
  }

  public void TocarMusica()
  {
    musicSource.clip = somMenu.musica;
    musicSource.loop = true;
    musicSource.Play();
  }
  public void PlayHover()
  {
    sfxSource.PlayOneShot(somMenu.hover);
  }

  public void PlayClick()
  {
    sfxSource.PlayOneShot(somMenu.click);
  }

  public void PlayBack()
  {
    sfxSource.PlayOneShot(somMenu.back);
  }
}
