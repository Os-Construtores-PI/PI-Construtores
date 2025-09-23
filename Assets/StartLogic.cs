using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class StartLogic : MonoBehaviour
{
    GameDirector gameDirector;

    private void Start()
    {
        DOTween.Init();
        gameDirector = FindAnyObjectByType<GameDirector>();
    }

    public void OnStartPressed()
    {
        // 1. Ativa o mundo (players, HUD completo, câmeras)
        gameDirector.StartWorld();

        // 2. Faz a animação de sumir
        RectTransform rect = GetComponent<RectTransform>();
        rect.pivot = new Vector2(0, 0);

        // Escala para 0 e destrói o GameObject ao final
        rect.DOScale(Vector3.zero, 0.35f).SetEase(Ease.InOutCubic)
            .OnComplete(() =>
            {
                // Remove todo o painel temporário do HUD
                Destroy(rect.gameObject);
            });
    }
}
