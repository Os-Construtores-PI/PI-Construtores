using DG.Tweening;
using UnityEngine;

public class StormEffect : MonoBehaviour
{
    private Light[] lightcomponents;

    [SerializeField] private float cooldownStorm = 5f;
    private float cooldownTimer = 0f;

    private void Start()
    {
        DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
        lightcomponents = GetComponentsInChildren<Light>();
    }

    private void Update()
    {
        StormTimer();
    }
    private void StormTimer()
    {        
        cooldownTimer += Time.deltaTime;

        if (cooldownTimer >= cooldownStorm)
        {
            cooldownTimer = 0f;
            TriggerStorm();
        }
    }
    private void TriggerStorm()
    {
        foreach (Light light in lightcomponents)
        {
            // duração total aleatória
            float totalDuration = Random.Range(0.5f, 1.5f);
            float upDuration = totalDuration * 0.2f;   // sobe rápido
            float downDuration = totalDuration * 0.8f; // desce devagar

            // intensidade aleatória (deixa natural)
            float peakIntensity = Random.Range(700f, 1200f);

            // delay aleatório pequeno pra cada luz
            float randomDelay = Random.Range(0f, 0.3f);

            Sequence seq = DOTween.Sequence();

            seq.AppendInterval(randomDelay);
            seq.Append(light.DOIntensity(peakIntensity, upDuration)
                .SetEase(Ease.OutQuad));
            seq.Append(light.DOIntensity(0, downDuration)
                .SetEase(Ease.InOutSine));

            seq.Play();
        }
    }
}
