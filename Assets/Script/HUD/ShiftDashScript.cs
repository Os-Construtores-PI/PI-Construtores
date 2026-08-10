using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShiftDashScript : MonoBehaviour
{
  [Header("Referencia da Imagem")]
  public Image shiftImage;
  public Image coolDownFillImage;

  [SerializeField]
  private CanvasGroup _canvasDashGroup;

  [Header("Configura��es do Fade")]
  public float fadeOutDuration = 0.2f;
  public float fadeInDuration = 0.3f;

  private Coroutine fadeCoroutine;

  private void Awake()
  {
    if (_canvasDashGroup == null)
      _canvasDashGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

    if (shiftImage == null)
      shiftImage = GetComponent<Image>();
  }

  public void OnDashUsed()
  {
    if (fadeCoroutine != null)
      StopCoroutine(fadeCoroutine);
    fadeCoroutine = StartCoroutine(FadeCanvas(0f, fadeOutDuration));
  }

  public void OnDashReady()
  {
    if (fadeCoroutine != null)
      StopCoroutine(fadeCoroutine);
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
