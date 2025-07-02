using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HealthHUDComponent : ComponentBehaviour
{
    [Header("Configurações HUD")]
    [SerializeField] private GameObject healthBarObject;
    [SerializeField] private IconData iconData; // Não usado aqui, mas mantido para expansão futura
    [SerializeField] public HealthHUDType HUDType;

    public Transform EnemyTarget { get; set; }
    public int IdHealth { get; set; }

    private Slider slidercomp;
    private Camera _cachedCamera;

    private void Start()
    {
        slidercomp = GetComponent<Slider>();
        if (slidercomp == null)
        {
            Debug.LogWarning($"Slider não encontrado em {gameObject.name}");
            return;
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

    private void InitializePlayerHUD()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var player in players)
        {
            if (TryGetBrainAndHealth(player, EntityType.PLAYER, out var brain, out var health) && brain.identity.ID == IdHealth)
            {
                if (health.TryGetAttribute("MAX_health", out float maxHealth) && health.TryGetAttribute("health", out float currentHealth))
                {
                    slidercomp.value = currentHealth / maxHealth;
                }
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

        if (TryGetBrainAndHealth(enemyObject, EntityType.ENEMY, out var brain, out var health) && brain.identity.ID == IdHealth)
        {
            if (health.TryGetAttribute("MAX_health", out float maxHealth) && health.TryGetAttribute("health", out float currentHealth))
            {
                slidercomp.value = currentHealth / maxHealth;
            }
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

    private bool TryGetBrainAndHealth(GameObject obj, EntityType expectedType, out BrainComponent brain, out HealthComponent health)
    {
        brain = null;
        health = null;

        if (!obj.TryGetComponent(out brain) || !obj.TryGetComponent(out health))
            return false;

        if (brain.identity.ID != IdHealth || brain.identity.TipoEntidade != expectedType)
        {
            brain = null;
            health = null;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Atualiza suavemente o valor do slider da barra de vida
    /// </summary>
    public void UpdateSlider(float value)
    {
        if (slidercomp == null) return;
        slidercomp.DOValue(value, 0.3f);
    }
    public void DamageSlider()
    {
        if(slidercomp.TryGetComponent(out RectTransform rectt))
        {
            rectt.DOPunchAnchorPos(Vector2.up * 150f,.6f);
        }
    }
}
