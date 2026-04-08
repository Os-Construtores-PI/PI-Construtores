using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class BezierPath : MonoBehaviour
{
  [System.Serializable]
  public class Waypoint
  {
    public string label = "WP";
    public Vector3 position;
    public Vector3 tangentIn;   // Handle de entrada
    public Vector3 tangentOut;  // Handle de saída
    public bool linked = true;  // Tangentes espelhadas

    public Waypoint(Vector3 pos, Vector3 tIn, Vector3 tOut, string lbl = "WP")
    {
      position = pos;
      tangentIn = tIn;
      tangentOut = tOut;
      label = lbl;
    }
  }

  [Header("Waypoints")]
  public List<Waypoint> waypoints = new();

  [Header("DOTween Settings")]
  public float duration = 3f;
  public Ease easeType = Ease.InOutSine;
  public bool playOnStart = false;
  public PathMode pathMode = PathMode.Full3D;
  public int pathResolution = 10;

  [Header("Gizmos")]
  public Color pathColor = new Color(0.2f, 0.9f, 0.5f, 1f);
  public Color waypointColor = new Color(0.2f, 0.7f, 1f, 1f);
  public Color tangentColor = new Color(1f, 0.6f, 0.1f, 1f);
  public float handleSize = 0.15f;

  // ── Inicialização ──────────────────────────────────────────────

  void Start()
  {
    if (playOnStart && Application.isPlaying)
      Play();
  }

  // ── API pública ────────────────────────────────────────────────

  /// <summary>Anima o objeto pelo path usando DOPath (CubicBezier).</summary>
  public Tween Play()
  {
    Vector3[] pts = BuildDOTweenArray();
    if (pts == null || pts.Length < 4) return null;

    return transform
        .DOPath(pts, duration, PathType.CubicBezier, pathMode, pathResolution)
        .SetEase(easeType);
  }

  /// <summary>
  /// Monta o array no formato DOPath CubicBezier:
  /// [ WP0, TangentOut0, TangentIn1, WP1, TangentOut1, TangentIn2, WP2 ... ]
  /// O primeiro ponto é a posição ATUAL do transform (não entra no array —
  /// DOPath parte da posição corrente), então começamos a partir do WP0.
  /// </summary>
  public Vector3[] BuildDOTweenArray()
  {
    if (waypoints.Count < 1) return null;

    // DOPath CubicBezier espera:
    //   stash (tangentOut do ponto de partida) já vem do transform pai.
    //   Array = [ TangentOut_origin, TangentIn_WP0, WP0,
    //             TangentOut_WP0,    TangentIn_WP1, WP1, ... ]
    //
    // Como DOPath parte da posição atual do transform, o array começa
    // com o tangentOut da origem (posição do transform).

    var list = new List<Vector3>();

    // TangentOut da origem: puxado do primeiro waypoint "de trás pra frente"
    // (usa tangentIn do WP0 espelhado em relação à origem)
    Vector3 origin = transform.position;
    Vector3 firstWP = waypoints[0].position;
    Vector3 originTangentOut = origin + (firstWP - waypoints[0].tangentIn);

    list.Add(originTangentOut);           // TangentOut da origem

    for (int i = 0; i < waypoints.Count; i++)
    {
      Waypoint wp = waypoints[i];
      list.Add(wp.tangentIn);           // TangentIn do WP atual
      list.Add(wp.position);            // Waypoint

      if (i < waypoints.Count - 1)
        list.Add(wp.tangentOut);      // TangentOut (só se não for o último)
    }

    return list.ToArray();
  }

  // ── Editor: adicionar / remover waypoints ──────────────────────

  public void AddWaypoint()
  {
    Vector3 basePos;
    Vector3 offset = new Vector3(2f, 0f, 0f);

    if (waypoints.Count == 0)
      basePos = transform.position + offset;
    else
      basePos = waypoints[^1].position + offset;

    var wp = new Waypoint(
        pos: basePos,
        tIn: basePos + new Vector3(-1f, 0.5f, 0f),
        tOut: basePos + new Vector3(1f, -0.5f, 0f),
        lbl: $"WP{waypoints.Count}"
    );

    waypoints.Add(wp);

#if UNITY_EDITOR
    EditorUtility.SetDirty(this);
#endif
  }

  public void RemoveLastWaypoint()
  {
    if (waypoints.Count == 0) return;
    waypoints.RemoveAt(waypoints.Count - 1);

#if UNITY_EDITOR
    EditorUtility.SetDirty(this);
#endif
  }

  public void ClearWaypoints()
  {
    waypoints.Clear();

#if UNITY_EDITOR
    EditorUtility.SetDirty(this);
#endif
  }

  // ── Gizmos ─────────────────────────────────────────────────────

#if UNITY_EDITOR
  void OnDrawGizmos()
  {
    if (waypoints == null || waypoints.Count == 0) return;

    Vector3 origin = transform.position;
    Vector3 originTangentOut = waypoints.Count > 0
        ? origin + (waypoints[0].position - waypoints[0].tangentIn)
        : origin + Vector3.right;

    // Linha de tangente da origem
    Gizmos.color = tangentColor * 0.6f;
    Gizmos.DrawLine(origin, originTangentOut);
    Gizmos.DrawSphere(originTangentOut, handleSize * 0.6f);

    // Waypoints e tangentes
    for (int i = 0; i < waypoints.Count; i++)
    {
      Waypoint wp = waypoints[i];

      // Linhas das tangentes
      Gizmos.color = tangentColor * 0.7f;
      Gizmos.DrawLine(wp.position, wp.tangentIn);
      if (i < waypoints.Count - 1)
        Gizmos.DrawLine(wp.position, wp.tangentOut);

      // Handle tangentIn
      Gizmos.color = tangentColor;
      Gizmos.DrawSphere(wp.tangentIn, handleSize * 0.6f);

      // Handle tangentOut
      if (i < waypoints.Count - 1)
        Gizmos.DrawSphere(wp.tangentOut, handleSize * 0.6f);

      // Waypoint principal
      Gizmos.color = waypointColor;
      Gizmos.DrawSphere(wp.position, handleSize);

      // Label
      Handles.Label(wp.position + Vector3.up * 0.3f, wp.label);
    }

    // Curva Bezier desenhada por segmento
    Gizmos.color = pathColor;
    Vector3 p0 = origin;
    Vector3 c0 = originTangentOut;

    for (int i = 0; i < waypoints.Count; i++)
    {
      Waypoint wp = waypoints[i];
      Vector3 p1 = wp.position;
      Vector3 c1 = wp.tangentIn;

      DrawCubicBezierGizmo(p0, c0, c1, p1, 30);

      p0 = p1;
      c0 = wp.tangentOut;
    }
  }

  static void DrawCubicBezierGizmo(Vector3 p0, Vector3 c0, Vector3 c1, Vector3 p1, int steps)
  {
    Vector3 prev = p0;
    for (int s = 1; s <= steps; s++)
    {
      float t = s / (float)steps;
      float u = 1f - t;
      Vector3 pt = u * u * u * p0 + 3 * u * u * t * c0 + 3 * u * t * t * c1 + t * t * t * p1;
      Gizmos.DrawLine(prev, pt);
      prev = pt;
    }
  }
#endif
}
