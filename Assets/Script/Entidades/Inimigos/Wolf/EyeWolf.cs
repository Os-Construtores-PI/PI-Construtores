using System.Threading;
using UnityEngine;

public class EyeWolf : MonoBehaviour
{
  [Header("Config do Campo de Visão")]
  public float _visionRange = 10f;
  public float _visionAngle = 120f;

  [Header("Performance")]
  [SerializeField, Tooltip("Intervalo em segundos entre cada scan de visão")]
  private float _scanInterval = 0.2f;

  [SerializeField, Tooltip("Tamanho do buffer usado no OverlapSphere (evita alocação de GC)")]
  private int _maxTargets = 8;

  [Header("Camadas de Detecção")]
  public LayerMask _targetMask;
  public LayerMask _obstacleMask;

  [Header("Debug")]
  [HideInInspector]
  public bool FoundPlayer;

  [HideInInspector]
  public Transform DetectedPlayer;

  private string _playerTag;
  private Collider[] _overlapBuffer;
  private CancellationTokenSource _scanCts;

  public void Awake()
  {
    _playerTag = Constants.Tags.Player.ToString();
    _overlapBuffer = new Collider[_maxTargets];
  }

  public async Awaitable OnEnable()
  {
    _scanCts = new CancellationTokenSource();
    var token = _scanCts.Token;
    try
    {
      while (!token.IsCancellationRequested)
      {
        FindTargets();
        await Awaitable.WaitForSecondsAsync(_scanInterval, token);
      }
    }
    catch (System.OperationCanceledException) { }
  }

  public void OnDisable()
  {
    _scanCts?.Cancel();
    _scanCts?.Dispose();
    _scanCts = null;
  }

  public void FindTargets()
  {
    FoundPlayer = false;
    DetectedPlayer = null;

    int count = Physics.OverlapSphereNonAlloc(
      transform.position,
      _visionRange,
      _overlapBuffer,
      _targetMask
    );
    for (int i = 0; i < count; i++)
    {
      Transform t = _overlapBuffer[i].transform;
      if (!t.CompareTag(_playerTag))
        continue;

      if (CanSeeTarget(t))
      {
        FoundPlayer = true;
        DetectedPlayer = t;
        return;
      }
    }
  }

  public bool CanSeeTarget(Transform target)
  {
    Vector3 toTarget = target.position + Vector3.up * 1.5f - transform.position;
    float dist = toTarget.magnitude;
    Vector3 dirToTarget = toTarget / dist;

    if (Vector3.Angle(transform.forward, dirToTarget) < _visionAngle / 2f)
    {
      if (!Physics.Raycast(transform.position, dirToTarget, dist, _obstacleMask))
        return true;
    }
    return false;
  }

  public void SetTarget(Transform t)
  {
    DetectedPlayer = t;
    FoundPlayer = t != null;
  }

  public void OnDrawGizmos()
  {
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, _visionRange);

    Vector3 angleA = AngleDirection(-_visionAngle / 2f);
    Vector3 angleB = AngleDirection(_visionAngle / 2f);
    Gizmos.DrawLine(transform.position, transform.position + angleA * _visionRange);
    Gizmos.DrawLine(transform.position, transform.position + angleB * _visionRange);

    if (FoundPlayer && DetectedPlayer != null)
    {
      Gizmos.color = Color.red;
      Gizmos.DrawLine(transform.position, DetectedPlayer.position);
    }
  }

  private Vector3 AngleDirection(float eulerAngle)
  {
    float rad = (eulerAngle + transform.eulerAngles.y) * Mathf.Deg2Rad;
    return new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
  }
}
