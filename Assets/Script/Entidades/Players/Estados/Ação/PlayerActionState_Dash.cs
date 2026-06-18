using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[System.Serializable]
public class PlayerActionStateDash : IPlayerState<Player>
{
  private float timeToExit;
  private float timeToExitWalker = 0.0f;
  private float _initialDashSpeed;
  private float _initialDashDistance;
  private bool _firstTime;

  [Header("Componentes")]
  [SerializeField]
  private Collider _dashHitboxCollider;

  [Header("Opções de Hitbox")]
  [SerializeField]
  private float _disableDamageCooldown = 4;

  [Header("Distância Minima para Dash")]
  [SerializeField]
  private readonly float _distanceThresold = 2;

  [Header("Velocidades de Dash")]
  [SerializeField]
  private float _minDashSpeed = 40f;

  [SerializeField]
  private float _maxDashSpeed = 60f;

  [SerializeField]
  private float _maxReferenceDistance = 20f;

  [SerializeField]
  private float _speedExponent = 0.1f;

  [Header("Configurações de Quicada e GraceTime")]
  [SerializeField]
  private float _bounceUpwardForce = 6f;

  [SerializeField]
  private float _graceTimeDuration = 0.35f;

  [SerializeField]
  private float _hitStopTimeScale = .05f;

  [SerializeField]
  private float _hitStopTimeScaleDuration = .35f;

  [SerializeField]
  private AnimationCurve _verticalImpulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

  private bool _hasHit;
  private float _currentGraceTime;
  private Player _currentPlayer;
  private HitboxComponent _hitboxComponent;

  private float _currentVerticalVelocity;
  private float _targetVerticalVelocity;
  private Tween _verticalTween;

  public PlayerActionType Type => PlayerActionType.Dash;
  public HashSet<PlayerActionType> IncompatibleActions => new() { { PlayerActionType.GroundSlam } };

  public void Enter(Player player)
  {
    if (player.IsHardLocked)
      return;

    _currentPlayer = player;

    if (!_firstTime)
    {
      _initialDashSpeed = player.DashSpeed;
      _initialDashDistance = player.DashDistance;
      _firstTime = true;
    }

    if (_dashHitboxCollider.TryGetComponent(out HitboxComponent hitbox))
    {
      _hitboxComponent = hitbox;
      _hitboxComponent.Hit.RemoveAllListeners();
      _hitboxComponent.Hit.AddListener(OnDashHitDetected);
    }

    _hasHit = false;
    _currentGraceTime = 0f;
    timeToExitWalker = 0f;
    _currentVerticalVelocity = 0f;
    _targetVerticalVelocity = 0f;

    player.LocomotionLayer.ChangeState(player.Locked, player);
    player.HurtboxCollider.CanTakeDamage = false;
    player.HurtboxCollider.TriggerInvulnerability(_disableDamageCooldown);
    _dashHitboxCollider.enabled = true;

    Vector3 targetDir = Vector3.zero;

    if (player.LockedTarget != null)
    {
      Vector3 diff = player.LockedTarget.transform.position - player.transform.position;
      float dist = diff.magnitude;

      if (dist < _distanceThresold)
      {
        targetDir = player.transform.forward;
        player.DashDistance = 0;
      }
      else
      {
        targetDir = diff.normalized;
        player.DashDistance = dist;
        player.BoostValue += player.LockedTarget.BoostGrace;
        player.DashSpeed = ComputeDashSpeed(dist);
      }
    }
    else
    {
      if (player.MoveInput != Vector2.zero)
      {
        targetDir = CalculateRawInputDirection(player);
      }
      else
      {
        targetDir =
          player.Direction.sqrMagnitude > 0.01f ? player.Direction : player.transform.forward;
      }

      player.DashSpeed = _initialDashSpeed;
      player.DashDistance = _initialDashDistance;
    }

    player.DashDirection = targetDir;
    if (player.DashDirection != Vector3.zero)
      player.transform.rotation = Quaternion.LookRotation(player.DashDirection);

    player.DashDuration = player.DashDistance / player.DashSpeed;
    timeToExit = player.DashDuration;
    player.IsDashing = true;
    player.CanDash = false;

    player.EffectsSystem.PlayEffect(EffectType.DashEffect, player.DashDuration);
    player.CurrentDashCount += 1;
    player.AnimatorComponent.SetTrigger(Constants.AnimatorTriggerNames.Dash);

    if (player.DashHudScript != null)
    {
      if (!player.DashHudScript.gameObject.activeInHierarchy)
        player.DashHudScript.gameObject.SetActive(true);
      player.DashHudScript.OnDashUsed();
    }
  }

  public void FixedUpdate(Player player)
  {
    if (_hasHit && _currentGraceTime > 0f)
    {
      _currentGraceTime -= Time.fixedDeltaTime;

      float elapsedT = 1f - Mathf.Clamp01(_currentGraceTime / _graceTimeDuration);
      _currentVerticalVelocity = _verticalImpulseCurve.Evaluate(elapsedT) * _bounceUpwardForce;

      Vector3 verticalMovement = Vector3.up * _currentVerticalVelocity * Time.fixedDeltaTime;
      player.CharacterController.Move(verticalMovement);

      Vector3 newDir = Vector3.zero;
      if (player.LockedTarget != null)
      {
        newDir = (player.LockedTarget.transform.position - player.transform.position).normalized;
      }
      else if (player.MoveInput != Vector2.zero)
      {
        newDir = CalculateRawInputDirection(player);
      }

      if (newDir != Vector3.zero)
      {
        player.DashDirection = newDir;
        player.transform.rotation = Quaternion.Slerp(
          player.transform.rotation,
          Quaternion.LookRotation(player.DashDirection),
          15f * Time.fixedDeltaTime
        );
      }
    }
    else
    {
      if (player.LockedTarget != null)
      {
        Vector3 diff = player.LockedTarget.transform.position - player.transform.position;
        if (diff.sqrMagnitude > 0.1f)
        {
          player.DashDirection = diff.normalized;
          player.transform.rotation = Quaternion.Slerp(
            player.transform.rotation,
            Quaternion.LookRotation(player.DashDirection),
            40f * Time.fixedDeltaTime
          );
        }
      }
    }

    ExitTimer(player);
  }

  public void Update(Player player) { }

  public void Exit(Player player)
  {
    _verticalTween?.Kill();
    _verticalTween = null;

    if (_hitboxComponent != null)
    {
      _hitboxComponent.Hit.RemoveListener(OnDashHitDetected);
      _hitboxComponent = null;
    }
    _currentPlayer = null;

    player.CanDash = true;
    player.IsDashing = false;

    player.LocomotionLayer.ChangeState(player.Moving, player);

    _dashHitboxCollider.enabled = false;
    player.AnimatorComponent.ResetTrigger(Constants.AnimatorTriggerNames.Dash);
    player.EffectsSystem.StopEffect(EffectType.DashEffect);

    if (!_hasHit)
    {
      Vector3 postDash =
        new Vector3(player.DashDirection.x, 0, player.DashDirection.z) * player.DashSpeed;
      player.MovementVector += postDash;
    }
    else
    {
      player.MovementVector = new Vector3(
        player.MovementVector.x,
        _currentVerticalVelocity * 0.5f,
        player.MovementVector.z
      );
    }

    ResetDashHUD(player.DashHudScript);
  }

  private void OnDashHitDetected()
  {
    if (_currentPlayer == null || _hasHit)
      return;

    _hasHit = true;
    _currentGraceTime = _graceTimeDuration;

    timeToExit += _graceTimeDuration;

    _dashHitboxCollider.enabled = false;
    _currentPlayer.MovementVector = Vector3.zero;
    _currentPlayer.CurrentDashCount = 0;
    _currentPlayer.transform.up = Vector3.up;

    _currentVerticalVelocity = _verticalImpulseCurve.Evaluate(0f) * _bounceUpwardForce;

    Time.timeScale = _hitStopTimeScale;
    DOVirtual.DelayedCall(_hitStopTimeScaleDuration, () => Time.timeScale = 1f);
  }

  private Vector3 CalculateRawInputDirection(Player player)
  {
    Vector3 camForward = player.MainCamera.transform.forward;
    Vector3 camRight = player.MainCamera.transform.right;
    camForward.y = 0;
    camRight.y = 0;
    return (
      camForward.normalized * player.MoveInput.y + camRight.normalized * player.MoveInput.x
    ).normalized;
  }

  private void ExitTimer(Player player)
  {
    if (timeToExitWalker < timeToExit && player.IsDashing)
    {
      timeToExitWalker += Time.fixedDeltaTime;

      if (!(_hasHit && _currentGraceTime > 0f))
      {
        player.CharacterController.Move(
          player.DashSpeed * Time.fixedDeltaTime * player.DashDirection
        );
      }
    }
    else
    {
      player.ActionLayer.ExitStateDeferred(this, player);
      timeToExitWalker = 0f;
    }
  }

  private float ComputeDashSpeed(float distance)
  {
    float t = Mathf.Clamp01(distance / _maxReferenceDistance);
    return _minDashSpeed + (_maxDashSpeed - _minDashSpeed) * Mathf.Pow(t, _speedExponent);
  }

  private void ResetDashHUD(ShiftDashScript dashScript)
  {
    if (dashScript != null && dashScript.gameObject.activeInHierarchy)
      dashScript.OnDashReady();
  }
}
