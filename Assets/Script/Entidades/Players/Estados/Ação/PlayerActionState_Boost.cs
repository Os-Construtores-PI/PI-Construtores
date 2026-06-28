using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerActionStateBoost : IPlayerState<Player>
{
  #region IState
  public PlayerActionType Type => PlayerActionType.Boost;
  public HashSet<PlayerActionType> IncompatibleActions => _incompatibleActions;
  private readonly HashSet<PlayerActionType> _incompatibleActions = new();
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
  private float _continuousBoostUsage = 10f;

  [SerializeField]
  private float _maxVelocity = 100f;

  [Tooltip("Velocidade de rotação em graus por segundo durante o boost.")]
  [SerializeField]
  private float _rotationSpeed = 180f;

  [Header("Boost Componentes")]
  [SerializeField]
  private Collider _boostHitboxCollider;

  [SerializeField]
  private SphereCollider _boostCollectionCollider;

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
    player.Stats.ModifyStatToTarget(StatType.Speed, _velocity);
    player.SpeedLines.Invoke(true);

    if (_boostCollectionCollider != null)
    {
      _boostCollectionCollider.enabled = true;
    }

    if (player.HurtboxCollider != null)
    {
      player.HurtboxCollider.TriggerInvulnerability(1000f);
    }

    if (_boostHitboxCollider != null)
    {
      _boostHitboxCollider.enabled = true;
    }

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

    if (_boostCollectionCollider != null)
    {
      _boostCollectionCollider.enabled = false;
    }

    if (player.HurtboxCollider != null)
    {
      player.HurtboxCollider.ResetInvulnerability();
    }

    if (_boostHitboxCollider != null)
    {
      _boostHitboxCollider.enabled = false;
    }

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
