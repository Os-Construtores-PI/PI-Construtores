using System.Threading;
using System.Threading.Tasks; // apenas se precisar em outro lugar
using UnityEngine;

public class EyeWolf : MonoBehaviour
{
  [Header("Config do Campo de Visão")]
  public float _visionRange = 10f;
  public float _visionAngle = 120f;

  [Header("Performance")]
  [SerializeField, Tooltip("Intervalo em segundos entre cada scan de visão")]
  private float _scanInterval = 0.2f;

  [Header("Camadas de Detecção")]
  public LayerMask _targetMask;
  public LayerMask _obstacleMask;

  [Header("Debug")]
  [HideInInspector]
  public bool FoundPlayer;

  [HideInInspector]
  public Transform DetectedPlayer;

  private Transform target;
  private CancellationTokenSource _scanCts;

  private void Start()
  {
    GameObject playerObj = GameObject.FindGameObjectWithTag("PlayersHolder");
    if (playerObj != null)
      target = playerObj.transform;
    else
      Debug.LogWarning("Player não encontrado! Verifique a Tag do Player");
  }

  // Método de ciclo de vida assíncrono nativo do Unity 6.
  // Substitui o Update() — roda em loop próprio, sem sobrecarregar por frame.
  private async Awaitable OnEnable()
  {
    _scanCts = new CancellationTokenSource();
    var token = _scanCts.Token;

    try
    {
      while (!token.IsCancellationRequested)
      {
        ProcurarAlvos();
        await Awaitable.WaitForSecondsAsync(_scanInterval, token);
      }
    }
    catch (System.OperationCanceledException)
    {
      // Esperado quando o objeto é desativado/destruído — não é erro.
    }
  }

  private void OnDisable()
  {
    _scanCts?.Cancel();
    _scanCts?.Dispose();
    _scanCts = null;
  }

  public void ProcurarAlvos()
  {
    FoundPlayer = false;
    DetectedPlayer = null;

    Collider[] targetsInArea = Physics.OverlapSphere(transform.position, _visionRange, _targetMask);
    foreach (var col in targetsInArea)
    {
      Transform t = col.transform;
      if (!t.CompareTag(Constants.Tags.Player.ToString()))
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
    Vector3 dirToTarget = (target.position + Vector3.up * 1.5f - transform.position).normalized;
    float dist = Vector3.Distance(transform.position, target.position);

    if (Vector3.Angle(transform.forward, dirToTarget) < _visionAngle / 2)
    {
      if (!Physics.Raycast(transform.position, dirToTarget, dist, _obstacleMask))
      {
        return true;
      }
    }
    return false;
  }

  private void OnDrawGizmos()
  {
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, _visionRange);

    Vector3 angleA = DirecaodoAngulo(-_visionAngle / 2);
    Vector3 angleB = DirecaodoAngulo(_visionAngle / 2);

    if (FoundPlayer && DetectedPlayer != null)
    {
      Gizmos.color = Color.red;
      Gizmos.DrawLine(transform.position, DetectedPlayer.position);
    }
  }

  private Vector3 DirecaodoAngulo(float anguloemGraus)
  {
    float rad = (anguloemGraus + transform.eulerAngles.y) * Mathf.Deg2Rad;
    return new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
  }

  public void SetTarget(Transform t)
  {
    DetectedPlayer = t;
    FoundPlayer = t != null;
  }
}
