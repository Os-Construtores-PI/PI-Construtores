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

  // Oculta o �cone quando o Dash � usado

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

  private void OnEnable()
  {
    if (DeviceSpriteManager.Instance != null)
    {
      DeviceSpriteManager.Instance.OnDeviceChanged += OnDeviceChanged;
      AtualizarSprite();
    }
  }

  private void OnDisable()
  {
    if (DeviceSpriteManager.Instance != null)
      DeviceSpriteManager.Instance.OnDeviceChanged -= OnDeviceChanged;
  }

  private void OnDeviceChanged(string device)
  {
    AtualizarSprite();
  }

  public void AtualizarSprite()
  {
    if (shiftImage == null || DeviceSpriteManager.Instance == null)
      return;
    shiftImage.sprite = DeviceSpriteManager.Instance.GetSprite(
      DeviceSpriteManager.InputIconType.Dash
    );

    //shiftImage.color = Color.red;

    Debug.Log($"[DASH] Device: {DeviceSpriteManager.Instance.GetCurrentDevice()}");
  }

  // mostra novamente quando o Dash est� liberado

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
