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

    private float _lastPercent = -1; // guarda o último valor da vida
    

    //----------------


    //---------------------



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
            _lastPercent = 1f;
        }
    }


    public void BindToPlayer(Player player)
    {
        if (player == null) return;
        IdHealth = player.ID;
        

        if (_slider == null)
            _slider = GetComponent<Slider>();
       

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
        
        if(Mathf.Approximately(healthPercent, _lastPercent)) return;

        bool tomouDano = healthPercent < _lastPercent;

        _lastPercent = healthPercent;

        // anima realmente quando muda 
        _slider.DOValue(healthPercent, 0.35f).SetEase(Ease.OutQuad);

        // Tremor da HUD se tomou dano
        if(tomouDano)
        {
            transform.DOKill();
            transform.DOShakePosition(0.4f, new Vector3(8f, 8f, 0f), 12, 90, false, true)
                .SetEase(Ease.OutQuad);
        }

 
    }

    
        

        




    public void ForceSetSlider(float healthPercent)
    {
        if (_slider == null) return;

        _slider.value = healthPercent; // sem animação
        _lastPercent = healthPercent; // atualiza cache
    }
    
}
