using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

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
  private float _disableDamageCooldown = 6;

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

  [Header("Vibração do Gamepad no Acerto")]
  [SerializeField]
  private float _hitRumbleLowFrequency = 0.5f;

  [SerializeField]
  private float _hitRumbleHighFrequency = 0.1f;

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

  [Header("Configurações da tremida da porrada de lockOn")]
  [SerializeField]
  private float _vibrationAmplitude;

  [SerializeField]
  private float _vibrationFrequency;

  [SerializeField]
  private float _vibrationDuration;

  private bool _hasHit;
  private float _currentGraceTime;
  private Player _currentPlayer;
  private HitboxComponent _hitboxComponent;

  private float _currentVerticalVelocity;
  private float _targetVerticalVelocity;
  private Tween _verticalTween;

  // KCC: guarda a direção do dash para o pipeline de velocidade
  private Vector3 _dashVelocity;
  private bool _isInGraceTime;

  public PlayerActionType Type => PlayerActionType.Dash;
  public HashSet<PlayerActionType> IncompatibleActions => new() { PlayerActionType.GroundSlam };

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
      _hitboxComponent.Hit.RemoveListener(OnDashHitDetected);
      _hitboxComponent.Hit.AddListener(OnDashHitDetected);
    }

    _hasHit = false;
    _currentGraceTime = 0f;
    timeToExitWalker = 0f;
    _currentVerticalVelocity = 0f;
    _targetVerticalVelocity = 0f;
    _isInGraceTime = false;
    _dashVelocity = Vector3.zero;

    player.LocomotionLayer.ChangeState(player.Locked, player);
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

    player.Motor.Engine.ForceUnground(0.1f);

    player.EffectsSystem.PlayEffect(EntityEffectType.PlayerDashEffect, player.DashDuration);
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
    // Timer de saída
    if (timeToExitWalker < timeToExit && player.IsDashing)
    {
      timeToExitWalker += Time.fixedDeltaTime;
    }
    else
    {
      player.ActionLayer.ExitStateDeferred(this, player);
      timeToExitWalker = 0f;
    }

    // Atualiza rotação durante o dash (seguir alvo)
    if (player.LockedTarget != null && !_isInGraceTime)
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

    // Grace time: atualiza direção e rotação, mas NÃO move aqui
    if (_isInGraceTime && _currentGraceTime > 0f)
    {
      _currentGraceTime -= Time.fixedDeltaTime;

      float elapsedT = 1f - Mathf.Clamp01(_currentGraceTime / _graceTimeDuration);
      _currentVerticalVelocity = _verticalImpulseCurve.Evaluate(elapsedT) * _bounceUpwardForce;

      // Atualiza direção durante grace time
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
    LevelPlayerRotation(player);

    player.LocomotionLayer.ChangeState(player.Moving, player);

    _dashHitboxCollider.enabled = false;
    player.AnimatorComponent.ResetTrigger(Constants.AnimatorTriggerNames.Dash);
    player.EffectsSystem.StopEffect(EntityEffectType.PlayerDashEffect);
    Gamepad.current?.SetMotorSpeeds(0, 0);

    if (!_hasHit)
    {
      Vector3 postDash =
        new Vector3(player.DashDirection.x, 0, player.DashDirection.z) * player.DashSpeed;
      player.Motor.Engine.BaseVelocity = postDash;
    }
    else
    {
      player.Motor.Engine.BaseVelocity = new Vector3(0, _currentVerticalVelocity * 0.5f, 0);
    }

    ResetDashHUD(player.DashHudScript);
  }

  public bool UpdateKCCVelocity(Player player, ref Vector3 currentVelocity, float deltaTime)
  {
    if (_isInGraceTime && _currentGraceTime > 0f)
    {
      Vector3 inputDir = Vector3.zero;
      if (player.MoveInput != Vector2.zero)
      {
        inputDir = CalculateRawInputDirection(player);
      }
      else if (player.LockedTarget != null)
      {
        inputDir = (player.LockedTarget.transform.position - player.transform.position).normalized;
        inputDir.y = 0;
      }

      float horizontalSpeed = player.Speed * 0.3f;
      Vector3 horizontalVel = inputDir * horizontalSpeed;

      currentVelocity = new Vector3(horizontalVel.x, _currentVerticalVelocity, horizontalVel.z);

      return true;
    }

    currentVelocity = player.DashDirection * player.DashSpeed;
    return true;
  }

  private void OnDashHitDetected()
  {
    if (_currentPlayer == null || _hasHit)
      return;

    _hasHit = true;
    _isInGraceTime = true;
    _currentGraceTime = _graceTimeDuration;
    timeToExit += _graceTimeDuration;

    _dashHitboxCollider.enabled = false;

    _currentPlayer.Motor.Engine.BaseVelocity = Vector3.zero;

    _currentPlayer.CurrentDashCount = 0;
    _currentPlayer.transform.up = Vector3.up;
    _currentPlayer.CustomShake.Invoke(
      _currentPlayer.ID,
      _vibrationAmplitude,
      _vibrationFrequency,
      _vibrationDuration
    );
    Gamepad.current?.SetMotorSpeeds(_hitRumbleLowFrequency, _hitRumbleHighFrequency);

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

  private void LevelPlayerRotation(Player player)
  {
    Vector3 flatForward = Vector3.ProjectOnPlane(player.transform.forward, Vector3.up);

    if (flatForward.sqrMagnitude < 0.0001f)
      flatForward = Vector3.ProjectOnPlane(player.DashDirection, Vector3.up);

    if (flatForward.sqrMagnitude > 0.0001f)
      player.transform.rotation = Quaternion.LookRotation(flatForward.normalized, Vector3.up);
  }
}
