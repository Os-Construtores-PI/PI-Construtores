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

  [Header("Efeitos")]
  [SerializeField]
  private Transform effectsRoot;

  private CinemachineCamera _currentCinemachineCamera;
  private CinemachineCamera _lockOnCinemachineCamera;
  private CinemachineInputAxisController inputAxisController;
  private readonly EffectsWorker _effects = new();
  private bool _effectsPaused;
  private bool _speedlinesOn;

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
    _effects.InitEffects(effectsRoot ?? transform);
  }

  public void Update()
  {
    bool paused = Time.timeScale < 1;
    if (paused && !_effectsPaused)
    {
      _effects.StopAllEffects();
      _speedlinesOn = false;
    }
    _effectsPaused = paused;

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

  public void SpeedlinesFX(bool set)
  {
    if (set == _speedlinesOn)
      return;

    if (set)
    {
      if (GameContext.IsTutorialActive || Time.timeScale < 1)
        return;

      _effects.PlayEffect(EntityEffectType.PlayerSpeedEffect);
    }
    else
    {
      _effects.StopEffect(EntityEffectType.PlayerSpeedEffect);
    }

    _speedlinesOn = set;
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
