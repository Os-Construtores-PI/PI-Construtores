using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[DefaultExecutionOrder(-100)]
public class CameraDirector : MonoBehaviour
{
  private const float MinTangentSqr = 0.0001f;

  [Header("Trilho")]
  [SerializeField]
  private SplineContainer _cameraPath;

  [Tooltip("Marque se os jogadores andam no sentido contrário ao do spline.")]
  [SerializeField]
  private bool _flipDirection;

  [Tooltip("Distância extra que outro ramo precisa ganhar para a câmera trocar de spline.")]
  [SerializeField]
  private float _branchSwitchMargin = 1.5f;

  [Tooltip("Ignora a altura ao escolher o ramo, evitando troca indevida durante pulos.")]
  [SerializeField]
  private bool _ignoreHeightOnBranchSelection = true;

  [SerializeField]
  private int _nearestResolution = 16;

  [SerializeField]
  private int _nearestIterations = 4;

  [Header("Enquadramento")]
  [Tooltip("Deslocamento lateral em relação ao trilho.")]
  [SerializeField]
  private float _lateralOffset;

  [Tooltip("Altura da câmera acima do jogador.")]
  [SerializeField]
  private float _heightOffset = 2f;

  [Tooltip("Distância ao longo do trilho. Negativo = atrás do jogador.")]
  [SerializeField]
  private float _railDistanceOffset = -8f;

  [SerializeField]
  private float _pitch = 10f;

  [SerializeField]
  private float _yaw;

  [Header("Suavização")]
  [SerializeField]
  private float _railSmoothTime = 0.15f;

  [Tooltip("Máximo de metros que a câmera pode ficar atrás ou à frente do jogador no trilho.")]
  [SerializeField]
  private float _maxRailLag = 3f;

  [SerializeField]
  private float _transitionSmoothTime = 0.35f;

  [Tooltip("Máximo que a câmera pode se afastar do trilho durante a troca de ramo.")]
  [SerializeField]
  private float _maxTransitionDeviation = 2f;

  [SerializeField]
  private float _verticalSmoothTime = 0.25f;

  [Tooltip("Variação de altura do jogador ignorada antes da câmera começar a seguir.")]
  [SerializeField]
  private float _verticalDeadZone = 0.5f;

  [SerializeField]
  private float _rotationSpeed = 6f;

  private class CameraEntry
  {
    public Transform Target;
    public Transform Rig;
    public int SplineIndex = -1;
    public float Distance;
    public float DistanceVelocity;
    public Vector3 TransitionOffset;
    public Vector3 TransitionVelocity;
    public Vector3 LastRailPosition;
    public Vector3 LastForward = Vector3.forward;
    public float Height;
    public float HeightVelocity;
    public bool Initialized;
  }

  private readonly List<CameraEntry> _entries = new();

  public void Register(Transform target, Transform rig)
  {
    if (target == null || rig == null)
      return;

    if (_entries.Exists(e => e.Target == target))
      return;

    _entries.Add(new CameraEntry { Target = target, Rig = rig });
  }

  public void Register(Transform target, Camera camera)
  {
    if (camera != null)
      Register(target, camera.transform);
  }

  public void Unregister(Transform target)
  {
    _entries.RemoveAll(e => e.Target == target);
  }

  public void SetPath(SplineContainer path)
  {
    _cameraPath = path;
    foreach (CameraEntry entry in _entries)
    {
      entry.SplineIndex = -1;
      entry.Initialized = false;
    }
  }

  private void LateUpdate()
  {
    if (_cameraPath == null || _cameraPath.Splines.Count == 0)
      return;

    for (int i = _entries.Count - 1; i >= 0; i--)
    {
      CameraEntry entry = _entries[i];

      if (entry.Target == null || entry.Rig == null)
      {
        _entries.RemoveAt(i);
        continue;
      }

      UpdateEntry(entry);
    }
  }

  /*
   * Projeta o alvo no trilho, avança a posição do trilho com suavização em metros,
   * posiciona o rig sobre o trilho (com deslocamento lateral) e deixa só a altura livre.
   */
  private void UpdateEntry(CameraEntry entry)
  {
    Vector3 targetPosition = entry.Target.position;
    FindNearest(targetPosition, entry.SplineIndex, out int index, out float targetT);

    Spline spline = _cameraPath.Splines[index];
    float length = Mathf.Max(spline.GetLength(), 0.001f);
    float targetDistance = targetT * length;

    bool switched = entry.Initialized && index != entry.SplineIndex;

    if (!entry.Initialized || switched)
    {
      entry.SplineIndex = index;
      entry.Distance = targetDistance;
      entry.DistanceVelocity = 0f;
    }
    else
    {
      AdvanceAlongRail(entry, spline, length, targetDistance);
    }

    float alongOffset = _flipDirection ? -_railDistanceOffset : _railDistanceOffset;
    EvaluateRail(
      spline,
      length,
      entry.Distance + alongOffset,
      entry.LastForward,
      out Vector3 railPosition,
      out Vector3 forward
    );
    entry.LastForward = forward;

    if (switched)
    {
      Vector3 jump = entry.LastRailPosition - railPosition;
      jump.y = 0f;
      entry.TransitionOffset = Vector3.ClampMagnitude(jump, _maxTransitionDeviation);
      entry.TransitionVelocity = Vector3.zero;
    }
    else
    {
      entry.TransitionOffset = Vector3.SmoothDamp(
        entry.TransitionOffset,
        Vector3.zero,
        ref entry.TransitionVelocity,
        _transitionSmoothTime
      );
    }

    entry.LastRailPosition = railPosition + entry.TransitionOffset;

    Vector3 right = Vector3.Cross(Vector3.up, forward);
    Vector3 flatPosition = entry.LastRailPosition + right * _lateralOffset;

    float height = UpdateHeight(entry, targetPosition.y + _heightOffset);

    Quaternion desiredRotation =
      Quaternion.LookRotation(forward, Vector3.up) * Quaternion.Euler(_pitch, _yaw, 0f);
    Vector3 finalPosition = new(flatPosition.x, height, flatPosition.z);

    if (!entry.Initialized)
    {
      entry.Rig.SetPositionAndRotation(finalPosition, desiredRotation);
      entry.Initialized = true;
      return;
    }

    float rotationBlend = 1f - Mathf.Exp(-_rotationSpeed * Time.deltaTime);
    entry.Rig.SetPositionAndRotation(
      finalPosition,
      Quaternion.Slerp(entry.Rig.rotation, desiredRotation, rotationBlend)
    );
  }

  /*
   * Suaviza a distância percorrida no spline (em metros). Em splines fechados usa o
   * menor caminho, e limita o atraso máximo da câmera em relação ao alvo.
   */
  private void AdvanceAlongRail(CameraEntry entry, Spline spline, float length, float targetDistance)
  {
    float delta = WrapDelta(targetDistance - entry.Distance, length, spline.Closed);
    float step = Mathf.SmoothDamp(0f, delta, ref entry.DistanceVelocity, _railSmoothTime);

    float remaining = delta - step;
    if (Mathf.Abs(remaining) > _maxRailLag)
      step = delta - Mathf.Sign(remaining) * _maxRailLag;

    entry.Distance = NormalizeDistance(entry.Distance + step, length, spline.Closed);
  }

  /*
   * Altura livre com zona morta: pequenas variações do jogador são ignoradas,
   * e a câmera só acompanha o excedente.
   */
  private float UpdateHeight(CameraEntry entry, float desiredHeight)
  {
    if (!entry.Initialized)
    {
      entry.Height = desiredHeight;
      entry.HeightVelocity = 0f;
      return entry.Height;
    }

    float diff = desiredHeight - entry.Height;
    float adjusted =
      Mathf.Abs(diff) <= _verticalDeadZone
        ? entry.Height
        : desiredHeight - Mathf.Sign(diff) * _verticalDeadZone;

    entry.Height = Mathf.SmoothDamp(
      entry.Height,
      adjusted,
      ref entry.HeightVelocity,
      _verticalSmoothTime
    );
    return entry.Height;
  }

  /*
   * Avalia o spline em espaço local e converte para o mundo uma única vez.
   * Se a tangente for degenerada, mantém a última direção válida.
   */
  private void EvaluateRail(
    Spline spline,
    float length,
    float distance,
    Vector3 fallbackForward,
    out Vector3 position,
    out Vector3 forward
  )
  {
    float normalized = NormalizeDistance(distance, length, spline.Closed) / length;
    SplineUtility.Evaluate(spline, normalized, out float3 localPoint, out float3 localTangent, out float3 _);

    Transform pathTransform = _cameraPath.transform;
    position = pathTransform.TransformPoint(localPoint);

    Vector3 tangent = pathTransform.TransformDirection(localTangent);
    tangent.y = 0f;

    if (tangent.sqrMagnitude < MinTangentSqr)
    {
      forward = fallbackForward;
      return;
    }

    tangent.Normalize();
    forward = _flipDirection ? -tangent : tangent;
  }

  /*
   * Procura o spline mais próximo do alvo. O ramo atual só é trocado se outro
   * estiver mais perto por _branchSwitchMargin, evitando oscilação nas bifurcações.
   */
  private void FindNearest(Vector3 worldPoint, int currentIndex, out int bestIndex, out float bestT)
  {
    Transform pathTransform = _cameraPath.transform;
    float3 local = pathTransform.InverseTransformPoint(worldPoint);
    IReadOnlyList<Spline> splines = _cameraPath.Splines;

    if (currentIndex >= splines.Count)
      currentIndex = -1;

    bestIndex = 0;
    bestT = 0f;
    float bestDistance = float.MaxValue;

    float currentDistance = float.MaxValue;
    float currentT = 0f;

    for (int i = 0; i < splines.Count; i++)
    {
      SplineUtility.GetNearestPoint(
        splines[i],
        local,
        out float3 nearest,
        out float t,
        _nearestResolution,
        _nearestIterations
      );

      Vector3 offset = pathTransform.TransformPoint(nearest) - worldPoint;
      if (_ignoreHeightOnBranchSelection)
        offset.y = 0f;

      float distance = offset.magnitude;

      if (i == currentIndex)
      {
        currentDistance = distance;
        currentT = t;
      }

      if (distance < bestDistance)
      {
        bestDistance = distance;
        bestIndex = i;
        bestT = t;
      }
    }

    if (currentIndex >= 0 && currentIndex != bestIndex)
    {
      if (currentDistance <= bestDistance + _branchSwitchMargin)
      {
        bestIndex = currentIndex;
        bestT = currentT;
      }
    }
  }

  private static float WrapDelta(float delta, float length, bool closed)
  {
    if (!closed)
      return delta;

    float half = length * 0.5f;
    return Mathf.Repeat(delta + half, length) - half;
  }

  private static float NormalizeDistance(float distance, float length, bool closed) =>
    closed ? Mathf.Repeat(distance, length) : Mathf.Clamp(distance, 0f, length);
}
