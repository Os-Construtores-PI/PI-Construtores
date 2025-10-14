using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using DG.Tweening;

public class ShiftDashScript : MonoBehaviour
{
    [Header("Referencia da Imagem")]
    public Image shiftImage;
    public Image coolDownFillImage;
    private CanvasGroup _canvasGroup;

    [Header("Configurações do Fade")]
    public float fadeOutDuration = 0.2f;
    public float fadeInDuration = 0.3f;


    // Oculta o ícone quando o Dash é usado

    private void Awake()
    {
        DOTween.Init(false, true); // inicializa DOTween (silencioso)
        _canvasGroup = GetComponent<CanvasGroup>();

        if(shiftImage == null )
            Debug.LogWarning("[DashHUDIcon] shiftImage não atribuído e não encontrado automaticamente.");

        _canvasGroup.alpha = 1f;
    }
    public void OnDashUSed()
    {
        Debug.Log("[DashHUDIcon] HideDashIcon chamado. shiftImage == " + (shiftImage == null ? "NULL" : "OK"));
        _canvasGroup.DOKill();
        _canvasGroup.DOFade(1f, fadeOutDuration).SetEase(Ease.OutQuad);

        
    }


    // mostra novamente quando o Dash está liberado

    public void OnDashReady()
    {
        Debug.Log("[DashHUDIcon] ShowDashIcon chamado.");
        _canvasGroup.DOKill();
        _canvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);
    }

    
}
