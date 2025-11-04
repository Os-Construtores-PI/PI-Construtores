using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerActionStateDash : IState<PlayerContext>
{
    private float timeToExit;
    private float timeToExitWalker = 0.0f;
    private int Priority => 10;

    public ActionType Type => ActionType.Dash;

    public HashSet<ActionType> IncompatibleActions => new() {};

    public void Enter(PlayerContext context)
    {
        context.OverrideGlobal = true;


        context.DashDirection = context.MoveInput != Vector2.zero ? context.Direction : context.PlayerTransform.forward;
        context.DashDuration = context.DashDistance / context.DashSpeed;
        timeToExit = context.DashDuration;
        context.IsDashing = true;
        context.CanDash = false;
        context.MovementVector = new(context.MovementVector.x, 0, context.MovementVector.z);
        context.DashCurrent += 1;
        context.CanMove = false;
        PlayDashVisual(context.PlayerTransform,context.DashDuration);

        if (context.DashScript != null)
        {
            if (!context.DashScript.gameObject.activeInHierarchy)
                context.DashScript.gameObject.SetActive(true);
            context.DashScript.OnDashUsed();
        }
    }

    public void Exit(PlayerContext context)
    {
        context.CanDash = true;
        context.OverrideGlobal = false;

        ResetDashHUD(context.DashScript);
    }

    public void FixedUpdate(PlayerContext context)
    {
        ExitTimer(context);
    }

    public void Update(PlayerContext context) {}

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
        DOTween
            .Sequence()
            .Append(transform.DOScaleY(0.65f, duration * 0.6f))
            .Append(transform.DOScaleY(1f, duration * 0.4f))
            .SetEase(Ease.InOutSine)
            .SetUpdate(UpdateType.Fixed);
    }

    private void ExitTimer(PlayerContext context)
    {
        if (timeToExitWalker < timeToExit)
        {
            timeToExitWalker += Time.deltaTime;
            context.PlayerController.Move(context.DashSpeed * Time.deltaTime * context.DashDirection);
        }
        else
        {
            context.ActionLayer.PopStateDeferred(context);
        }
    }
}
