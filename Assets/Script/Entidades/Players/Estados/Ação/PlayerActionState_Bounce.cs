using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerActionStateBounce : IPlayerState<Player>
{
  public PlayerActionType Type => PlayerActionType.Bounce;
  public HashSet<PlayerActionType> IncompatibleActions => new();

  [Header("Janela de Bounce")]
  [SerializeField]
  private float BounceWindowDuration = 0.4f;

  [Header("Máximo de Combos")]
  [SerializeField]
  private int MaxBounceCombo = 3;

  [Header("Força horizontal pra frente")]
  [SerializeField]
  private float BounceFrontImpulse = 30f;

  [Header("Conversão de Impacto para Bounce")]
  [SerializeField]
  private float BounceConversionRate = 0.85f;
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
