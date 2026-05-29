using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class RailObject : MonoBehaviour
{
  [Header("Configurações do Rail")]
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

  [Header("Collider")]
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

  public void SetDirection(RailDirection dir) => direction = dir;

  private SplineContainer _spline;
  private static readonly List<RailObject> _allRails = new();
  private RailObject _cachedNextCandidate;
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
    if (_triggerCollider == null || _triggerCollider is MeshCollider)
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

    _allRails.RemoveAll(r => r == null);
    _allRails.Add(this);
  }

  private void Start()
  {
    BakeNeighbors(_allRails.ToArray());
  }

  private void AdjustTriggerBounds()
  {
    if (_spline == null || _spline.Spline.Count == 0)
      return;

    const int samples = 20;
    Bounds bounds = new(
      _spline.transform.InverseTransformPoint(
        _spline.transform.TransformPoint(_spline.Spline.EvaluatePosition(0f))
      ),
      Vector3.zero
    );

    for (int i = 1; i <= samples; i++)
    {
      float t = i / (float)samples;
      float3 localPos = _spline.Spline.EvaluatePosition(t);
      bounds.Encapsulate(localPos);
    }

    bounds.Expand(triggerRadius * 2f);

    if (_triggerCollider is BoxCollider box)
    {
      box.center = bounds.center; // já está em local space
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

    player.NextRailCanditate = _cachedNextCandidate?.GetComponent<SplineContainer>();

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
      player.NextRailCanditate = null;
    }
  }

  public void BakeNeighbors(RailObject[] allRails)
  {
    _cachedNextCandidate = null;
    float bestScore = float.MaxValue;

    foreach (var rail in allRails)
    {
      if (rail == null)
        continue; // referência morta do Editor
      if (rail == this)
        continue;
      if (!rail.CanChain)
        continue;

      float dist = Vector3.Distance(transform.position, rail.transform.position);
      if (dist > transitionRadius)
        continue;

      float score = ScoreCandidate(rail);
      if (score < bestScore)
      {
        bestScore = score;
        _cachedNextCandidate = rail;
      }
    }
  }

  private float ScoreCandidate(RailObject other)
  {
    var spline = GetComponent<SplineContainer>().Spline;
    var otherSpline = other.GetComponent<SplineContainer>().Spline;

    Vector3 exitPoint = transform.TransformPoint(
      spline.EvaluatePosition(direction == RailDirection.Forward ? 1f : 0f)
    );
    Vector3 entryA = other.transform.TransformPoint(otherSpline.EvaluatePosition(0f));
    Vector3 entryB = other.transform.TransformPoint(otherSpline.EvaluatePosition(1f));

    return Mathf.Min(Vector3.Distance(exitPoint, entryA), Vector3.Distance(exitPoint, entryB));
  }

  public bool TryAttachPlayer(Player player)
  {
    if (player.CurrentRail == _spline)
    {
      Debug.LogWarning($"[RailObject] Player already attached to THIS rail: {name}");
      return false;
    }

    float3 localPlayerPos = _spline.transform.InverseTransformPoint(player.transform.position);

    SplineUtility.GetNearestPoint(_spline.Spline, localPlayerPos, out float3 pos, out float t);

    float snapThreshold = 2f; // metros
    if (
      Vector3.Distance(player.transform.position, _spline.transform.TransformPoint(pos))
      > snapThreshold
    )
    {
      player.transform.position = _spline.transform.TransformPoint(pos);
    }

    float3 tangentLocal = _spline.Spline.EvaluateTangent(t);
    Vector3 tangentWorld = _spline.transform.TransformDirection(tangentLocal);

    if (tangentWorld.sqrMagnitude < 0.01f)
    {
      Debug.LogWarning($"[RailObject] Zero tangent at t={t} for {name}");
      return false;
    }

    float angle = Vector3.Angle(tangentWorld, player.transform.forward);
    direction = angle > 90f ? RailDirection.Backward : RailDirection.Forward;

    player.NextRailCanditate = _cachedNextCandidate?.GetComponent<SplineContainer>();

    player.CurrentRail = _spline;

#if UNITY_EDITOR
    Debug.Log($"[RailObject] Attached player to {name} | t={t:F2} | dir={direction}");
#endif
    return true;
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
