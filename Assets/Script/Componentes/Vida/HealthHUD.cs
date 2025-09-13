using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HealthHUDComponent : MonoBehaviour
{
    [Header("Configurações HUD")]
    public HealthHUDType HUDType;

    public Transform EnemyTarget { get; set; }

    public int IdHealth = 0;

    private Slider _slider;
    private Camera _cachedCamera;


    private void Update()
    {
        if (_slider == null) return;
    }
    private void Awake()
    {
        DOTween.Init();
        _slider = GetComponent<Slider>();
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
        _slider.value = player.Health / player.MaxHealth;

        // Registrar para updates futuros
        player._OnHealthChanged.AddListener(UpdateSlider); // Supondo que você tenha esse evento
    }

    private void InitializePlayerHUD()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.TryGetComponent(out Player playerref) && playerref.ID == IdHealth)
            {
                _slider.DOValue(playerref.Health / playerref.MaxHealth,.5f);
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
}
