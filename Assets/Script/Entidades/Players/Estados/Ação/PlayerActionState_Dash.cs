using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerActionStateDash : IState<Player>
{
  private float timeToExit;
  private float timeToExitWalker = 0.0f;
  private float _disableDamageCooldown = 4;
  private readonly float _distanceThresold = 2;
  private int Priority => 10;

  public ActionType Type => ActionType.Dash;

  public HashSet<ActionType> IncompatibleActions => new() { };

  public void Enter(Player player)
  {
    if (player.IsHardLocked)
      return;
    player.OverrideGlobal = true;
    player.HurtboxCollider.CanTakeDamage = false;
    player.HurtboxCollider.DamageCooldown = _disableDamageCooldown;
    player.HitboxCollider.enabled = true;

    player.DashDirection =
      player.MoveInput != Vector2.zero ? player.Direction : player.transform.forward;
    if (player.LockedTarget != null)
    {
      Vector3 distanceToTarget = player.LockedTarget.transform.position - player.transform.position;
      player.DashDirection = distanceToTarget.normalized;
      player.DashDistance = distanceToTarget.magnitude;
      if (distanceToTarget.magnitude < _distanceThresold)
      {
        player.DashDirection = Vector3.zero;
        player.DashDistance = 0;
        return;
      }
    }
    player.transform.forward = player.DashDirection;
    player.DashDuration = player.DashDistance / player.DashSpeed;
    timeToExit = player.DashDuration;
    player.IsDashing = true;
    player.CanDash = false;
    player.EffectsWorker.PlayEffect(Constants.EffectsNames.Player.Dash, player.DashDuration);
    player.MovementVector = new(player.MovementVector.x, 0, player.MovementVector.z);
    player.CurrentDashCount += 1;
    player.CanMove = false;
    player.AnimatorComponent.SetTrigger(Constants.AnimatorTriggerNames.Dash);
    //PlayDashVisual(player.PlayerModelTransform,player.PlayerDashDuration);

    if (player.DashHudScript != null)
    {
      if (!player.DashHudScript.gameObject.activeInHierarchy)
        player.DashHudScript.gameObject.SetActive(true);
      player.DashHudScript.OnDashUsed();
    }
  }

  public void Exit(Player player)
  {
    player.CanDash = true;
    player.IsDashing = false;
    player.OverrideGlobal = false;
    player.HitboxCollider.enabled = false;
    player.AnimatorComponent.ResetTrigger(Constants.AnimatorTriggerNames.Dash);
    player.EffectsWorker.StopEffect(Constants.EffectsNames.Player.Dash);
    ResetDashHUD(player.DashHudScript);
  }

  public void FixedUpdate(Player player)
  {
    ExitTimer(player);
  }

  public void Update(Player player) { }

  private void ResetDashHUD(ShiftDashScript dashScript)
  {
    if (dashScript != null)
    {
      if (!dashScript.gameObject.activeInHierarchy)
        dashScript.gameObject.SetActive(true);
      dashScript.OnDashReady();
    }
  }

  private void PlayDashVisual(Transform transform, float duration)
  {
    float initialYScale = transform.localScale.y;
    DOTween
      .Sequence()
      .Append(transform.DOScaleY(initialYScale * 0.5f, duration * 0.6f))
      .Append(transform.DOScaleY(initialYScale * 1f, duration * 0.4f))
      .SetEase(Ease.InOutSine)
      .SetUpdate(UpdateType.Fixed)
      .Play();
  }

  private void ExitTimer(Player player)
  {
    if (timeToExitWalker < timeToExit)
    {
      timeToExitWalker += Time.deltaTime;
      player.CharacterController.Move(player.DashSpeed * Time.deltaTime * player.DashDirection);
    }
    else
    {
      player.ActionLayer.PopStateDeferred(player);
      timeToExitWalker = 0f;
    }
  }
}
