using UnityEngine;

public class MenuAudioManager : MonoBehaviour
{
  public static MenuAudioManager Instance;

  [SerializeField] private somMenu somMenu;

  [Header("Sources")]
  [SerializeField] private AudioSource musicSource;
  [SerializeField] private AudioSource sfxSource;
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
  void Start()
    {
        
    }

  public void TocarMusica()
  {
    musicSource.clip = somMenu.musica;
    musicSource.loop = true;
    musicSource.Play();
  }
}
