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
    

    



    private void Awake()
    {
        DOTween.Init();

        if (_slider == null)
            _slider = GetComponent<Slider>();

        if (_slider != null)
        {
            _slider.minValue = 0f;
            _slider.maxValue = 1f;
            _slider.value = 1f; // vida cheia no inicio
        }
    }


    public void BindToPlayer(Player player)
    {
        if (player == null) return;
        IdHealth = player.ID;
        // Atualiza o slider imediatamente

        // atualiza o slider imediatamente
        float percent = (float)player.Health / player.MaxHealth;
        UpdateDotSlider(percent);

        // Sempre que a vida mudar -> atualiza o slider
        player._OnHealthChanged.AddListener((value) =>
        {
            float hpPercent = (float)value / player.MaxHealth;
            UpdateDotSlider(hpPercent);
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


    
    public void UpdateDotSlider(float healthPercent)
    {
        if (_slider == null) return;
        
        // Tween suave do valor atual para o novo
        _slider.DOValue(healthPercent, 0.35f).SetEase(Ease.OutQuad);

        // Tremor da HUD se tomou dano
        if(healthPercent < _slider.value)
        {
            transform.DOKill();
            transform.DOShakePosition(0.3f, strength: new Vector3(10f, 10f, 0f), vibrato: 15, randomness: 90, snapping: false, fadeOut: true).SetEase(Ease.OutQuad);
        }
    }
    
}
