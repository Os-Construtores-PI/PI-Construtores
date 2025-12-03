using UnityEngine;

public class ActTransition : MonoBehaviour
{
    [SerializeField] AudioSource backgroundMusic;
    [SerializeField] float pitchIncrease = .1f;

    private bool triggeredTransition = false;
    private void OnTriggerEnter(Collider other)
    {
        if(triggeredTransition) return;
        triggeredTransition = true;
        backgroundMusic.pitch += pitchIncrease;

    }
}
