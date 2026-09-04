using UnityEngine;

public class DirectionalTrampoline : LockableObject
{
  [SerializeField]
  private float _impulseForce = 10f;

  [SerializeField]
  private Color _gizmoColor = Color.white;

  [SerializeField]
  private float _jumpCooldown = 1f;

  private bool _canJump = true;
  private Timer _jumpTimer = new();

  public void Jump(Player player)
  {
    _canJump = false;
    player.ActionLayer.PopEveryState(player);
    player.Motor.Velocity = Vector3.zero;
    player.Motor.Velocity = transform.up * _impulseForce;
    player.Motor.Engine.ForceUnground(.1f);
    player.IsImpulsioned = true;
    player.CurrentDashCount = 0;
    player.BoostValue += _boostGrace;
    player.DisableLockIn();
    player.AnimatorComponent.SetTrigger(Constants.AnimatorTriggerNames.WasVerticalBoosted);
    _jumpTimer.Start(_jumpCooldown);
  }

  public void Update()
  {
    if (_jumpTimer.Tick(Time.deltaTime))
    {
      _canJump = true;
    }
  }

#if UNITY_EDITOR
  public void OnDrawGizmos()
  {
    Gizmos.color = _gizmoColor;
    Gizmos.DrawRay(transform.position, transform.up * 10);
  }
#endif

  public void OnTriggerEnter(Collider collision)
  {
    if (collision.TryGetComponent(out Player player) && _canJump)
    {
      Jump(player);
    }
  }
}
