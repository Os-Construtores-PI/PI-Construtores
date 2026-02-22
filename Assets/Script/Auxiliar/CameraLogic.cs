using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
using System.Collections;

public class CameraLogic : Entities
{
    [Header("Referência atual do jogador")]
    [SerializeField] private Player playerTarget;

    private CinemachineCamera currentCamera;
    private CinemachineInputAxisController inputAxisController;
    private readonly Dictionary<string,ParticleSystem> effects = new();

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

    private void Update()
    {
        if (playerTarget == null || currentCamera == null)
            return;

        // Garante referência ao controlador de input
        if (inputAxisController == null)
            currentCamera.TryGetComponent(out inputAxisController);

        if (playerTarget.Context.CameraLocked)
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
      foreach(ParticleSystem particle in GetComponentsInChildren<ParticleSystem>())
      {
        effects.Add(particle.name,particle);
      }
    }


    public void SpeedFX()
    {
      effects[Constants.EffectsNames.Interface.Speed].Play();
    }
    public void StopSpeedFX()
    {
      StartCoroutine(StopEffectsRoutine(Constants.EffectsNames.Interface.Speed,0.5f));
    }
    private IEnumerator StopEffectsRoutine(string effect,float waitTime)
    {
      yield return new WaitForSeconds(waitTime);
      effects[effect].Stop();
    }

    private void SetDistanceCulling()
    {
        float[] layersDistance = new float[32];
        layersDistance[0] = 40;
        layersDistance[6] = 40;
        layersDistance[7] = 40;
        layersDistance[8] = 40;
        layersDistance[9] = 40;
        layersDistance[12] = 100;
        layersDistance[13] = 200;
        if(TryGetComponent(out Camera cam))
        {
            cam.layerCullDistances = layersDistance; 
        }
    }
    /// <summary>
    /// Configura a CinemachineCamera para seguir o alvo.
    /// </summary>
    public void SetTarget(Player newTarget, CinemachineCamera freeLook = null)
    {
        if (newTarget == null) return;

        playerTarget = newTarget;
        id = newTarget.ID;

        // Busca pelo filho "TargetCam" no player
        Transform targetTransform = newTarget.transform.Find("TargetCam");
        if (targetTransform == null)
        {
            Debug.LogWarning($"CameraLogic: Player {newTarget.name} não possui filho 'TargetCam'. Usando root transform.");
            targetTransform = newTarget.transform;
        }

        if (freeLook != null)
        {
            freeLook.Follow = targetTransform;
            freeLook.LookAt = targetTransform;
            currentCamera = freeLook;
        }
        else if (currentCamera != null)
        {
            currentCamera.Follow = targetTransform;
            currentCamera.LookAt = targetTransform;
        }
    }

    /// <summary>
    /// Troca para outra câmera virtual em runtime.
    /// </summary>
    public void SwitchCamera(CinemachineCamera newCam, Player newTarget)
    {
        if (newCam == null || newTarget == null) return;

        if (currentCamera != null)
            currentCamera.Priority = 0;

        newCam.Priority = 10;
        currentCamera = newCam;

        SetTarget(newTarget, newCam);
    }
}

