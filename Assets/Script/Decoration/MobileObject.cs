using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Core.PathCore;
using UnityEngine;

public class MobileObject : MonoBehaviour // Alterado para MonoBehaviour para exemplo, mantenha sua base se preferir
{
  private readonly List<Vector3> _targets = new();

  [Header("Configurações de Path")]
  [SerializeField, Tooltip("Não usar Bezier, têm uma classe própria pra isso.")]
  PathType _pathType = PathType.Linear;

  [SerializeField]
  PathMode _pathMode = PathMode.Full3D;

  [SerializeField]
  Ease _animationType = Ease.Linear;

  [SerializeField]
  LoopType _loopType = LoopType.Yoyo;

  [SerializeField]
  int _pathResolution = 10;

  [SerializeField]
  Color _gizmoColor = Color.white;

  [Header("Animação")]
  [SerializeField]
  float _duration = 4;

  [SerializeField]
  int _numOfLoops = -1;

  [SerializeField]
  UpdateType _updateType = UpdateType.Fixed;

  [SerializeField]
  bool _willRotate = false;

  [SerializeField]
  bool _independent = false;

  public void Start()
  {
    InitTargets();

    if (_targets.Count > 0)
    {
      var tween = transform
        .DOPath(_targets.ToArray(), _duration, _pathType, _pathMode, _pathResolution, _gizmoColor)
        .SetLoops(_numOfLoops, _loopType)
        .SetEase(_animationType)
        .SetUpdate(_updateType, _independent);

      if (_willRotate)
        tween.SetLookAt(0.01f, forwardDirection: transform.forward);
    }
  }

  void InitTargets()
  {
    _targets.Clear();

    List<Transform> mainPoints = new();
    foreach (Transform child in transform)
    {
      if (child.name.ToLower().Contains("target"))
      {
        mainPoints.Add(child); // Adiciona cada posição de filho na lista
      }
    }

    if (mainPoints.Count == 0)
      return;

    Transform tangentStash = transform
      .Cast<Transform>()
      .FirstOrDefault(t => t.name.ToLower().Contains("tangentout"));

    for (int i = 0; i < mainPoints.Count; i++)
    {
      Transform currentPoint = mainPoints[i];

      if (_pathType == PathType.CubicBezier)
      {
        if (i == 0)
        {
          // WP0: stash(A) → tangentIn(B) → WP0
          if (tangentStash != null)
            _targets.Add(tangentStash.position); // A: tangentOut do Transform atual

          Transform tangentIn = currentPoint
            .Cast<Transform>()
            .FirstOrDefault(t => t.name.ToLower().Contains("tangentin"));

          if (tangentIn != null)
            _targets.Add(tangentIn.position); // B

          _targets.Add(currentPoint.position); // WP0

          // Guarda tangentOut do WP0 (ponto C) pro próximo
          Transform tangentOut = currentPoint
            .Cast<Transform>()
            .FirstOrDefault(t => t.name.ToLower().Contains("tangentout"));

          tangentStash = tangentOut;
          continue;
        }

        // Demais pontos: stash(C) → tangentIn(D) → WP1...
        if (tangentStash != null)
          _targets.Add(tangentStash.position);

        Transform tIn = currentPoint
          .Cast<Transform>()
          .FirstOrDefault(t => t.name.ToLower().Contains("tangentin"));

        if (tIn != null)
          _targets.Add(tIn.position);

        _targets.Add(currentPoint.position);

        if (i < mainPoints.Count - 1)
        {
          Transform tOut = currentPoint
            .Cast<Transform>()
            .FirstOrDefault(t => t.name.ToLower().Contains("tangentout"));

          tangentStash = tOut;
        }

        continue;
      }

      // Fallback linear
      _targets.Add(currentPoint.position);
    }
  }

  public void OnDrawGizmos()
  {
    if (_targets == null || _targets.Count < 2)
      return;

    Gizmos.color = _gizmoColor;

    for (int i = 0; i < _targets.Count; i++)
    {
      // Desenha as esferas nos pontos
      Gizmos.DrawSphere(_targets[i], 0.15f);

      // Desenha linhas conectando os pontos para debug visual do array
      if (i < _targets.Count - 1)
      {
        Gizmos.DrawLine(_targets[i], _targets[i + 1]);
      }
    }
  }
}
