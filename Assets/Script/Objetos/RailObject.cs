using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
[RequireComponent(typeof(Collider))]
public class RailObject : MonoBehaviour
{
  [Header("⚙️ Configurações do Rail")]
  [Tooltip("Velocidade de slide exclusiva deste rail (m/s)")]
  [SerializeField]
  private float slideSpeed = 12f;

  [Tooltip("Direção do movimento ao longo do spline")]
  [SerializeField]
  private RailDirection direction = RailDirection.Forward;

  [Tooltip("Permite transicionar para NextRailCandidate ao pular")]
  [SerializeField]
  private bool canChain = true;

  [Tooltip("Raio máximo para detectar transição ao próximo rail")]
  [SerializeField]
  private float transitionRadius = 3f;

  [Header("🎯 Collider")]
  [Tooltip("Layer para detectar entrada do player")]
  [SerializeField]
  private LayerMask playerLayer;

  [Tooltip("Espessura do trigger ao redor do spline")]
  [SerializeField]
  private float triggerRadius = 1.5f;

  public float SlideSpeed => slideSpeed;
  public RailDirection Direction => direction;
  public bool CanChain => canChain;
  public float TransitionRadius => transitionRadius;

  private SplineContainer _spline;
  private Collider _triggerCollider;

  public enum RailDirection
  {
    Forward,
    Backward,
  }

  private void Awake()
  {
    _spline = GetComponent<SplineContainer>();

    _triggerCollider = GetComponent<Collider>();
    if (_triggerCollider == null)
    {
      var box = gameObject.AddComponent<BoxCollider>();
      box.isTrigger = true;
      _triggerCollider = box;
      AdjustTriggerBounds();
    }
    else
    {
      _triggerCollider.isTrigger = true;
    }
  }

  private void AdjustTriggerBounds()
  {
    if (_spline == null || _spline.Spline.Count == 0)
      return;

    Bounds bounds = new(_spline.Spline.EvaluatePosition(0), Vector3.zero);
    for (int i = 0; i < _spline.Spline.Count; i++)
    {
      bounds.Encapsulate(_spline.Spline.EvaluatePosition(i / (float)_spline.Spline.Count));
    }

    bounds.Expand(triggerRadius * 2f);

    if (_triggerCollider is BoxCollider box)
    {
      box.center = bounds.center - transform.position;
      box.size = bounds.size;
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    if (((1 << other.gameObject.layer) & playerLayer) == 0)
      return;

    Player player = other.GetComponent<Player>();
    if (player == null || player.CurrentRail != null)
      return;

    float3 localPlayerPos = _spline.transform.InverseTransformPoint(player.transform.position);
    SplineUtility.GetNearestPoint(_spline.Spline, localPlayerPos, out _, out float t);
    float3 tangentLocal = _spline.Spline.EvaluateTangent(t);
    Vector3 tangentWorld = _spline.transform.TransformDirection(tangentLocal);
    float angle = Vector3.Angle(tangentWorld, player.transform.forward);
    direction = angle > 90f ? RailDirection.Backward : RailDirection.Forward;

    player.CurrentRail = _spline;
    player.ActionLayer.PushState(player.RailSlide, player);
  }

  private void OnTriggerExit(Collider other)
  {
    if (((1 << other.gameObject.layer) & playerLayer) == 0)
      return;

    Player player = other.GetComponent<Player>();
    if (
      player != null
      && player.CurrentRail == _spline
      && player.ActionLayer.GetActive<PlayerActionStateRailSlide>() == null
    )
    {
      player.CurrentRail = null;
    }
  }

#if UNITY_EDITOR
  private void OnDrawGizmosSelected()
  {
    Gizmos.color = Color.cyan;
    Gizmos.DrawWireSphere(transform.position, transitionRadius);
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, triggerRadius);
  }
#endif
}
