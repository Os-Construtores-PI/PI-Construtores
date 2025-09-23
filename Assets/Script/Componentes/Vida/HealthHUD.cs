using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
            _slider.value = 1f;
       // _slider = GetComponent<Slider>();
    }
    private void Start()
    {
        if (transform.parent.TryGetComponent(out PlayerHUD playerHUD))
        {
            IdHealth = playerHUD.ID;
        }
        else
        {
            print("Não está com um pai com PlayerHUD");
        }

        switch (HUDType)
        {
            case HealthHUDType.PLAYER:
                InitializePlayerHUD();
                break;

            case HealthHUDType.ENEMY:
                InitializeEnemyHUD();
                break;
        }
    }
    public void BindToPlayer(Player player)
    {
        if (player == null) return;
        IdHealth = player.ID;
        // Atualiza o slider imediatamente
       // _slider.value = player.Health / player.MaxHealth;
        UptadeHealthImagens(player.Health, player.MaxHealth);

        // Registrar para updates futuros
        player._OnHealthChanged.AddListener((value) =>
        {
            UptadeHealthImagens(value * player.MaxHealth, player.MaxHealth);
        });

    }

    private void InitializePlayerHUD()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.TryGetComponent(out Player playerref) && playerref.ID == IdHealth)
            {
                // _slider.DOValue(playerref.Health / playerref.MaxHealth,.5f);
                float value = playerref.Health / playerref.MaxHealth;
                _slider.DOValue(value, .5f);
               // UptadeHealthImagens(value);
                break;

            }
        }
    }   

    private void InitializeEnemyHUD()
    {
        if (EnemyTarget == null)
        {
            Debug.LogWarning($"{gameObject.name}: EnemyTarget não definido.");
            return;
        }

        GameObject enemyObject = EnemyTarget.parent ? EnemyTarget.parent.gameObject : null;
        if (enemyObject == null)
        {
            Debug.LogWarning($"{gameObject.name}: EnemyTarget não tem pai.");
            return;
        }

        if (enemyObject.TryGetComponent(out CombatEntities combat) && combat.ID == IdHealth)
        {
            _slider.value = combat.Health / combat.MaxHealth;
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
    public void UpdateSlider(float value)
    {
        if (_slider == null) return;
        _slider.DOValue(value, 0.5f);
    }

    private void UptadeHealthImagens(float currentHealth, float maxHealth)
    {
        int vidaInt = Mathf.RoundToInt(currentHealth);

        if (vidaInt == _lastHealth) return;
        _lastHealth = vidaInt;

        for (int i = 0; i < _healthImages.Count; i++)
        {
            if (i < vidaInt)
            {
                // vida ativa
                _healthImages[i].DOFade(1f, 0.3f);
            }
            else
            {
                // vida perdida
                _healthImages[i].DOFade(0f, 0.3f);
            }
        }
    }
}
