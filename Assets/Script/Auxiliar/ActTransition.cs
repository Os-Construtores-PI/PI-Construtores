using UnityEngine;
using UnityEngine.Events;

public class ActTransition : MonoBehaviour
{
    public readonly UnityEvent Transition = new();

    [SerializeField]
    AudioSource backgroundMusic;

    [SerializeField]
    float pitchIncrease = .1f;

    private bool triggeredTransition = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggeredTransition)
            return;
        triggeredTransition = true;
        Transition.Invoke();
        backgroundMusic.pitch += pitchIncrease;
    }
}
