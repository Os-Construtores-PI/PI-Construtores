using System.Collections.Generic;
using UnityEngine;

public class PlayerActionStateBounce : IState<Player>
{
  public ActionType Type => ActionType.Bounce;
  public HashSet<ActionType> IncompatibleActions => new();

  private const float BounceWindowDuration = 0.4f;
  private const int MaxBounceCombo = 3;
  private const float BounceFrontImpulse = 30f;
  private const float BounceConversionRate = 0.85f;
  private readonly float[] ComboBonus = { 0f, 0.25f, 0.55f, 0.90f };

  public int Combo { get; private set; } = 0;
  private float _windowLeft;

  public void Enter(Player player)
  {
    Combo = Mathf.Min(Combo + 1, MaxBounceCombo); // combo persiste entre aterrissagens
    _windowLeft = BounceWindowDuration;
  }

  public void Exit(Player player) { }

  public void Update(Player player)
  {
    _windowLeft -= Time.deltaTime;
    if (_windowLeft <= 0f)
    {
      Combo = 0;
      player.ActionLayer.ExitState(this, player);
    }
  }

  public void FixedUpdate(Player player) { }

  // Chamado pelo GroundedState no ExecuteJump
  public float CalculateBounceImpulse(Player player, ref Vector3 move)
  {
    float bonus = ComboBonus[Combo];
    float jumpY = player.GroundSlamImpactSpeed * BounceConversionRate * (1f + bonus);
    jumpY = Mathf.Max(jumpY, player.JumpForce);
    move += player.transform.forward * BounceFrontImpulse;
    player.GroundSlamImpactSpeed = 0f;
    return jumpY;
  }
}
