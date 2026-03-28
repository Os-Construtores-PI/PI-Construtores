using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HealthHUDComponent : MonoBehaviour
{
    [Header("Configurações HUD")]
    public HealthHUDType HUDType;
    public int IdHealth = 0;

    [Header("Sliders")]
    public Slider _slider; // ===> BARRA DE VIDA REAL
    public Slider _damageSlider; // ===> BARRA DE DANO / FADE

    public Transform EnemyTarget { get; set; }
    private CombatEntities _boundEntity;
    private Player _boundPlayer;

    private Camera _cachedCamera;

    private void Awake()
    {
        DOTween.Init();
    }

    public void BindToPlayer(Player player)
    {
        if (player == null)
            return;

        if (_boundPlayer != null)
            _boundPlayer._OnHealthChanged.RemoveListener(UpdateSlider);

        _boundPlayer = player;
        _boundPlayer._OnHealthChanged.AddListener(UpdateSlider);

        // Inicializa sliders imediatamente
        float percent =
            _boundPlayer.MaxHealth > 0 ? _boundPlayer.Health / _boundPlayer.MaxHealth : 1f;
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

    private void UpdateSlider(float normalizedHealth)
    {
        if (_slider == null)
            return;

        normalizedHealth = Mathf.Clamp01(normalizedHealth);

        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        // Atualiza barra principal (vida real)
        _slider.DOValue(normalizedHealth, 0.35f).SetEase(Ease.OutQuad);

        // Só aplica o efeito de dano se realmente perdeu vida
        if (_damageSlider != null && _damageSlider.value > normalizedHealth)
        {
            _damageSlider.DOKill();

            // Tween da barra de dano (vermelha) indo para o novo valor, mais lento
            _damageSlider.DOValue(normalizedHealth, 0.7f).SetEase(Ease.OutQuad);

            // Fade apenas quando toma dano
            if (_damageSlider.fillRect != null)
            {
                Image fillImage = _damageSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.DOKill();
                    fillImage.color = new Color(
                        fillImage.color.r,
                        fillImage.color.g,
                        fillImage.color.b,
                        1f
                    ); // reseta alpha
                    fillImage.DOFade(0f, 0.5f).SetDelay(0.2f);
                }
            }
        }
        else if (_damageSlider != null)
        {
            // Se curou, apenas sincroniza o valor para não ficar travado
            _damageSlider.value = normalizedHealth;
        }
    }

    private void LateUpdate()
    {
        if (HUDType != HealthHUDType.ENEMY || EnemyTarget == null)
            return;

        transform.position = EnemyTarget.position;
        FaceCamera();
    }

    private void FaceCamera()
    {
        if (_cachedCamera == null)
        {
            _cachedCamera = Camera.main;
            if (_cachedCamera == null)
                _cachedCamera = FindClosestCamera();
        }

        if (_cachedCamera != null)
        {
            Vector3 direction = transform.position - _cachedCamera.transform.position;
            transform.forward = direction.normalized;
        }
    }

    private Camera FindClosestCamera()
    {
        Camera[] cameras = Camera.allCameras;
        Camera closestCam = null;
        float closestDistSqr = float.MaxValue;
        Vector3 myPosition = transform.position;

        foreach (Camera cam in cameras)
        {
            float distSqr = (cam.transform.position - myPosition).sqrMagnitude;
            if (distSqr < closestDistSqr)
            {
                closestDistSqr = distSqr;
                closestCam = cam;
            }
        }

        return closestCam;
    }

    private void OnDestroy()
    {
        if (_boundPlayer != null)
            _boundPlayer._OnHealthChanged.RemoveListener(UpdateSlider);
        if (_boundEntity != null)
            _boundEntity._OnHealthChanged.RemoveListener(UpdateSlider);
    }
}
