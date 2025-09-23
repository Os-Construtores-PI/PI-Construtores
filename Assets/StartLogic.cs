using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class StartLogic : MonoBehaviour
{
    private GameDirector gameDirector;

    private void Start()
    {
        DOTween.Init();
        gameDirector = FindAnyObjectByType<GameDirector>();
    }

    public void OnStartPressed()
    {
        // 1. Ativa o mundo
        gameDirector.StartWorld();

        // 2. Remove a câmera inicial (tag MainCamera)
        Camera startCam = Camera.main;
        if (startCam != null)
            Destroy(startCam.gameObject);

        // 3. Faz a animação só no painel de Start
        if (transform.childCount > 0)
        {
            if (transform.GetChild(0).TryGetComponent<RectTransform>(out var startPanel))
            {
                startPanel.DOScale(Vector3.zero, 0.35f)
                    .SetEase(Ease.InOutCubic)
                    .OnComplete(() =>
                    {
                        // Mata tweens do painel
                        DOTween.Kill(startPanel.gameObject);

                        // Destroi o Canvas inteiro (onde está esse script)
                        Destroy(gameObject);
                    });
            }
            else
            {
                // fallback: se não achar painel, destrói direto
                Destroy(gameObject);
            }
        }
        else
        {
            // se não tiver filhos, destrói direto
            Destroy(gameObject);
        }
    }
}
