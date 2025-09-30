using DG.Tweening;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class HealthHUDComponent : MonoBehaviour
{
    [Header("Configurações HUD")]
    public HealthHUDType HUDType;

    public Transform EnemyTarget { get; set; }

    public int IdHealth = 0;

    public Slider _slider;
    private Camera _cachedCamera;
    private int _lastHealth = -1; // não atualiza atoa

    [Header("Imagens da Vida(Opcional)")]
    public List<Image> _healthImages; // arraste as imagens aqui



    private void Awake()
    {
        DOTween.Init();

        if (_slider != null)
        {
            _slider.value = 1f;
        }
        _slider = GetComponent<Slider>();
    }


    public void BindToPlayer(Player player)
    {
        if (player == null) return;
        IdHealth = player.ID;
        // Atualiza o slider imediatamente


        float percent = (float)player.Health / player.MaxHealth;
        UpdateHealthImagens(percent);

        player._OnHealthChanged.AddListener((value) =>
        {
            float hpPercent = (float)value / player.MaxHealth;
            UpdateHealthImagens(value);
        });



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


    public void UpdateHealthImagens(float healthPercent)
    {
        if (_healthImages == null || _healthImages.Count == 0) return;

        int vidaInt = Mathf.FloorToInt(healthPercent * _healthImages.Count);

        if (vidaInt == _lastHealth) return;


        bool tomouDano = vidaInt < _lastHealth;
        _lastHealth = vidaInt;

        for (int i = 0; i < _healthImages.Count; i++)
        {
            if (i < vidaInt)
            {
                _healthImages[i].DOFade(1f, 0.3f);
            }
            else
            {
                _healthImages[i].DOFade(0f, 0.3f);
            }
        }

        if (tomouDano)
        {
            transform.DOKill();

            transform.DOShakePosition(0.3f, strength: new Vector3(10f, 10f, 0f), vibrato: 15, randomness: 90, snapping: false, fadeOut: true)
                .SetEase(Ease.OutQuad);
        }
    }
    public void UpdateDotSlider(float healthPercent)
    {
        _slider.DOValue(healthPercent,.35f);
    }
    
}
