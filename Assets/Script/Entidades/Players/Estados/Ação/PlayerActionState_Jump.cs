using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerActionStateJump : IState<Player>
{
  public ActionType Type => ActionType.Jump;
  public HashSet<ActionType> IncompatibleActions => _incompatibleActions;
  private readonly HashSet<ActionType> _incompatibleActions = new();

  [SerializeField]
  private float _jumpHeightMultiplierPerExtraJump = 0.35f;

  [SerializeField]
  private float _wallJumpHorizontalBias = 6.5f;

  // Quanto do momentum horizontal do rail é preservado no pulo.
  // 1.0 = preserva tudo; 0.0 = ignora o rail e usa a lógica padrão.
  [SerializeField]
  private float _railMomentumPreservation = 1f;

  // Multiplicador extra de altura ao pular de um rail (feeling de catapulta)
  [SerializeField]
  private float _railJumpHeightBonus = 1.25f;

  public void Enter(Player player)
  {
    Vector3 targetVelocity = player.MovementVector;
    float jumpY;

    // ─── Bounce ───────────────────────────────────────────────────────────
    PlayerActionStateBounce bounceState = player.ActionLayer.GetActive<PlayerActionStateBounce>();
    bool isBouncing = bounceState != null && player.GroundSlamImpactSpeed > 0f;

    if (isBouncing)
    {
      jumpY = bounceState.CalculateBounceImpulse(player, ref targetVelocity);
      player.ActionLayer.ExitState(bounceState, player);
    }
    else
    {
      float jumpMultiplier = 1f + player.CurrentJumpCount * _jumpHeightMultiplierPerExtraJump;
      jumpY = player.JumpForce * jumpMultiplier;
    }

    // ─── Wall Jump ────────────────────────────────────────────────────────
    if (player.TouchingWall)
    {
      Vector3 jumpDir = (Vector3.up + player.LastWallNormal * _wallJumpHorizontalBias).normalized;
      targetVelocity = jumpDir * player.JumpForce * player.WallJumpMultiplier;
      player.TouchingWall = false;
    }
    // ─── Rail Jump (Sonic/JSR) ────────────────────────────────────────────
    // Se o player acabou de sair de um rail (janela de 300ms), herda o momentum
    // horizontal do rail e aplica bonus de altura — a tangente da ponta do spline
    // determina para onde ele vai voar, não o input atual.
    else if (player.Moving.RailExitMomentumTimer.TimeLeft > 0f)
    {
      Vector3 railMomentum = player.RailSlide.RailExitMomentum;
      Vector3 railHorizontal = new(railMomentum.x, 0f, railMomentum.z);

      float railVertical = Mathf.Max(railMomentum.y, 0f);
      jumpY = (player.JumpForce * _railJumpHeightBonus) + railVertical;

      Vector3 currentHorizontal = new(player.MovementVector.x, 0f, player.MovementVector.z);
      Vector3 finalHorizontal = Vector3.Lerp(
        currentHorizontal,
        railHorizontal,
        _railMomentumPreservation
      );

      targetVelocity = new Vector3(finalHorizontal.x, jumpY, finalHorizontal.z);

      player.RailSlide.RailExitMomentum = Vector3.zero;
      player.Moving.RailExitMomentumTimer.Stop();
    }
    else
    {
      targetVelocity.y = jumpY;
    }

    // ─── Rail Slide cancel ────────────────────────────────────────────────
    if (player.ActionLayer.GetActive<PlayerActionStateRailSlide>() != null)
      player.WantsToCancelRailSlide = true;

    if (player.CurrentJumpCount > 0)
      player.AnimatorComponent?.SetTrigger(Constants.AnimatorTriggerNames.DoubleJump);

    player.CurrentJumpCount++;
    player.EffectsSystem?.PlayEffect(EffectType.JumpEffect, 1);

    player.MovementVector = targetVelocity;
    player.ActionLayer.ExitState(this, player);
  }

  public void Exit(Player player) { }

  public void Update(Player player) { }

  public void FixedUpdate(Player player) { }
}
