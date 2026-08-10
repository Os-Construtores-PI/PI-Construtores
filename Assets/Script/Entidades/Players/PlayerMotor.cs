using KinematicCharacterController;
using UnityEngine;

[RequireComponent(typeof(KinematicCharacterMotor))]
public class PlayerMotor : MonoBehaviour, ICharacterController
{
  [Header("KCC Motor")]
  public KinematicCharacterMotor Engine { get; private set; }

  [Header("Player Reference")]
  [SerializeField]
  private Player _player;

  public Vector3 Velocity
  {
    get => Engine.BaseVelocity;
    set => Engine.BaseVelocity = value;
  }

  public bool OverrideMotorRotation { get; set; } = false;

  public void SetVelocity(Vector3 velocity) => Engine.BaseVelocity = velocity;

  public void ResetVelocity() => Engine.BaseVelocity = Vector3.zero;

  public void AddVelocity(Vector3 delta) => Engine.BaseVelocity += delta;

  public bool IsGrounded => Engine.GroundingStatus.IsStableOnGround;
  public bool WasGrounded => Engine.LastGroundingStatus.IsStableOnGround;
  public bool IsStableOnGround => Engine.GroundingStatus.IsStableOnGround;
  public bool IsOnStableGround =>
    Engine.GroundingStatus.FoundAnyGround && Engine.GroundingStatus.IsStableOnGround;
  public Vector3 GroundNormal => Engine.GroundingStatus.GroundNormal;
  public Collider GroundCollider => Engine.GroundingStatus.GroundCollider;

  public CapsuleCollider CharacterCapsule => Engine.Capsule;

  public Vector3 CharacterUp => Engine.CharacterUp;

  private void Awake()
  {
    Engine = GetComponent<KinematicCharacterMotor>();
    Engine.CharacterController = this;
  }

  private void OnValidate()
  {
    if (_player == null)
      _player = GetComponent<Player>();
  }

  public void BeforeCharacterUpdate(float deltaTime) { }

  public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
  {
    if (OverrideMotorRotation)
      return;

    if (_player.Direction.sqrMagnitude > 0.01f)
    {
      Quaternion targetRot = Quaternion.LookRotation(_player.Direction, Engine.CharacterUp);
      currentRotation = Quaternion.Slerp(currentRotation, targetRot, 15f * deltaTime);
    }
  }

  public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
  {
    ApplyLocomotionVelocity(ref currentVelocity, deltaTime);
    _player.ActionLayer.ApplyKCCVelocity(_player, ref currentVelocity, deltaTime);
  }

  private void ApplyLocomotionVelocity(ref Vector3 currentVelocity, float deltaTime)
  {
    var state = _player.LocomotionLayer.CurrentState;

    if (state is ILocomotionState<Player> locomotionState)
    {
      locomotionState.CalculateKCCVelocity(_player, ref currentVelocity, deltaTime);
    }
  }

  public void PostGroundingUpdate(float deltaTime)
  {
    if (IsGrounded && !WasGrounded)
    {
      _player.CurrentJumpCount = 0;
      _player.CurrentDashCount = 0;

      var states = _player.ActionLayer.GetCurrentStates();
      foreach (var state in states)
      {
        if (state is IGroundCollisionHandler handler)
          handler.OnLanded(_player);
      }
    }

    if (!IsGrounded && WasGrounded)
    {
      var states = _player.ActionLayer.GetCurrentStates();
      foreach (var state in states)
      {
        if (state is IGroundCollisionHandler handler)
          handler.OnLeftGround(_player);
      }
    }
  }

  public void AfterCharacterUpdate(float deltaTime)
  {
    if (
      _player.ActionLayer.GetActive<PlayerActionStateRailSlide>()
      is PlayerActionStateRailSlide railSlide
    )
    {
      railSlide.ApplyTargetPosition(_player);
    }

    UpdateAnimator();
  }

  public bool IsColliderValidForCollisions(Collider collider)
  {
    if (collider == null)
      return false;
    if (collider.transform.IsChildOf(transform))
      return false;
    return true;
  }

  public void OnGroundHit(
    Collider hitCollider,
    Vector3 hitNormal,
    Vector3 hitPoint,
    ref HitStabilityReport hitStabilityReport
  )
  {
    var states = _player.ActionLayer.GetCurrentStates();
    foreach (var state in states)
    {
      if (state is IGroundCollisionHandler handler)
        handler.OnGroundHit(_player, hitCollider, hitNormal, hitPoint);
    }
  }

  public void OnMovementHit(
    Collider hitCollider,
    Vector3 hitNormal,
    Vector3 hitPoint,
    ref HitStabilityReport hitStabilityReport
  )
  {
    var states = _player.ActionLayer.GetCurrentStates();
    foreach (var state in states)
    {
      if (state is IWallCollisionHandler handler)
        handler.OnWallHit(_player, hitCollider, hitNormal, hitPoint);
    }
  }

  public void ProcessHitStabilityReport(
    Collider hitCollider,
    Vector3 hitNormal,
    Vector3 hitPoint,
    Vector3 atCharacterPosition,
    Quaternion atCharacterRotation,
    ref HitStabilityReport hitStabilityReport
  ) { }

  public void OnDiscreteCollisionDetected(Collider hitCollider) { }

  private void UpdateAnimator()
  {
    Vector3 vel = Engine.BaseVelocity;
    _player.AnimatorComponent.SetFloat(Constants.AnimatorFloatNames.VelocityY, vel.y);
    _player.AnimatorComponent.SetFloat(
      Constants.AnimatorFloatNames.VelocityX,
      new Vector2(vel.x, vel.z).sqrMagnitude
    );
    _player.AnimatorComponent.SetBool(Constants.AnimatorBoolNames.IsGrounded, IsGrounded);
  }
}
