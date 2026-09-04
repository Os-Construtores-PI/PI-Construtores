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

  [Header("Bounce")]
  [SerializeField]
  private float BounceConversionRate = 0.85f;

  [SerializeField]
  private float BounceFrontImpulse = 30f;

  [SerializeField]
  private float[] BounceComboBonus = { 0f, 0.25f, 0.55f, 0.90f };

  private bool _deactivated = false;
  private Vector2 _momentum;
  private float _currentVerticalSpeed = 0f;
  private int _bounceCombo = 0;

  public void Enter(Player player)
  {
    _momentum = new(player.Motor.Engine.BaseVelocity.x, player.Motor.Engine.BaseVelocity.z);
    _deactivated = false;
    player.GroundSlamImpactSpeed = 0f;
    player.HurtboxCollider.TriggerInvulnerability(1000);
    _currentVerticalSpeed = 0f;
    _bounceCombo = 0;

    player.LocomotionLayer.ChangeState(player.Locked, player);
    player.Motor.Engine.ForceUnground(0.1f);
    _groundSlamHitboxCollider.enabled = true;
  }

  public void Update(Player player) { }

  public void FixedUpdate(Player player)
  {
    float currentFallSpeed = Mathf.Abs(_currentVerticalSpeed);
    player.GroundSlamImpactSpeed = Mathf.Min(
      Mathf.Max(player.GroundSlamImpactSpeed, currentFallSpeed),
      MaxImpactCap
    );

    if (player.Motor.Engine.GroundingStatus.IsStableOnGround && !_deactivated)
    {
      _deactivated = true;
      OnImpact(player);
    }
  }

  public bool UpdateKCCVelocity(Player player, ref Vector3 currentVelocity, float deltaTime)
  {
    if (_deactivated)
    {
      currentVelocity = Vector3.zero;
      return true;
    }

    _currentVerticalSpeed = Mathf.MoveTowards(
      _currentVerticalSpeed,
      -SlamForce,
      Mathf.Abs(player.GravityValue * player.GravityDownMultiplier * 3f) * deltaTime
    );

    Vector3 horizontal = new Vector3(_momentum.x, 0f, _momentum.y);
    horizontal = Vector3.MoveTowards(horizontal, Vector3.zero, player.AirFriction * deltaTime);
    _momentum = new Vector2(horizontal.x, horizontal.z);

    currentVelocity = new Vector3(_momentum.x, _currentVerticalSpeed, _momentum.y);
    return true;
  }

  private void OnImpact(Player player)
  {
    player.CustomShake.Invoke(player.ID, 0.8f, 15f, 0.4f);

    _bounceCombo = Mathf.Min(_bounceCombo + 1, BounceComboBonus.Length - 1);
    float bonus = BounceComboBonus[_bounceCombo];
    float jumpY = player.GroundSlamImpactSpeed * BounceConversionRate * (1f + bonus);
    jumpY = Mathf.Max(jumpY, player.JumpForce);

    player.Motor.Engine.ForceUnground(0.1f);

    Vector3 bounceVelocity = player.transform.forward * BounceFrontImpulse;
    bounceVelocity.y = jumpY;
    player.Motor.Engine.BaseVelocity = bounceVelocity;

    player.ActionLayer.ExitState(this, player);
  }

  public void Exit(Player player)
  {
    player.LocomotionLayer.ChangeState(player.Moving, player);
    player.HurtboxCollider.ResetInvulnerability();
    player.GroundSlamImpactSpeed = 0f;

    _groundSlamHitboxCollider.enabled = false;
    _currentVerticalSpeed = 0f;
  }
}
