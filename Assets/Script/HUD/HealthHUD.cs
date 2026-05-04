using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HealthHUD : BarHUD
{
  [SerializeField]
  private Slider _damageSlider; // ===> BARRA DE DANO / FADE

  private void Awake()
  {
    DOTween.Init();
  }

  public override void BindToPlayer(Player player)
  {
    if (player == null)
      return;

    if (_boundPlayer != null)
      _boundPlayer._OnHealthChanged.RemoveListener(UpdateSlider);

    _boundPlayer = player;
    _boundPlayer._OnHealthChanged.AddListener(UpdateSlider);

    // Inicializa sliders imediatamente
    float percent = _boundPlayer.MaxHealth > 0 ? _boundPlayer.Health / _boundPlayer.MaxHealth : 1f;
    _slider.value = percent;
    if (_damageSlider != null)
      _damageSlider.value = percent;

    if (_slider != null)
    {
      RectTransform sliderRect = _slider.GetComponent<RectTransform>();
      if (sliderRect != null)
      {
        // Salva o tamanho original
        float originalWidth = sliderRect.sizeDelta.x;

        // Começa com largura zero
        sliderRect.sizeDelta = new Vector2(0f, sliderRect.sizeDelta.y);

        sliderRect
          .DOSizeDelta(new Vector2(originalWidth, sliderRect.sizeDelta.y), 1f)
          .SetEase(Ease.OutQuart);
      }
    }
  }

  protected override void UpdateSlider(float normalizedValue)
  {
    if (_slider == null)
      return;

    normalizedValue = Mathf.Clamp01(normalizedValue);

    if (!gameObject.activeInHierarchy)
      gameObject.SetActive(true);

    // Atualiza barra principal (vida real)
    _slider.DOValue(normalizedValue, 0.35f).SetEase(Ease.OutQuad);

    // Só aplica o efeito de dano se realmente perdeu vida
    if (_damageSlider != null && _damageSlider.value > normalizedValue)
    {
      _damageSlider.DOKill();

      // Tween da barra de dano (vermelha) indo para o novo valor, mais lento
      _damageSlider.DOValue(normalizedValue, 0.7f).SetEase(Ease.OutQuad);

      // Fade apenas quando toma dano
      if (_damageSlider.fillRect != null)
      {
        if (_damageSlider.fillRect.TryGetComponent<Image>(out var fillImage))
        {
          fillImage.DOKill();
          fillImage.color = new Color(fillImage.color.r, fillImage.color.g, fillImage.color.b, 1f); // reseta alpha
          fillImage.DOFade(0f, 0.5f).SetDelay(0.2f);
        }
      }
    }
    else if (_damageSlider != null)
    {
      // Se curou, apenas sincroniza o valor para não ficar travado
      _damageSlider.value = normalizedValue;
    }
  }

  private void OnDestroy()
  {
    if (_boundPlayer != null)
      _boundPlayer._OnHealthChanged.RemoveListener(UpdateSlider);
  }
}
