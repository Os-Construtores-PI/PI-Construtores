using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class HealthHUDComponent : MonoBehaviour
{
    [Header("Configurações HUD")]
    public HealthHUDType HUDType;
    public int IdHealth = 0;

    public Slider _slider;
    public Transform EnemyTarget { get; set; }
    private CombatEntities _boundEntity;


    private Camera _cachedCamera;
    private Player _boundPlayer;
    private float _currentPercent = 1f;
    private float _targetPercent = 1f;
    private float _lerpSpeed = 2f;





    //----------------


    //---------------------



    private void Awake()
    {
        DOTween.Init();

        if (_slider == null)
            _slider = GetComponent<Slider>() ?? GetComponentInChildren<Slider>();

        if (_slider != null)
        {
            _slider.minValue = 0f;
            _slider.maxValue = 1f;
            _slider.value = 1f;
            _currentPercent = 1f;
            _targetPercent = 1f;
        }
    }


    public void BindToPlayer(Player player)
    {
        if (player == null) return;

        if (_boundPlayer != null)
            _boundPlayer._OnHealthChanged.RemoveListener(OnPlayerHealthChanged);

        _boundPlayer = player;
        _boundPlayer._OnHealthChanged.AddListener(OnPlayerHealthChanged);

        // inicializa o slider no valor correto
        float percent = player.MaxHealth > 0 ? player.Health / player.MaxHealth : 1f;
        _slider.value = percent;
        _currentPercent = percent;
        _targetPercent = percent;


    }



    private void OnPlayerHealthChanged(float value)
    {
        if (_boundPlayer == null || _boundPlayer.MaxHealth <= 0f) return;

        float newPercent = value / _boundPlayer.MaxHealth;

        // somente diminui proporcionalmente
        if (newPercent < _targetPercent)
        {
            _targetPercent = newPercent;
        }
    }






    private void LateUpdate()
    {
        if (HUDType != HealthHUDType.ENEMY || EnemyTarget == null)
            return;

        transform.position = EnemyTarget.position;
        FaceCamera();
    }

    /// <summary>
    /// Faz o HUD virar para a câmera principal ou para a câmera mais próxima
    /// </summary>
    private void FaceCamera()
    {
        if (_cachedCamera == null)
        {
            _cachedCamera = Camera.main;
            if (_cachedCamera == null)
            {
                // Fallback: busca a câmera mais próxima
                _cachedCamera = FindClosestCamera();
            }
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
    /// <summary>
    /// Atualiza suavemente o valor do slider da barra de vida
    /// </summary>

    private void OnDestroy()
    {
        if (_boundPlayer != null)
            _boundPlayer._OnHealthChanged.RemoveListener(OnPlayerHealthChanged);
    }

    private void Update()
    {
        if (_slider == null) return;

        if (_currentPercent != _targetPercent)
        {
            // Lerp manual para reduzir suavemente
            _currentPercent = Mathf.MoveTowards(_currentPercent, _targetPercent, Time.deltaTime * _lerpSpeed);
            _slider.value = _currentPercent;
        }
    }

    public void BindToEntity(CombatEntities entity)
    {
        if (entity == null) return;

        if (_boundEntity != null)
            _boundEntity._OnHealthChanged.RemoveListener(UpdateSlider);

        _boundEntity = entity;

        // Atualiza o slider imediatamente para a vida atual
        float percent = (_boundEntity.MaxHealth > 0f) ? _boundEntity.Health / _boundEntity.MaxHealth : 1f;
        _slider.value = percent;

        // Adiciona listener para receber updates proporcionais ao dano
        _boundEntity._OnHealthChanged.AddListener(UpdateSlider);
    }

    private void UpdateSlider(float normalizedHealth)
    {
        if (_slider == null) return;

        normalizedHealth = Mathf.Clamp01(normalizedHealth);

        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        // Tween suave
        _slider.DOValue(normalizedHealth, 0.35f).SetEase(Ease.OutQuad);
    }


    


}










    

     







