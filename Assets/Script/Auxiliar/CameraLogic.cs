using UnityEngine;
using Unity.Cinemachine;

public class CameraLogic : Entities
{
    [Header("Referência atual do jogador")]
    [SerializeField] private Player playerTarget;

    private CinemachineCamera currentCamera;

    public override void Awake()
    {
        base.Awake();
        if (playerTarget != null)
            SetTarget(playerTarget);
        SetDistanceCulling();
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

