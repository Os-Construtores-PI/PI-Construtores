using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class CameraLogic : Entity
{
  [SerializeField]
  private float _distance = 200f;

  [Header("Referência atual do jogador")]
  [SerializeField]
  private Player playerTarget;

  private CinemachineCamera _currentCinemachineCamera;
  private CinemachineCamera _lockOnCinemachineCamera;
  private CinemachineInputAxisController inputAxisController;
  private readonly Dictionary<EntityEffectType, ParticleSystem> effects = new();

  [Header("Draw Distance")]
  [SerializeField] private float drawDistance = 80f;

  [SerializeField] private float shadowDistance = 40f;

  [SerializeField] private LayerMask drawDistanceLayers;

  [Header("Objetos com Draw Distance")]
  [SerializeField] private bool useObjectDrawDistance = true;

  [SerializeField]
  [Min(0.05f)]
  private float drawDistanceUpdateInterval = 0.15f;

  private DrawDistance[] drawDistanceObjects;
  private float drawDistanceTimer;

  public override void Awake()
  {
    base.Awake();
    if (playerTarget != null)
      SetTarget(playerTarget);
    SetDistanceCulling();

    GatherDrawDistanceObjects();
  }

  public override void Start()
  {
    base.Start();
    GatherEffects();
  }

  public void Update()
  {

    if (useObjectDrawDistance)
    {
      drawDistanceTimer += Time.deltaTime;

      if (drawDistanceTimer >= drawDistanceUpdateInterval)
      {
        drawDistanceTimer = 0f;
        UpdateDrawDistanceObjects();
      }
    }

    if (Time.timeScale < 1)
    {
      foreach (KeyValuePair<EntityEffectType, ParticleSystem> pair in effects)
      {
        pair.Value.Stop();
      }
    }

    if (playerTarget == null || _currentCinemachineCamera == null)
      return;


    if (Time.timeScale < 1)
    {
      foreach (KeyValuePair<EntityEffectType, ParticleSystem> pair in effects)
      {
        pair.Value.Stop();
      }
    }
    if (playerTarget == null || _currentCinemachineCamera == null)
      return;

    // Garante referência ao controlador de input
    if (inputAxisController == null)
      _currentCinemachineCamera.TryGetComponent(out inputAxisController);

    if (playerTarget.CameraLocked)
    {
      // 🔥 trava completamente os inputs da câmera
      if (inputAxisController != null)
        inputAxisController.enabled = false;

      return;
    }

    // se destravou → garante que voltou ao normal
    if (inputAxisController != null && !inputAxisController.enabled)
      inputAxisController.enabled = true;
  }

  private void GatherEffects()
  {
    foreach (ParticleSystem particle in GetComponentsInChildren<ParticleSystem>())
    {
      if (Lookups.Effects.LookupTable.TryGetValue(particle.tag, out EntityEffectType effectType))
      {
        effects.Add(effectType, particle);
      }
    }
  }

  public void SpeedlinesFX(bool set)
  {
    if (set)
    {
      effects[EntityEffectType.PlayerSpeedEffect].Play();
    }
    else
    {
      effects[EntityEffectType.PlayerSpeedEffect].Stop();
    }
  }

  private IEnumerator StopEffectsRoutine(EntityEffectType effectType, float waitTime)
  {
    yield return new WaitForSeconds(waitTime);
    effects[effectType].Stop();
  }

  private void SetDistanceCulling()
  {
    Camera cam = GetComponent<Camera>();

    if(cam == null)
    {
      Debug.LogWarning("[CameraLogic] Câmera não encontrada.");
      return;
    }

    float[] layersDistance = new float[32];
    
    // Todas as Layers podem ser renderizadas até o Far Clip.
    for (int i = 0; i < layersDistance.Length; i++)
    {
      layersDistance[i] = _distance;
    }

    // Apenas as Layers selecionadas recebem o culling reduzido
    for (int i = 0; i <= 32; i++)
    {
      if ((drawDistanceLayers.value & (1 << i)) != 0)
      {
        layersDistance[i] = Mathf.Min(drawDistance, _distance);
      }
    }

    cam.layerCullDistances = layersDistance;
    cam.layerCullSpherical = true;

    // Distância máxima geral da câmera
    QualitySettings.shadowDistance = Mathf.Min(shadowDistance, _distance);

    Debug.Log(
        $"[CameraLogic] Far Clip: {_distance}m | " +
        $"Mobile Culling: {drawDistance}m | " +
        $"Shadows: {shadowDistance}m"
    );
  }

  private void GatherDrawDistanceObjects()
  {
    if (!useObjectDrawDistance)
      return;

    drawDistanceObjects = FindObjectsByType<DrawDistance>(
      FindObjectsInactive.Include,
      FindObjectsSortMode.None);

    Debug.Log(
        $"[CameraLogic] {drawDistanceObjects.Length} objetos com DrawDistance encontrados."
    );
  }

  private void UpdateDrawDistanceObjects()
  {
    if (!useObjectDrawDistance)
      return;

    if (drawDistanceObjects == null || drawDistanceObjects.Length == 0)
      return;

    Camera cam = GetComponent<Camera>();

    if (cam == null)
      return;

    Vector3 cameraPosition = cam.transform.position;

    foreach (DrawDistance drawObject in drawDistanceObjects)
    {
      if (drawObject == null)
        continue;

      float distance = Vector3.Distance(
          cameraPosition,
          drawObject.transform.position
      );

      bool shouldRender =
          distance <= drawObject.GetDrawDistance();

      Renderer[] renderers = drawObject.GetRenderers();

      if (renderers == null)
        continue;

      foreach (Renderer renderer in renderers)
      {
        if (renderer == null)
          continue;

        renderer.enabled = shouldRender;
      }
    }
  }

  /// <summary>
  /// Configura a CinemachineCamera para seguir o alvo.
  /// </summary>
  public void SetTarget(
    Player newTarget,
    CinemachineCamera freeLook = null,
    CinemachineCamera boostCam = null
  )
  {
    if (newTarget == null)
      return;

    playerTarget = newTarget;
    id = newTarget.ID;

    Transform targetTransform = newTarget.transform.Find("TargetCam");
    if (targetTransform == null)
    {
      Debug.LogWarning(
        $"CameraLogic: Player {newTarget.name} não possui filho 'TargetCam'. Usando root transform."
      );
      targetTransform = newTarget.transform;
    }

    if (freeLook != null)
    {
      freeLook.Follow = targetTransform;
      freeLook.LookAt = targetTransform;
      _currentCinemachineCamera = freeLook;
    }
    else if (_currentCinemachineCamera != null)
    {
      _currentCinemachineCamera.Follow = targetTransform;
      _currentCinemachineCamera.LookAt = targetTransform;
      _lockOnCinemachineCamera.Follow = targetTransform;
      _lockOnCinemachineCamera.LookAt = targetTransform;
    }

    if (boostCam != null)
    {
      boostCam.Follow = targetTransform;
      boostCam.LookAt = targetTransform;
    }
  }

  /// <summary>
  /// Troca para outra câmera virtual em runtime.
  /// </summary>
  public void SwitchCamera(CinemachineCamera newCam, Player newTarget)
  {
    if (newCam == null || newTarget == null)
      return;

    if (_currentCinemachineCamera != null)
      _currentCinemachineCamera.Priority = 0;

    newCam.Priority = 10;
    _currentCinemachineCamera = newCam;

    SetTarget(newTarget, newCam);
  }
}
