using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class CameraLogic : Entity
{
  [SerializeField]
  private float _distance = 900f;

  [Header("Referência atual do jogador")]
  [SerializeField]
  private Player playerTarget;

  [Header("Câmeras Cinemachine")]
  [SerializeField]
  private CinemachineCamera _lockOnCinemachineCamera;

  [Header("Modo trilho")]
  [SerializeField]
  private bool _railDriven = true;

  private CinemachineCamera _currentCinemachineCamera;
  private CinemachineInputAxisController inputAxisController;
  private readonly Dictionary<EntityEffectType, ParticleSystem> effects = new();

  public override void Awake()
  {
    base.Awake();
    if (playerTarget != null)
      SetTarget(playerTarget);
    SetDistanceCulling();
  }

  public override void Start()
  {
    base.Start();
    GatherEffects();
  }

  public void Update()
  {
    if (Time.timeScale < 1)
    {
      foreach (KeyValuePair<EntityEffectType, ParticleSystem> pair in effects)
      {
        pair.Value.Stop();
      }
    }

    if (playerTarget == null || _currentCinemachineCamera == null)
      return;

    if (inputAxisController == null)
      _currentCinemachineCamera.TryGetComponent(out inputAxisController);

    if (playerTarget.CameraLocked)
    {
      if (inputAxisController != null)
        inputAxisController.enabled = false;

      return;
    }

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
    float[] layersDistance = new float[32];
    for (int i = 0; i < layersDistance.Count(); i++)
    {
      layersDistance[i] = _distance;
    }

    if (TryGetComponent(out Camera cam))
    {
      cam.layerCullDistances = layersDistance;
    }
  }

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

    Transform followTarget = _railDriven ? null : targetTransform;

    if (freeLook != null)
      _currentCinemachineCamera = freeLook;

    ApplyTarget(_currentCinemachineCamera, followTarget);
    ApplyTarget(_lockOnCinemachineCamera, followTarget);
    ApplyTarget(boostCam, followTarget);
  }

  private static void ApplyTarget(CinemachineCamera cam, Transform target)
  {
    if (cam == null)
      return;

    cam.Follow = target;
    cam.LookAt = target;
  }

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
