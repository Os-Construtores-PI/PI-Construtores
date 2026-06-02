using System.Collections.Generic;
using UnityEngine;

// NOTA: Removi 'using DG.Tweening;' pois não estava sendo usado no código.
// Se for usar DoTween para suavizar a FOV ou Shake no futuro, pode readicionar.

[System.Serializable]
public class PlayerActionStateBoost : IState<Player>
{
  #region IState
  public ActionType Type => ActionType.Boost;
  public HashSet<ActionType> IncompatibleActions => _incompatibleActions;
  private readonly HashSet<ActionType> _incompatibleActions = new();
  #endregion

  #region Fields
  private const int MainCameraPriority = 10;
  private const int BoostCameraPriority = 20;
  private const int InactivePriority = 0;

  [Header("Camera")]
  [SerializeField]
  private float _defaultFOV = 80f;

  [SerializeField]
  private float _boostFOV = 120f;

  [Header("Enter Effects")]
  [SerializeField]
  private float _enterShakeAmplitude = 1.5f;

  [SerializeField]
  private float _enterShakeFrequency = 0.4f;

  [SerializeField]
  private float _enterShakeDuration = 0.25f;

  [Header("Boost Settings")]
  [SerializeField]
  private float _rotationSpeed = 50f;

  [SerializeField]
  private float _boostUsage = 20f;

  [SerializeField]
  private float _slopeLimit = 30f;

  [SerializeField]
  private float _maxVelocity = 100f;

  [SerializeField]
  private float _forcedDuration = 1.5f;

  [Tooltip("Valor mínimo do input de movimento para permitir o cancelamento antecipado do boost.")]
  [SerializeField]
  private float _cancelInputThreshold = 0.1f;

  [Tooltip("Distância extra do raycast para detectar o chão em alta velocidade.")]
  [SerializeField]
  private float _groundCheckExtraDistance = 1f;

  [SerializeField]
  private SphereCollider _boostCollider;

  private float _playerOriginalSpeed;
  private float _velocity;
  private float _forcedTimer;
  private bool _isFree;
  private bool _canCancel;
  #endregion

  #region IState Callbacks
  public void Enter(Player player)
  {
    _velocity = Mathf.Clamp(player.DashSlashBoostButton.Value, 0f, _maxVelocity);
    _playerOriginalSpeed = player.Speed;
    _forcedTimer = _forcedDuration;
    _isFree = false;
    _canCancel = false;

    float velocityFraction = _velocity / _maxVelocity;

    player.LocomotionLayer.ChangeState(player.LockedInHorizontal, player);
    player.SpeedLines.Invoke(true);

    if (_boostCollider != null)
      _boostCollider.enabled = true;

    player.CustomShake.Invoke(
      player.ID,
      _enterShakeAmplitude * velocityFraction,
      _enterShakeFrequency * velocityFraction,
      _enterShakeDuration
    );

    player.TrailsSystem.PlayEffect(TrailType.MovementTrail);
    player.TrailsSystem.PlayEffect(TrailType.MovementSupport1Trail);
    player.TrailsSystem.PlayEffect(TrailType.MovementSupport2Trail);

    SetBoostCamera(player, active: true);
    player.MainCamera.Lens.FieldOfView = _boostFOV;
  }

  public void Exit(Player player)
  {
    _velocity = 0f;
    _forcedTimer = 0f;
    _isFree = false;
    _canCancel = false;

    player.Stats.ModifyStatToTarget(StatType.Speed, _playerOriginalSpeed);

    player.SpeedLines.Invoke(false);
    if (_boostCollider != null)
      _boostCollider.enabled = false;

    player.TrailsSystem.StopEffect(TrailType.MovementTrail);
    player.TrailsSystem.StopEffect(TrailType.MovementSupport1Trail);
    player.TrailsSystem.StopEffect(TrailType.MovementSupport2Trail);

    SetBoostCamera(player, active: false);
    player.MainCamera.Lens.FieldOfView = _defaultFOV;
  }

  public void Update(Player player)
  {
    player.DashSlashBoostButton.Value -= _boostUsage * Time.deltaTime;
    player.DashSlashBoostButton.Value = Mathf.Max(0f, player.DashSlashBoostButton.Value);

    if (player.DashSlashBoostButton.Value <= 0f)
    {
      player.ActionLayer.ExitState(this, player);
      return;
    }

    if (player.MoveInput.y <= _cancelInputThreshold && _canCancel)
    {
      player.ActionLayer.ExitState(this, player);
      return;
    }

    if (_canCancel && !_isFree)
    {
      player.ActionLayer.ExitState(this, player);
      return;
    }

    if (!_isFree)
    {
      _forcedTimer -= Time.deltaTime;
      if (_forcedTimer <= 0f)
      {
        TransitionToFreeMovement(player);
      }
    }
  }

  public void FixedUpdate(Player player)
  {
    if (_isFree)
      return;

    RotatePlayer(player);
    ApplyVelocity(player);
  }
  #endregion

  #region Private Methods
  private void TransitionToFreeMovement(Player player)
  {
    _isFree = true;
    _canCancel = true;

    Vector3 safeMovement = player.MovementVector;
    safeMovement.y = 0f;
    player.MovementVector = safeMovement;
    player.Stats.ModifyStatToTarget(StatType.Speed, _velocity);
    player.LocomotionLayer.ChangeState(player.Moving, player);
    player.MainCamera.Lens.FieldOfView = _boostFOV;
    SetBoostCamera(player, active: false);
  }

  private void RotatePlayer(Player player)
  {
    player.transform.Rotate(
      Vector3.up,
      player.MoveInput.x * _rotationSpeed * Time.fixedDeltaTime,
      Space.World
    );
  }

  private void ApplyVelocity(Player player)
  {
    if (OnSlope(player, out RaycastHit hit))
    {
      Vector3 slopeDir = Vector3.ProjectOnPlane(player.transform.forward, hit.normal).normalized;
      player.MovementVector = slopeDir * _velocity;
    }
    else
    {
      Vector3 horizontal = player.transform.forward * _velocity;
      player.MovementVector = new Vector3(horizontal.x, player.MovementVector.y, horizontal.z);
    }
  }

  private bool OnSlope(Player player, out RaycastHit hit)
  {
    if (player.CharacterController == null)
    {
      hit = default;
      return false;
    }

    Vector3 rayOrigin =
      player.transform.position
      - Vector3.up
        * (player.CharacterController.height * 0.5f - player.CharacterController.skinWidth);
    float reach = (player.CharacterController.height * 0.5f) + _groundCheckExtraDistance;

    if (Physics.Raycast(rayOrigin, Vector3.down, out hit, reach))
    {
      float angle = Vector3.Angle(hit.normal, Vector3.up);
      return angle > 0f && angle <= _slopeLimit;
    }

    return false;
  }

  private static void SetBoostCamera(Player player, bool active)
  {
    if (player.MainCamera != null)
    {
      player.MainCamera.Priority = active ? InactivePriority : MainCameraPriority;
    }

    if (player.BoostCamera != null)
    {
      player.BoostCamera.Priority = active ? BoostCameraPriority : InactivePriority;
    }
  }
  #endregion
}
