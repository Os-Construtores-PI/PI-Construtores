using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class CameraLogic : Entity
{
  [Header("Câmera")]
  [SerializeField]
  private float _distance = 200f;

  [Header("Referência atual do jogador")]
  [SerializeField]
  private Player playerTarget;

  private Camera renderCamera;

  private CinemachineCamera _currentCinemachineCamera;
  private CinemachineCamera _lockOnCinemachineCamera;
  private CinemachineInputAxisController inputAxisController;

  private readonly Dictionary<EntityEffectType, ParticleSystem> effects = new();

  [Header("Sombras")]
  [SerializeField]
  private float shadowDistance = 40f;

  [Header("Objetos com Draw Distance")]
  [SerializeField]
  private bool useObjectDrawDistance = true;

  [SerializeField]
  [Min(0.05f)]
  private float drawDistanceUpdateInterval = 0.15f;

  private DrawDistance[] drawDistanceObjects;
  private float drawDistanceTimer;


  public override void Awake()
  {
    base.Awake();

    // Procura a câmera física
    renderCamera = GetComponent<Camera>();

    if (renderCamera == null)
    {
      renderCamera = GetComponentInChildren<Camera>(true);
    }

    if (renderCamera == null)
    {
      Debug.LogWarning(
          "[CameraLogic] Nenhuma câmera física encontrada."
      );
    }

    if (playerTarget != null)
    {
      SetTarget(playerTarget);
    }

    SetCameraDistance();

    GatherDrawDistanceObjects();
  }


  public override void Start()
  {
    base.Start();

    GatherEffects();
  }


  public void Update()
  {
    // ============================================
    // DRAW DISTANCE
    // ============================================

    if (useObjectDrawDistance)
    {
      drawDistanceTimer += Time.deltaTime;

      if (drawDistanceTimer >= drawDistanceUpdateInterval)
      {
        drawDistanceTimer = 0f;

        UpdateDrawDistanceObjects();
      }
    }


    // ============================================
    // EFEITOS
    // ============================================

    if (Time.timeScale < 1)
    {
      foreach (
          KeyValuePair<EntityEffectType, ParticleSystem> pair
          in effects
      )
      {
        pair.Value.Stop();
      }
    }


    // ============================================
    // CÂMERA
    // ============================================

    if (
        playerTarget == null ||
        _currentCinemachineCamera == null
    )
    {
      return;
    }


    // Garante referência ao controlador de input
    if (inputAxisController == null)
    {
      _currentCinemachineCamera.TryGetComponent(
          out inputAxisController
      );
    }


    // Câmera bloqueada
    if (playerTarget.CameraLocked)
    {
      if (inputAxisController != null)
      {
        inputAxisController.enabled = false;
      }

      return;
    }


    // Câmera desbloqueada
    if (
        inputAxisController != null &&
        !inputAxisController.enabled
    )
    {
      inputAxisController.enabled = true;
    }
  }


  // ============================================================
  // EFEITOS
  // ============================================================

  private void GatherEffects()
  {
    foreach (
        ParticleSystem particle
        in GetComponentsInChildren<ParticleSystem>()
    )
    {
      if (
          Lookups.Effects.LookupTable.TryGetValue(
              particle.tag,
              out EntityEffectType effectType
          )
      )
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


  private IEnumerator StopEffectsRoutine(
      EntityEffectType effectType,
      float waitTime
  )
  {
    yield return new WaitForSeconds(waitTime);

    effects[effectType].Stop();
  }


  // ============================================================
  // DISTÂNCIA GERAL DA CÂMERA
  // ============================================================

  private void SetCameraDistance()
  {
    if (renderCamera == null)
    {
      Debug.LogWarning(
          "[CameraLogic] Câmera física não encontrada."
      );

      return;
    }

    /*
     * Esta é apenas a distância máxima geral
     * que a câmera consegue enxergar.
     *
     * NÃO existe mais Layer Culling aqui.
     */

    renderCamera.farClipPlane = _distance;

    // Distância das sombras
    QualitySettings.shadowDistance =
        Mathf.Min(shadowDistance, _distance);

    Debug.Log(
        $"[CameraLogic] Far Clip: {_distance}m | " +
        $"Shadow Distance: {shadowDistance}m"
    );
  }


  // ============================================================
  // ENCONTRA OBJETOS COM DRAW DISTANCE
  // ============================================================

  private void GatherDrawDistanceObjects()
  {
    if (!useObjectDrawDistance)
    {
      return;
    }

    drawDistanceObjects =
        FindObjectsByType<DrawDistance>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

    Debug.Log(
        $"[CameraLogic] " +
        $"{drawDistanceObjects.Length} " +
        $"objetos com DrawDistance encontrados."
    );
  }


  // ============================================================
  // ATUALIZA DRAW DISTANCE
  // ============================================================

  private void UpdateDrawDistanceObjects()
  {
    if (!useObjectDrawDistance)
    {
      return;
    }


    if (
        drawDistanceObjects == null ||
        drawDistanceObjects.Length == 0
    )
    {
      return;
    }


    if (renderCamera == null)
    {
      return;
    }


    Vector3 cameraPosition =
        renderCamera.transform.position;


    foreach (DrawDistance drawObject in drawDistanceObjects)
    {
      if (drawObject == null)
      {
        continue;
      }


      drawObject.UpdateObjects(cameraPosition);
    }
  }


  // ============================================================
  // TARGET
  // ============================================================

  public void SetTarget(
      Player newTarget,
      CinemachineCamera freeLook = null,
      CinemachineCamera boostCam = null
  )
  {
    if (newTarget == null)
    {
      return;
    }


    playerTarget = newTarget;

    id = newTarget.ID;


    Transform targetTransform =
        newTarget.transform.Find("TargetCam");


    if (targetTransform == null)
    {
      Debug.LogWarning(
          $"CameraLogic: Player {newTarget.name} " +
          $"não possui filho 'TargetCam'. " +
          $"Usando root transform."
      );

      targetTransform =
          newTarget.transform;
    }


    if (freeLook != null)
    {
      freeLook.Follow =
          targetTransform;

      freeLook.LookAt =
          targetTransform;

      _currentCinemachineCamera =
          freeLook;
    }
    else if (_currentCinemachineCamera != null)
    {
      _currentCinemachineCamera.Follow =
          targetTransform;

      _currentCinemachineCamera.LookAt =
          targetTransform;


      if (_lockOnCinemachineCamera != null)
      {
        _lockOnCinemachineCamera.Follow =
            targetTransform;

        _lockOnCinemachineCamera.LookAt =
            targetTransform;
      }
    }


    if (boostCam != null)
    {
      boostCam.Follow =
          targetTransform;

      boostCam.LookAt =
          targetTransform;
    }
  }


  // ============================================================
  // TROCA DE CÂMERA
  // ============================================================

  public void SwitchCamera(
      CinemachineCamera newCam,
      Player newTarget
  )
  {
    if (
        newCam == null ||
        newTarget == null
    )
    {
      return;
    }


    if (_currentCinemachineCamera != null)
    {
      _currentCinemachineCamera.Priority = 0;
    }


    newCam.Priority = 10;

    _currentCinemachineCamera =
        newCam;


    SetTarget(
        newTarget,
        newCam
    );
  }
}
