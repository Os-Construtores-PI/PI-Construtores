using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerActionStateDash : IState<PlayerContext>
{
  private float timeToExit;
  private float timeToExitWalker = 0.0f;
  private int Priority => 10;

  public ActionType Type => ActionType.Dash;

  public HashSet<ActionType> IncompatibleActions => new() { };

  public void Enter(PlayerContext context)
  {
    if (context.IsHardLocked)
      return;

    context.OverrideGlobal = true;

    context.EntityEffects.PlayEffect(Constants.EffectsNames.Player.Dash);
    context.PlayerDashDirection =
      context.PlayerMoveInput != Vector2.zero
        ? context.PlayerDirection
        : context.EntityTransform.forward;
    context.PlayerDashDuration = context.DashDistance / context.PlayerDashSpeed;
    timeToExit = context.PlayerDashDuration;
    context.PlayerIsDashing = true;
    context.PlayerCanDash = false;
    context.PlayerMovementVector = new(
      context.PlayerMovementVector.x,
      0,
      context.PlayerMovementVector.z
    );
    context.PlayerDashCurrent += 1;
    context.PlayerCanMove = false;
    context.PlayerAnimator.SetTrigger(Constants.AnimatorTriggerNames.Dash);
    //PlayDashVisual(context.PlayerModelTransform,context.PlayerDashDuration);

    if (context.PlayerDashScript != null)
    {
      if (!context.PlayerDashScript.gameObject.activeInHierarchy)
        context.PlayerDashScript.gameObject.SetActive(true);
      context.PlayerDashScript.OnDashUsed();
    }
  }

  public void Exit(PlayerContext context)
  {
    context.PlayerCanDash = true;
    context.OverrideGlobal = false;
    context.PlayerAnimator.ResetTrigger(Constants.AnimatorTriggerNames.Dash);
    context.EntityEffects.StopEffect(Constants.EffectsNames.Player.Dash);
    ResetDashHUD(context.PlayerDashScript);
  }

  public void FixedUpdate(PlayerContext context)
  {
    ExitTimer(context);
  }

  public void Update(PlayerContext context) { }

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

  private void ExitTimer(PlayerContext context)
  {
    if (timeToExitWalker < timeToExit)
    {
      timeToExitWalker += Time.deltaTime;
      context.PlayerController.Move(
        context.PlayerDashSpeed * Time.deltaTime * context.PlayerDashDirection
      );
    }
    else
    {
      context.PlayerActionLayer.PopStateDeferred(context);
      timeToExitWalker = 0f;
    }
  }
}
