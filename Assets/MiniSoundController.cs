using UnityEngine;

public class MiniSoundController : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] float maxPitch = 2.0f; // Máximo de aceleração (ex: 2x mais rápido)
    [SerializeField] float pitchIncreaseRate = 0.1f; // Quanto aumenta por segundo/ponto
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public void IncreasePitch()
    {
        audioSource.pitch = Mathf.Min(audioSource.pitch + pitchIncreaseRate,maxPitch);
    }
}
