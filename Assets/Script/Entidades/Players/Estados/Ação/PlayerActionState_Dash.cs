using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerActionStateDash : IState<Player>
{
  private float timeToExit;
  private float timeToExitWalker = 0.0f;
  private float _disableDamageCooldown = 4;
  private readonly float _distanceThresold = 2;
  private float _initialDashSpeed;
  private float _initialDashDistance;
  private bool _firstTime;
  private float _minDashSpeed = 40f;
  private float _maxDashSpeed = 60f;
  private float _maxReferenceDistance = 20f;
  private float _speedExponent = 0.1f;

  public ActionType Type => ActionType.Dash;
  public HashSet<ActionType> IncompatibleActions => new() { { ActionType.GroundSlam } };

  public void Enter(Player player)
  {
    if (player.IsHardLocked)
      return;

    if (!_firstTime)
    {
      _initialDashSpeed = player.DashSpeed;
      _initialDashDistance = player.DashDistance;
      _firstTime = true;
    }

    player.LocomotionLayer.ChangeState(player.LockedS, player);
    player.HurtboxCollider.CanTakeDamage = false;
    player.HurtboxCollider.TriggerInvulnerability(_disableDamageCooldown);
    player.DashHitboxCollider.enabled = true;

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

    player.EffectsWorker.PlayEffect(Constants.EffectsNames.Player.Dash, player.DashDuration);
    player.CurrentDashCount += 1;
    player.CanMove = false;
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

    ExitTimer(player);
  }

  public void Update(Player player) { }

  public void Exit(Player player)
  {
    player.CanDash = true;
    player.IsDashing = false;
    player.LocomotionLayer.ChangeState(player.AirborneS, player);
    player.DashHitboxCollider.enabled = false;
    player.AnimatorComponent.ResetTrigger(Constants.AnimatorTriggerNames.Dash);
    player.EffectsWorker.StopEffect(Constants.EffectsNames.Player.Dash);
    Vector3 postDash =
      new Vector3(player.DashDirection.x, 0, player.DashDirection.z) * player.DashSpeed;
    player.MovementVector += postDash;

    ResetDashHUD(player.DashHudScript);
  }

  private Vector3 CalculateRawInputDirection(Player player)
  {
    Vector3 camForward = player.CinemachineCamera.transform.forward;
    Vector3 camRight = player.CinemachineCamera.transform.right;
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
      player.CharacterController.Move(
        player.DashSpeed * Time.fixedDeltaTime * player.DashDirection
      );
    }
    else
    {
      player.ActionLayer.PopStateDeferred(player);
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
