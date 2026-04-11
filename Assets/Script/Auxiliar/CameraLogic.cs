using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLogic : Entities
{
  [Header("Referência atual do jogador")]
  [SerializeField]
  private Player playerTarget;

  private CinemachineCamera _currentCinemachineCamera;
  private CinemachineCamera _lockOnCinemachineCamera;
  private CinemachineInputAxisController inputAxisController;
  private readonly Dictionary<string, ParticleSystem> effects = new();

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
      foreach (KeyValuePair<string, ParticleSystem> pair in effects)
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
      effects.Add(particle.name, particle);
    }
  }

  public void SpeedFX()
  {
    effects[Constants.EffectsNames.Interface.Speed].Play();
  }

  public void StopSpeedFX()
  {
    StartCoroutine(StopEffectsRoutine(Constants.EffectsNames.Interface.Speed, 0.5f));
  }

  private IEnumerator StopEffectsRoutine(string effect, float waitTime)
  {
    yield return new WaitForSeconds(waitTime);
    effects[effect].Stop();
  }

  private void SetDistanceCulling()
  {
    float[] layersDistance = new float[32];
    for (int i = 0; i < layersDistance.Count(); i++)
    {
      layersDistance[i] = 200;
    }
    if (TryGetComponent(out Camera cam))
    {
      cam.layerCullDistances = layersDistance;
    }
  }

  /// <summary>
  /// Configura a CinemachineCamera para seguir o alvo.
  /// </summary>
  public void SetTarget(
    Player newTarget,
    CinemachineCamera freeLook = null,
    CinemachineCamera lockOn = null
  )
  {
    if (newTarget == null)
      return;

    playerTarget = newTarget;
    id = newTarget.ID;

    // Busca pelo filho "TargetCam" no player
    Transform targetTransform = newTarget.transform.Find("TargetCam");
    if (targetTransform == null)
    {
      Debug.LogWarning(
        $"CameraLogic: Player {newTarget.name} não possui filho 'TargetCam'. Usando root transform."
      );
      targetTransform = newTarget.transform;
    }

    if (freeLook != null && lockOn != null)
    {
      freeLook.Follow = targetTransform;
      freeLook.LookAt = targetTransform;
      lockOn.Follow = targetTransform;
      lockOn.LookAt = targetTransform;
      _lockOnCinemachineCamera = lockOn;
      _currentCinemachineCamera = freeLook;
    }
    else if (_currentCinemachineCamera != null)
    {
      _currentCinemachineCamera.Follow = targetTransform;
      _currentCinemachineCamera.LookAt = targetTransform;
      _lockOnCinemachineCamera.Follow = targetTransform;
      _lockOnCinemachineCamera.LookAt = targetTransform;
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
