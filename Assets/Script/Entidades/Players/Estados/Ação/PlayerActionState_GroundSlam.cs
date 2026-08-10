using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerActionStateGroundSlam : IPlayerState<Player>
{
  public PlayerActionType Type => PlayerActionType.GroundSlam;
  public HashSet<PlayerActionType> IncompatibleActions =>
    new() { PlayerActionType.Dash, PlayerActionType.Jump };

  [Header("Componentes")]
  [SerializeField]
  private Collider _groundSlamHitboxCollider;

  [Header("Força de Impacto no Chão")]
  [SerializeField]
  private float SlamForce = 75f;

  [SerializeField]
  private float MaxImpactCap = 30f;

  private bool _deactivated = false;
  private Vector2 _momentum;

  public void Enter(Player player)
  {
    _momentum = new(player.MovementVector.x, player.MovementVector.z);
    _deactivated = false;
    player.GroundSlamImpactSpeed = 0f;
    player.LocomotionLayer.ChangeState(player.Locked, player);
  }

  public void Update(Player player) { }

  public void FixedUpdate(Player player)
  {
    if (!player.IsGrounded)
    {
      player.MovementVector = new(_momentum.x, -SlamForce, _momentum.y);
      _groundSlamHitboxCollider.enabled = true;
      float currentFallSpeed = Mathf.Abs(player.MovementVector.y);
      player.GroundSlamImpactSpeed = Mathf.Min(
        Mathf.Max(player.GroundSlamImpactSpeed, currentFallSpeed),
        MaxImpactCap
      );
    }
    else if (!_deactivated)
    {
      _deactivated = true;
      player.LocomotionLayer.ChangeState(player.Moving, player);
      player.ActionLayer.ExitState(this, player);
    }
  }

  public void Exit(Player player)
  {
    _groundSlamHitboxCollider.enabled = false;
  }
}
