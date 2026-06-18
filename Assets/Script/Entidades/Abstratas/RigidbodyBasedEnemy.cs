using UnityEngine;

public class RigidbodyBasedEnemy : Enemies
{
  public override void Awake()
  {
    base.Awake();
    _rb ??= GetComponent<Rigidbody>();
  }

  protected override void TriggerKnockback()
  {
    int quantity = Physics.OverlapSphereNonAlloc(
      transform.position,
      _knockbackRadius,
      knockbackResult,
      LayerMask.GetMask("Entity", "Player"),
      QueryTriggerInteraction.Collide
    );

    for (int i = 0; i < quantity; i++)
    {
      Collider hit = knockbackResult[i];
      if (!hit.CompareTag(Constants.Tags.Player.ToString()))
        continue;

      Vector3 forceDir = (transform.position - hit.transform.position).normalized;
      forceDir.y *= _verticalMultiplier;

      _rb.AddForce(forceDir.normalized * _knockbackForce, ForceMode.Impulse);
      return;
    }
  }

  protected void MoveWithRigidbody(Vector3 targetPos, float speed)
  {
    if (_rb == null)
      return;

    Vector3 dir = (targetPos - transform.position).normalized;
    dir.y = 0;

    _rb.MovePosition(transform.position + speed * Time.fixedDeltaTime * dir);
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
