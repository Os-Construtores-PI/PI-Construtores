using UnityEngine;

public class CameraTargetScanner : IScanner<Ray, (bool found, RaycastHit hit)>
{
  private const float TARGET_BUFFER = 2f;
  private const string TAG_PLAYER = "Player";

  private readonly RaycastHit[] _results = new RaycastHit[20];
  private readonly int _targetsMask;
  private readonly int _obstacleMask;
  private readonly float _sphereRadius;
  private readonly float _maxDistance;
  private readonly float _dotThreshold;

  public CameraTargetScanner(
    float sphereRadius = 8f,
    float maxDistance = 150f,
    float dotThreshold = 0.5f
  )
  {
    _sphereRadius = sphereRadius;
    _maxDistance = maxDistance;
    _dotThreshold = dotThreshold;
    _targetsMask = LayerMask.GetMask("Object", "Entity");
    _obstacleMask = LayerMask.GetMask("Default");
  }

  public (bool found, RaycastHit hit) Scan(Ray ray)
  {
    int hitCount = Physics.SphereCastNonAlloc(
      ray.origin,
      _sphereRadius,
      ray.direction,
      _results,
      _maxDistance,
      _targetsMask,
      QueryTriggerInteraction.Collide
    );

    if (!TryFindClosestPoint(ray, hitCount, out Vector3 targetPoint))
      return (false, default);

    return ConfirmTarget(ray.origin, targetPoint);
  }

  private bool TryFindClosestPoint(Ray ray, int hitCount, out Vector3 bestPoint)
  {
    bestPoint = default;
    bool found = false;
    float closestDistance = float.MaxValue;
    Vector3 forward = ray.direction.normalized;

    for (int i = 0; i < hitCount; i++)
    {
      Collider col = _results[i].collider;

      if (col.CompareTag(TAG_PLAYER))
        continue;

      if (!TryGetLockPoint(col, ray.origin, out Vector3 point))
        continue;

      Vector3 toTarget = point - ray.origin;

      if (Vector3.Dot(forward, toTarget.normalized) < _dotThreshold)
        continue;

      float distance = toTarget.magnitude;
      if (distance > _maxDistance || distance >= closestDistance)
        continue;

      if (Physics.Linecast(ray.origin, point, _obstacleMask, QueryTriggerInteraction.Ignore))
        continue;

      closestDistance = distance;
      bestPoint = point;
      found = true;
    }

    return found;
  }

  private (bool found, RaycastHit hit) ConfirmTarget(Vector3 origin, Vector3 targetPoint)
  {
    Vector3 direction = (targetPoint - origin).normalized;

    bool hitSomething = Physics.Raycast(
      origin,
      direction,
      out RaycastHit hit,
      _maxDistance + TARGET_BUFFER,
      _targetsMask | _obstacleMask,
      QueryTriggerInteraction.Collide
    );

    if (!hitSomething)
      return (false, default);

    bool isTargetLayer = (_targetsMask & (1 << hit.collider.gameObject.layer)) != 0;
    return isTargetLayer ? (true, hit) : (false, default);
  }

  private static bool TryGetLockPoint(Collider col, Vector3 reference, out Vector3 point)
  {
    point = default;

    if (!col.TryGetComponent(out ILockable lockable))
      return false;

    Vector3 lockPoint = lockable.GetLockOnPoint(reference);
    if (Vector3.Distance(reference, lockPoint) > lockable.LockRange)
      return false;

    point = lockPoint;
    return true;
  }
}

public class WallScanner : IScanner<Transform, RaycastHit?>
{
  private readonly int _mask;
  private readonly float _distance;

  public WallScanner(float distance = 5f, string layerName = "RunningWall")
  {
    _distance = distance;
    _mask = LayerMask.GetMask(layerName);
  }

  public RaycastHit? Scan(Transform origin)
  {
    Vector3 position = origin.position;

    if (
      Physics.Raycast(
        position,
        origin.right,
        out RaycastHit hit,
        _distance,
        _mask,
        QueryTriggerInteraction.Ignore
      )
    )
      return hit;

    if (
      Physics.Raycast(
        position,
        -origin.right,
        out hit,
        _distance,
        _mask,
        QueryTriggerInteraction.Ignore
      )
    )
      return hit;

    return null;
  }
}

public readonly struct RailScanQuery
{
  public readonly Vector3 Position;
  public readonly Vector3 MoveDirection;

  public RailScanQuery(Vector3 position, Vector3 moveDirection)
  {
    Position = position;
    MoveDirection = moveDirection;
  }
}

public class RailEntryScanner : IScanner<RailScanQuery, RailObject>
{
  private const float MIN_EFFECTIVE_RADIUS = 3f;
  private const float PROXIMITY_WEIGHT = 0.7f;
  private const float ALIGNMENT_WEIGHT = 0.3f;
  private const float CLOSE_DISTANCE = 1f;
  private const float CLOSE_BONUS = 10f;

  private readonly float _effectiveRadius;

  public RailEntryScanner(float entryRadius)
  {
    _effectiveRadius = Mathf.Max(entryRadius, MIN_EFFECTIVE_RADIUS);
  }

  public RailObject Scan(RailScanQuery query)
  {
    RailObject bestRail = null;
    float bestScore = -1f;

    foreach (RailObject rail in RailManager.Rails)
    {
      if (rail.IsOnCooldown)
        continue;

      if (!rail.GetNearestPointOnSpline(query.Position, out Vector3 nearestPoint, out _))
        continue;

      float distance = Vector3.Distance(query.Position, nearestPoint);
      if (distance > _effectiveRadius)
        continue;

      float score = CalculateScore(query, nearestPoint, distance);
      if (score <= bestScore)
        continue;

      bestScore = score;
      bestRail = rail;
    }

    return bestRail;
  }

  private float CalculateScore(RailScanQuery query, Vector3 nearestPoint, float distance)
  {
    Vector3 toRail = (nearestPoint - query.Position).normalized;
    float alignment = Vector3.Dot(toRail, query.MoveDirection);

    float proximityScore = 1f - (distance / _effectiveRadius);
    float alignmentScore = (alignment + 1f) * 0.5f;
    float score = proximityScore * PROXIMITY_WEIGHT + alignmentScore * ALIGNMENT_WEIGHT;

    return distance < CLOSE_DISTANCE ? score + CLOSE_BONUS : score;
  }
}

public class EnemyActivationScanner : IScanner<Vector3, bool>
{
  private readonly float _sqrRadius;

  public EnemyActivationScanner(float radius)
  {
    _sqrRadius = radius * radius;
  }

  public bool Scan(Vector3 playerPosition)
  {
    if (EnemySpawner.Instance == null)
      return false;

    int amount = EnemySpawner.Instance.GetAmountPool();

    for (int i = 0; i < amount; i++)
    {
      GameObject enemy = EnemySpawner.Instance.GetDisabledObject();
      if (enemy == null)
        continue;

      if ((enemy.transform.position - playerPosition).sqrMagnitude <= _sqrRadius)
        enemy.SetActive(true);
    }

    return true;
  }
}
