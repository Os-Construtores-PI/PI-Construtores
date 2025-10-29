using UnityEngine;

public class ActTransition : MonoBehaviour
{
    [SerializeField] AudioSource backgroundMusic;
    [SerializeField] float pitchIncrease = .1f;
    private void OnTriggerEnter(Collider other)
    {
        backgroundMusic.pitch += pitchIncrease;
    }
}
