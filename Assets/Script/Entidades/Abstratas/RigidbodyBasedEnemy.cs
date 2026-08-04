using UnityEngine;

public class RigidbodyBasedEnemy : Enemies
{
  public override void Awake()
  {
    base.Awake();
    _rb ??= GetComponent<Rigidbody>();
  }

  protected override void TriggerKnockback(Player player)
  {
    Vector3 forceDir = (transform.position - player.transform.position).normalized;
    forceDir.y *= _verticalMultiplier;

    _rb.AddForce(forceDir.normalized * _knockbackForce, ForceMode.VelocityChange);
    return;
  }

  protected void MoveWithRigidbody(Vector3 targetPos, float speed)
  {
    if(_rb == null) return;

    Vector3 dir = targetPos - transform.position;

    if (dir.sqrMagnitude < 0.01f)
      return;

    dir.Normalize();

    Vector3 nextPosition =
      transform.position + dir * speed * Time.fixedDeltaTime;

    _rb.MovePosition(nextPosition);

    RotateTowards(targetPos);
  }

  protected void RotateTowards(Vector3 targetPos)
  {
    Vector3 dir = targetPos - transform.position;
    dir.y = 0;

    if (dir.sqrMagnitude < 0.01f)
      return;

    Quaternion targetRot = Quaternion.LookRotation(dir);
    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 8f * Time.deltaTime);
  }
}
