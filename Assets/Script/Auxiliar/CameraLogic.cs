using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class CameraLogic : Entity
{
  [Header("Referência atual do jogador")]
  [SerializeField]
  private Player playerTarget;


  private CinemachineCamera _currentCinemachineCamera;
  private CinemachineCamera _lockOnCinemachineCamera;
  private CinemachineInputAxisController inputAxisController;

  private readonly Dictionary<EntityEffectType, ParticleSystem> effects = new();

  [Header("Sombras")]
  [SerializeField]
  private float shadowDistance = 40f;

  private DrawDistance[] drawDistance;



  public override void Awake()
  {
    base.Awake();

    if(playerTarget != null)
    {
      SetTarget(playerTarget);
    }
  }


  public override void Start()
  {
    base.Start();

    GatherEffects();
    GatherDrawDistances();
  }


  public void Update()
  {

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

    UpdateDrawDistance();

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

  private void GatherDrawDistances()
  {
    drawDistance =
      FindObjectsByType<DrawDistance>(
        FindObjectsInactive.Include,
        FindObjectsSortMode.None);

    if (drawDistance == null)
    {
      drawDistance = System.Array.Empty<DrawDistance>();
    }


    Debug.Log(
        $"[CameraLogic] " +
        $"{drawDistance.Length} DrawDistance encontrados."
    );
  }

  public void UpdateDrawDistance()
  {
    if (
        playerTarget == null ||
        drawDistance == null ||
        drawDistance.Length == 0
    )
    {
      return;
    }


    Vector3 playerPosition =
        playerTarget.transform.position;


    foreach (DrawDistance drawDistance in drawDistance)
    {
      if (drawDistance == null)
      {
        continue;
      }


      drawDistance.UpdateDrawDistance(
          playerPosition
      );
    }
  }


  // ============================================================
  // DISTÂNCIA GERAL DA CÂMERA
  // ============================================================


  // ============================================================
  // ENCONTRA OBJETOS COM DRAW DISTANCE
  // ============================================================



  // ============================================================
  // ATUALIZA DRAW DISTANCE
  // ============================================================



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
