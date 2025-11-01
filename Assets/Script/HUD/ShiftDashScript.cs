using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class ShiftDashScript : MonoBehaviour
{
    [Header("Referencia da Imagem")]
    public Image shiftImage;
    public Image coolDownFillImage;
    [SerializeField] private CanvasGroup _canvasDashGroup;

    [Header("Configura��es do Fade")]
    public float fadeOutDuration = 0.2f;
    public float fadeInDuration = 0.3f;

    private Coroutine fadeCoroutine;

    // Oculta o �cone quando o Dash � usado

    private void Awake()
    {
        if (_canvasDashGroup == null)
        {
            _canvasDashGroup = GetComponent<CanvasGroup>();
            if (_canvasDashGroup == null)
            {
                _canvasDashGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        if(shiftImage == null)
        {
            Debug.LogWarning("[ShiftDashScript] Nenhuma imagem de dash atribuida");
        }
    }
    public void OnDashUsed()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCanvas(0f, fadeOutDuration));


    }


    // mostra novamente quando o Dash est� liberado

    public void OnDashReady()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCanvas(1f, fadeInDuration));
    }


    private IEnumerator FadeCanvas(float targetAlpha, float duration)
    {
        float startAlpha = _canvasDashGroup.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            _canvasDashGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }

        _canvasDashGroup.alpha = targetAlpha;
    }

}
