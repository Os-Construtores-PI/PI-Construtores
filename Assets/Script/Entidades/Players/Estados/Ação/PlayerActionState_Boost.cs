using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerActionStateBoost : IState<Player>
{
  #region IState
  public ActionType Type => ActionType.Boost;
  public HashSet<ActionType> IncompatibleActions => _incompatibleActions;
  private readonly HashSet<ActionType> _incompatibleActions = new();
  #endregion

  #region Fields

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
  private float _initialBoostUsage = 20f;

  [SerializeField]
  private float _continuousBoostUsage = 10f;

  [SerializeField]
  private float _maxVelocity = 100f;

  [Tooltip("Velocidade de rotação em graus por segundo durante o boost.")]
  [SerializeField]
  private float _rotationSpeed = 180f;

  [SerializeField]
  private SphereCollider _boostCollider;

  private float _playerOriginalSpeed;
  private float _velocity;

  private Quaternion _boostRotation;
  #endregion

  #region IState Callbacks
  public void Enter(Player player)
  {
    _velocity = Mathf.Clamp(player.BoostValue, 0f, _maxVelocity);
    _playerOriginalSpeed = player.Speed;
    _boostRotation = player.transform.rotation;

    float velocityFraction = _velocity / _maxVelocity;

    player.LocomotionLayer.ChangeState(player.LockedInHorizontal, player);

    player.BoostValue -= _initialBoostUsage;
    player.Stats.ModifyStatToTarget(StatType.Speed, _velocity);

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
    player.MainCamera.Lens.FieldOfView = _boostFOV;
  }

  public void Exit(Player player)
  {
    _velocity = 0f;

    float currentYVelocity = player.MovementVector.y;
    player.MovementVector = Vector3.zero;
    player.MovementVector.y = currentYVelocity;

    player.Stats.ModifyStatToTarget(StatType.Speed, _playerOriginalSpeed);
    player.LocomotionLayer.ChangeState(player.Moving, player);

    player.SpeedLines.Invoke(false);
    if (_boostCollider != null)
      _boostCollider.enabled = false;

    player.TrailsSystem.StopEffect(TrailType.MovementTrail);
    player.TrailsSystem.StopEffect(TrailType.MovementSupport1Trail);
    player.TrailsSystem.StopEffect(TrailType.MovementSupport2Trail);

    player.MainCamera.Lens.FieldOfView = _defaultFOV;
  }

  public void Update(Player player)
  {
    player.BoostValue -= _continuousBoostUsage * Time.deltaTime;
    player.BoostValue = Mathf.Max(0f, player.BoostValue);

    if (player.BoostValue <= 0f)
    {
      player.ActionLayer.ExitState(this, player);
      return;
    }

    float turnInput = player.MoveInput.x;
    if (Mathf.Abs(turnInput) > 0.01f)
    {
      _boostRotation *= Quaternion.AngleAxis(
        turnInput * _rotationSpeed * Time.deltaTime,
        Vector3.up
      );
      player.transform.rotation = _boostRotation;
    }

    Vector3 newMovementVector = player.transform.forward * _velocity;
    newMovementVector.y = player.MovementVector.y;

    player.MovementVector = newMovementVector;

    if (!player.PlayerInput.actions.FindAction("Dash / Boost").IsPressed())
    {
      player.ActionLayer.ExitState(this, player);
      return;
    }
  }

  public void FixedUpdate(Player player) { }

  #endregion
}
