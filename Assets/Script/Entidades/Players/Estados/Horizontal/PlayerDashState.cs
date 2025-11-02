using DG.Tweening;
using UnityEngine;

public class PlayerDashState : IState<PlayerContext>
{
    private float timeToExit;
    private float timeToExitWalker = 0.0f;
    public void Enter(PlayerContext context)
    {
        timeToExit = context.DashCooldown;
        context.DashDirection = context.MoveInput != Vector2.zero ? context.Direction : context.PlayerTransform.forward;
        context.DashDuration = context.DashDistance / context.DashSpeed;
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
        ResetDashHUD(context.DashScript);
    }

    public void FixedUpdate(PlayerContext context)
    {
        Vector3 move = context.MovementVector;
        move.x = QualityOfLife.PlayerFriction(move.x, context.AirFriction,context.MoveInput);
        move.z = QualityOfLife.PlayerFriction(move.z, context.AirFriction,context.MoveInput);
        move = new(move.x, move.y + context.Gravity * Time.deltaTime, move.z);
        context.MovementVector = move;
    }

    public void Update(PlayerContext context)
    {
        ExitTimer(context);
    }

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
        }
        else
        {
            context.HorizontalLayer.ChangeState(new PlayerMovimentState(), context);
        }
    }
}
